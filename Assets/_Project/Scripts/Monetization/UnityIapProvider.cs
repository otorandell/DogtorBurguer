// Real store body over Unity In-App Purchasing 5.x (com.unity.purchasing 5.4.2 — IAP 4 left
// support in June 2026). Unity defines ENABLE_CLOUD_SERVICES_PURCHASING for game assemblies while
// the package is installed; without it this file compiles to nothing and IapManager falls back to
// MockIapProvider, so a checkout without the package still builds.
#if ENABLE_CLOUD_SERVICES_PURCHASING
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace DogtorBurguer
{
    /// <summary>
    /// <see cref="IIapProvider"/> over Unity IAP 5: connects the StoreController, fetches the
    /// catalog, and grants ONLY from <see cref="HandlePurchasePending"/> — the store raises that
    /// for fresh purchases AND for replayed/restored ones (FetchPurchases at init, Restore on
    /// iOS), so the same path covers Remove Ads coming back after a reinstall. Each pending order
    /// is confirmed back to the store after granting (unconfirmed orders are replayed next
    /// launch — that's the crash-safety net). Receipt validation hook: <see cref="IsOrderValid"/>
    /// (needs the editor-generated tangle classes — pre-launch checklist).
    /// </summary>
    public class UnityIapProvider : MonoBehaviour, IIapProvider
    {
        private const float CONNECT_RETRY_DELAY = 15f;

        private StoreController _controller;
        private Action<string> _onGranted;
        private Action<IapResult> _pending;
        private List<ProductDefinition> _catalog;
        private bool _productsFetched;

        public bool IsInitialized => _controller != null && _productsFetched;

        public void Initialize(IReadOnlyList<(string StoreId, bool Consumable)> products, Action<string> onGranted)
        {
            _onGranted = onGranted;
            _catalog = new List<ProductDefinition>();
            foreach ((string storeId, bool consumable) in products)
                _catalog.Add(new ProductDefinition(storeId, consumable ? ProductType.Consumable : ProductType.NonConsumable));

            _controller = UnityIAPServices.StoreController();
            _controller.OnStoreConnected += HandleStoreConnected;
            _controller.OnProductsFetched += HandleProductsFetched;
            _controller.OnProductsFetchFailed += HandleProductsFetchFailed;
            _controller.OnPurchasePending += HandlePurchasePending;
            _controller.OnPurchaseFailed += HandlePurchaseFailed;
            _controller.ProcessPendingOrdersOnPurchasesFetched(true);

            Connect();
        }

        private async void Connect()
        {
            try
            {
                await _controller.Connect();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[UnityIapProvider] Store connect failed ({e.Message}), retrying in {CONNECT_RETRY_DELAY}s");
                StartCoroutine(RetryConnectRoutine());
            }
        }

        private IEnumerator RetryConnectRoutine()
        {
            yield return new WaitForSecondsRealtime(CONNECT_RETRY_DELAY);
            Connect();
        }

        private void HandleStoreConnected()
        {
            Debug.Log("[UnityIapProvider] Store connected, fetching products.");
            _controller.FetchProducts(_catalog);
        }

        private void HandleProductsFetched(List<Product> products)
        {
            _productsFetched = true;
            Debug.Log($"[UnityIapProvider] {products.Count} products fetched.");
            // Replays owned non-consumables / unfinished orders through OnPurchasePending.
            _controller.FetchPurchases();
        }

        private void HandleProductsFetchFailed(ProductFetchFailed failure)
        {
            Debug.LogWarning($"[UnityIapProvider] Product fetch failed: {failure.FailureReason}");
        }

        public string LocalizedPrice(string storeId)
        {
            if (!IsInitialized) return null;
            Product product = _controller.GetProductById(storeId);
            return product != null && product.availableToPurchase ? product.metadata.localizedPriceString : null;
        }

        public void Purchase(string storeId, Action<IapResult> onResult)
        {
            if (!IsInitialized) { onResult?.Invoke(IapResult.NotReady); return; }
            if (_pending != null) { onResult?.Invoke(IapResult.Busy); return; }

            Product product = _controller.GetProductById(storeId);
            if (product == null || !product.availableToPurchase)
            {
                Debug.LogWarning($"[UnityIapProvider] Product '{storeId}' not available to purchase.");
                onResult?.Invoke(IapResult.Failed);
                return;
            }

            _pending = onResult;
            _controller.PurchaseProduct(product);
        }

        public void RestorePurchases(Action<bool> onDone)
        {
            if (!IsInitialized) { onDone?.Invoke(false); return; }
            // Cross-platform in IAP 5; restored entitlements come back through OnPurchasePending.
            _controller.RestoreTransactions((success, error) =>
            {
                if (!success) Debug.LogWarning($"[UnityIapProvider] Restore failed: {error}");
                onDone?.Invoke(success);
            });
        }

        // The one grant path — fresh purchases, replayed unfinished orders and restores all land
        // here. Grant first, then confirm to the store; an unconfirmed order is replayed on the
        // next launch, so a crash between grant and confirm re-grants (idempotent for Remove Ads,
        // acceptable double-grant risk for gem packs vs. losing a paid purchase).
        private void HandlePurchasePending(PendingOrder order)
        {
            if (!IsOrderValid(order))
            {
                Debug.LogWarning("[UnityIapProvider] Rejected pending order (invalid receipt).");
                Resolve(IapResult.Failed);
                return;
            }

            foreach (CartItem item in order.CartOrdered.Items())
                _onGranted?.Invoke(item.Product.definition.id);

            _controller.ConfirmPurchase(order);
            Resolve(IapResult.Success);
        }

        private void HandlePurchaseFailed(FailedOrder order)
        {
            Debug.LogWarning($"[UnityIapProvider] Purchase failed: {order.FailureReason} {order.Details}");
            Resolve(order.FailureReason == PurchaseFailureReason.UserCancelled ? IapResult.Cancelled : IapResult.Failed);
        }

        // Local receipt validation. Unity's obfuscated tangle classes are editor-generated
        // (Services > In-App Purchasing > Receipt Validation Obfuscator, with the Google Play
        // public key) — until they exist and a validator is plugged in here, every order passes.
        // Tracked in Docs/pre-launch-checklist.md.
        private static bool IsOrderValid(PendingOrder order) => order != null;

        private void Resolve(IapResult result)
        {
            Action<IapResult> callback = _pending;
            _pending = null;
            callback?.Invoke(result);
        }

        private void OnDestroy()
        {
            if (_controller == null) return;
            _controller.OnStoreConnected -= HandleStoreConnected;
            _controller.OnProductsFetched -= HandleProductsFetched;
            _controller.OnProductsFetchFailed -= HandleProductsFetchFailed;
            _controller.OnPurchasePending -= HandlePurchasePending;
            _controller.OnPurchaseFailed -= HandlePurchaseFailed;
        }
    }
}
#endif

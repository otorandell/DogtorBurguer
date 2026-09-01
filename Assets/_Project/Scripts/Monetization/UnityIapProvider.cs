// Real store body over Unity In-App Purchasing 4.x (com.unity.purchasing). Unity defines
// ENABLE_CLOUD_SERVICES_PURCHASING for game assemblies while the package is installed (NOT
// UNITY_PURCHASING — that one lives only inside the package's own asmdefs); without it this file
// compiles to nothing and IapManager falls back to MockIapProvider, so a checkout without the
// package still builds.
#if ENABLE_CLOUD_SERVICES_PURCHASING
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace DogtorBurguer
{
    /// <summary>
    /// <see cref="IIapProvider"/> over Unity IAP: registers the catalog with the store, resolves
    /// purchases through the store listener, and grants ONLY from <see cref="ProcessPurchase"/> —
    /// which the store also calls for restored / deferred transactions, so the same path covers
    /// Remove Ads coming back after a reinstall. Receipt validation hook: see
    /// <see cref="IsReceiptValid"/> (needs the editor-generated tangle classes — pre-launch checklist).
    /// </summary>
    public class UnityIapProvider : MonoBehaviour, IIapProvider, IDetailedStoreListener
    {
        private IStoreController _controller;
        private IExtensionProvider _extensions;
        private Action<string> _onGranted;
        private Action<IapResult> _pending;

        public bool IsInitialized => _controller != null;

        public void Initialize(IReadOnlyList<(string StoreId, bool Consumable)> products, Action<string> onGranted)
        {
            _onGranted = onGranted;

            ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach ((string storeId, bool consumable) in products)
                builder.AddProduct(storeId, consumable ? ProductType.Consumable : ProductType.NonConsumable);

            UnityPurchasing.Initialize(this, builder);
        }

        public string LocalizedPrice(string storeId)
        {
            Product product = _controller?.products.WithID(storeId);
            return product != null && product.availableToPurchase ? product.metadata.localizedPriceString : null;
        }

        public void Purchase(string storeId, Action<IapResult> onResult)
        {
            if (_controller == null) { onResult?.Invoke(IapResult.NotReady); return; }
            if (_pending != null) { onResult?.Invoke(IapResult.Busy); return; }

            Product product = _controller.products.WithID(storeId);
            if (product == null || !product.availableToPurchase)
            {
                Debug.LogWarning($"[UnityIapProvider] Product '{storeId}' not available to purchase.");
                onResult?.Invoke(IapResult.Failed);
                return;
            }

            _pending = onResult;
            _controller.InitiatePurchase(product);
        }

        public void RestorePurchases(Action<bool> onDone)
        {
            if (_extensions == null) { onDone?.Invoke(false); return; }
#if UNITY_IOS
            _extensions.GetExtension<IAppleExtensions>().RestoreTransactions((success, message) =>
            {
                if (!success) Debug.LogWarning($"[UnityIapProvider] Restore failed: {message}");
                onDone?.Invoke(success);
            });
#else
            // Google Play replays owned non-consumables through ProcessPurchase at init.
            onDone?.Invoke(true);
#endif
        }

        // --- IDetailedStoreListener ---

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            Debug.Log("[UnityIapProvider] Store initialized.");
        }

        public void OnInitializeFailed(InitializationFailureReason error) => OnInitializeFailed(error, null);

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogWarning($"[UnityIapProvider] Store init failed: {error} {message}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            Product product = args.purchasedProduct;
            if (!IsReceiptValid(product))
            {
                Debug.LogWarning($"[UnityIapProvider] Rejected receipt for '{product.definition.id}'.");
                Resolve(IapResult.Failed);
                return PurchaseProcessingResult.Complete;
            }

            _onGranted?.Invoke(product.definition.id);
            Resolve(IapResult.Success);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.LogWarning($"[UnityIapProvider] Purchase of '{product?.definition.id}' failed: {reason}");
            Resolve(reason == PurchaseFailureReason.UserCancelled ? IapResult.Cancelled : IapResult.Failed);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription description) =>
            OnPurchaseFailed(product, description.reason);

        // Local receipt validation. Unity's CrossPlatformValidator needs the obfuscated tangle
        // classes generated in the editor (Window > Unity IAP > Receipt Validation Obfuscator,
        // with the Google Play public key) — until they exist every receipt passes. Tracked in
        // Docs/pre-launch-checklist.md.
        private static bool IsReceiptValid(Product product) => product != null && product.hasReceipt;

        private void Resolve(IapResult result)
        {
            Action<IapResult> callback = _pending;
            _pending = null;
            callback?.Invoke(result);
        }
    }
}
#endif

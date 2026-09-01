using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Stand-in store for the editor and for builds without the purchasing package: every
    /// purchase "succeeds" one frame later (async like a real store, so callers can't rely on a
    /// synchronous grant), no localized prices (callers show the config label), restore is a no-op.
    /// Logs every transaction loudly — it grants real in-game value for free.
    /// </summary>
    public class MockIapProvider : MonoBehaviour, IIapProvider
    {
        private Action<string> _onGranted;
        private bool _busy;

        public bool IsInitialized { get; private set; }

        public void Initialize(IReadOnlyList<(string StoreId, bool Consumable)> products, Action<string> onGranted)
        {
            _onGranted = onGranted;
            IsInitialized = true;
            Debug.Log($"[MockIapProvider] Initialized with {products.Count} products (no real store — purchases are free).");
        }

        public string LocalizedPrice(string storeId) => null;

        public void Purchase(string storeId, Action<IapResult> onResult)
        {
            if (!IsInitialized) { onResult?.Invoke(IapResult.NotReady); return; }
            if (_busy) { onResult?.Invoke(IapResult.Busy); return; }

            _busy = true;
            StartCoroutine(CompleteNextFrame(storeId, onResult));
        }

        private IEnumerator CompleteNextFrame(string storeId, Action<IapResult> onResult)
        {
            yield return null;
            _busy = false;
            Debug.Log($"[MockIapProvider] 'Bought' {storeId} (mock — nothing charged)");
            _onGranted?.Invoke(storeId);
            onResult?.Invoke(IapResult.Success);
        }

        public void RestorePurchases(Action<bool> onDone)
        {
            Debug.Log("[MockIapProvider] Restore requested (mock — nothing to restore)");
            onDone?.Invoke(true);
        }
    }
}

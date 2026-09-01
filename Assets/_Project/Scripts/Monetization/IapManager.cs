using System;
using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Game-facing in-app-purchase facade, the twin of <see cref="AdManager"/>: owns one
    /// <see cref="IIapProvider"/> (Unity IAP when the purchasing package is installed, the mock
    /// otherwise) and the catalog → grant mapping. The shop asks it to <see cref="Purchase"/>
    /// and shows <see cref="PriceLabel"/>; every grant — bought, restored or replayed — lands in
    /// <see cref="Grant"/>, which forwards to the store-agnostic rules in ShopService.
    /// </summary>
    public class IapManager : Singleton<IapManager>
    {
        private IIapProvider _provider;

        /// <summary>Raised after any grant (a purchase, or a restore replaying one) so open UI can re-render.</summary>
        public event Action<string> OnGranted;

        public bool IsStoreReady => _provider != null && _provider.IsInitialized;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;
            DontDestroyOnLoad(gameObject);

#if ENABLE_CLOUD_SERVICES_PURCHASING
            _provider = gameObject.AddComponent<UnityIapProvider>();
#else
            Debug.LogWarning("[IapManager] Purchasing package not installed — using MockIapProvider (purchases are free).");
            _provider = gameObject.AddComponent<MockIapProvider>();
#endif
            _provider.Initialize(Catalog(), Grant);
        }

        // The catalog is MonetizationConfig's: every gem pack (consumable) plus Remove Ads.
        private static List<(string StoreId, bool Consumable)> Catalog()
        {
            List<(string, bool)> products = new();
            foreach (GemProduct product in MonetizationConfig.GEM_PRODUCTS)
                products.Add((product.StoreId, true));
            products.Add((MonetizationConfig.REMOVE_ADS_STORE_ID, false));
            return products;
        }

        /// <summary>Starts a store purchase. The grant arrives via <see cref="Grant"/> (and
        /// <see cref="OnGranted"/>); onResult reports the store outcome for UI feedback.</summary>
        public void Purchase(string storeId, Action<IapResult> onResult)
        {
            if (_provider == null) { onResult?.Invoke(IapResult.NotReady); return; }
            _provider.Purchase(storeId, onResult);
        }

        /// <summary>The store's localized price when known, else the config's placeholder label.</summary>
        public string PriceLabel(string storeId, string fallback)
        {
            string localized = _provider?.LocalizedPrice(storeId);
            return string.IsNullOrEmpty(localized) ? fallback : localized;
        }

        /// <summary>App Store's required "Restore Purchases" action; harmless on Google Play.</summary>
        public void RestorePurchases(Action<bool> onDone)
        {
            if (_provider == null) { onDone?.Invoke(false); return; }
            _provider.RestorePurchases(onDone);
        }

        // The one grant path. Gem packs add gems (a replayed consumable is a purchase that never
        // got granted, so adding again is right); Remove Ads is idempotent in ShopService.
        private void Grant(string storeId)
        {
            if (storeId == MonetizationConfig.REMOVE_ADS_STORE_ID)
            {
                ShopService.GrantRemoveAds();
            }
            else
            {
                GemProduct? pack = MonetizationConfig.FindGemProduct(storeId);
                if (pack.HasValue) ShopService.GrantGemPack(pack.Value);
                else Debug.LogWarning($"[IapManager] Grant for unknown product '{storeId}' ignored.");
            }

            OnGranted?.Invoke(storeId);
        }
    }
}

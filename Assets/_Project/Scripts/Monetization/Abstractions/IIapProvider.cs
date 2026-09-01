using System;
using System.Collections.Generic;

namespace DogtorBurguer
{
    /// <summary>
    /// The in-app-purchase store contract, shaped like real billing SDKs behave (Unity IAP over
    /// Google Play / App Store): an async init that registers the product catalog, purchases
    /// that resolve through callbacks (success, user-cancel, failure), a store-localized price
    /// per product, and a restore path for non-consumables. Game code talks to
    /// <see cref="IapManager"/>, which owns one provider — the mock in the editor and in builds
    /// without the purchasing package, Unity IAP otherwise.
    ///
    /// Contract rules implementations must honor:
    /// - <c>onGranted</c> is the ONLY grant path: it fires for a completed purchase, and again for
    ///   restored / deferred purchases the store replays (non-consumables at init, a consumable
    ///   whose grant was interrupted). Grants must therefore be idempotent where it matters.
    /// - The purchase callback fires exactly once per Purchase call; NotReady / Busy fire
    ///   synchronously, everything else after the store answers.
    /// - <see cref="LocalizedPrice"/> returns null until the store has priced the product —
    ///   callers fall back to the config label.
    /// </summary>
    public interface IIapProvider
    {
        bool IsInitialized { get; }

        /// <summary>Begins store init with the catalog (store id, consumable?). Safe to call once at boot.</summary>
        void Initialize(IReadOnlyList<(string StoreId, bool Consumable)> products, Action<string> onGranted);

        /// <summary>The store's localized price string for a product, or null while unknown.</summary>
        string LocalizedPrice(string storeId);

        /// <summary>Starts a purchase; the grant arrives through onGranted, the outcome through onResult.</summary>
        void Purchase(string storeId, Action<IapResult> onResult);

        /// <summary>Replays owned non-consumables through onGranted (App Store requires a user-facing
        /// button for this; Google Play restores on init). onDone(true) when the store accepted the request.</summary>
        void RestorePurchases(Action<bool> onDone);
    }
}

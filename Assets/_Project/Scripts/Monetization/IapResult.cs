namespace DogtorBurguer
{
    /// <summary>Outcome of one <see cref="IIapProvider.Purchase"/> call.</summary>
    public enum IapResult
    {
        /// <summary>The store completed the purchase; the grant was delivered through onGranted.</summary>
        Success,
        /// <summary>The player backed out of the store dialog — not an error, no feedback needed.</summary>
        Cancelled,
        /// <summary>The store refused or the transaction errored (network, payment declined, unknown product).</summary>
        Failed,
        /// <summary>The store isn't initialized (no connection yet, or the package is missing).</summary>
        NotReady,
        /// <summary>Another purchase is still in flight.</summary>
        Busy,
    }
}

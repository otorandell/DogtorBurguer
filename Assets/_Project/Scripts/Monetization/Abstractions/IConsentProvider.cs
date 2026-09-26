using System;

namespace DogtorBurguer
{
    /// <summary>
    /// The privacy-consent contract (GDPR/UK today; a Google-certified CMP behind it). The CMP
    /// shows its OWN forms — the game never draws consent UI — and records the answer in the
    /// standard IAB TCF storage, which LevelPlay (7.7+) reads by itself, so no consent value is
    /// passed around in code. Owned by <see cref="AdManager"/> (the only consumer of consent).
    ///
    /// Contract rules implementations must honor:
    /// - <see cref="Gather"/>'s onDone fires exactly once, on the main thread, whatever
    ///   happens (form shown, not required, network error) — ads must start either way.
    /// - <see cref="ShowPrivacyOptions"/>'s onClosed fires exactly once, on the main thread.
    /// </summary>
    public interface IConsentProvider
    {
        /// <summary>Refreshes the consent status and, where the law requires it and no valid
        /// answer is stored, shows the consent form. Call once at boot, BEFORE ads init.</summary>
        void Gather(Action onDone);

        /// <summary>True when this player must be offered a way to revisit their choice (the
        /// Settings "Privacy" row) — i.e. they are in a regulated region.</summary>
        bool PrivacyOptionsRequired { get; }

        /// <summary>Re-opens the CMP's privacy options form (change / withdraw consent).</summary>
        void ShowPrivacyOptions(Action onClosed);
    }
}

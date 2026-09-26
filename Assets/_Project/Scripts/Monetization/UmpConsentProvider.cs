#if UMP_CONSENT
using System;
using System.Collections.Generic;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Google's User Messaging Platform (the Google-certified CMP) — compiled only with the
    /// UMP_CONSENT define, set once the Google Mobile Ads Unity plugin is imported and the AdMob
    /// app ID is filled in (see Docs/pre-launch-checklist.md → Consent). The form itself is the
    /// "European regulations" message configured in AdMob → Privacy & messaging (published
    /// 2026-09-26 for the Doctor Burger app: EN ES PT DE FR IT, "Do not consent" on the first
    /// screen). UMP writes the IAB TCF strings; LevelPlay reads them — nothing is passed here.
    /// SDK callbacks are marshalled to the main thread before touching game code.
    /// </summary>
    public class UmpConsentProvider : MonoBehaviour, IConsentProvider
    {
        private readonly Queue<Action> _mainThread = new();

        public void Gather(Action onDone)
        {
            var request = new ConsentRequestParameters();
            if (MonetizationConfig.UMP_DEBUG_FORCE_EEA)
            {
                // Test only: pretend the device is in the EEA so the form shows from anywhere.
                request.ConsentDebugSettings = new ConsentDebugSettings
                {
                    DebugGeography = DebugGeography.EEA,
                    TestDeviceHashedIds = new List<string>(MonetizationConfig.UMP_TEST_DEVICE_IDS),
                };
            }

            ConsentInformation.Update(request, updateError =>
            {
                if (updateError != null)
                {
                    Debug.LogWarning($"[UmpConsentProvider] Consent update failed: {updateError.Message} — ads start anyway (with whatever consent is stored).");
                    Post(onDone);
                    return;
                }

                // Shows the form only when required (regulated region, no valid stored answer).
                ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
                {
                    if (formError != null)
                        Debug.LogWarning($"[UmpConsentProvider] Consent form error: {formError.Message}");
                    Post(onDone);
                });
            });
        }

        public bool PrivacyOptionsRequired =>
            ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;

        public void ShowPrivacyOptions(Action onClosed)
        {
            ConsentForm.ShowPrivacyOptionsForm(formError =>
            {
                if (formError != null)
                    Debug.LogWarning($"[UmpConsentProvider] Privacy options form error: {formError.Message}");
                Post(onClosed);
            });
        }

        private void Post(Action action)
        {
            if (action == null) return;
            lock (_mainThread) _mainThread.Enqueue(action);
        }

        private void Update()
        {
            while (true)
            {
                Action next;
                lock (_mainThread)
                {
                    if (_mainThread.Count == 0) return;
                    next = _mainThread.Dequeue();
                }
                next();
            }
        }
    }
}
#endif

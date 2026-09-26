using System;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Consent stand-in for the editor and for builds without the Google UMP SDK (no
    /// UMP_CONSENT define): nothing to ask, ads start immediately. In the editor it reports the
    /// privacy row as REQUIRED so the Settings layout can be seen and tapped (the tap only logs).
    /// </summary>
    public class MockConsentProvider : MonoBehaviour, IConsentProvider
    {
        public void Gather(Action onDone) => onDone?.Invoke();

        public bool PrivacyOptionsRequired =>
#if UNITY_EDITOR
            true;
#else
            false;
#endif

        public void ShowPrivacyOptions(Action onClosed)
        {
            Debug.Log("[MockConsentProvider] Privacy options requested — the real form is Google's (UMP), on device.");
            onClosed?.Invoke();
        }
    }
}

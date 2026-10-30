#if UNITY_ANDROID || UNITY_EDITOR

using System;
using System.Threading;
using UnityEngine;

internal partial class AirbridgeAndroidPlugin
{
    #region Permission

    private static SynchronizationContext _mainContext;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CaptureMainContext()
    {
        _mainContext = SynchronizationContext.Current;
    }

    // Android has no App Tracking Transparency, so the request completes as authorized without
    // asking. The callback is posted to the main thread, which is where the iOS SDK reports too.
    public bool RequestTrackingAuthorization(Action<AirbridgeTrackingAuthorization> onComplete)
    {
        if (_mainContext == null)
        {
            Debug.Log("[Airbridge] The main thread is unknown, so the callback is called in place.");
            onComplete(AirbridgeTrackingAuthorization.Authorized);
        }
        else
        {
            _mainContext.Post(_ => onComplete(AirbridgeTrackingAuthorization.Authorized), null);
        }

        return true;
    }

    public AirbridgeTrackingAuthorization TrackingAuthorizationStatus()
    {
        return AirbridgeTrackingAuthorization.Authorized;
    }

    #endregion
}

#endif

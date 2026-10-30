#if UNITY_IOS || UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

internal partial class AirbridgeIOSPlugin
{
    #region Permission

    [DllImport("__Internal")] private static extern bool native_requestTrackingAuthorization(
        int requestId,
        Action<int, int> onComplete
    );

    // Requests are not merged, so each one keeps its own callback rather than sharing a field.
    // Requests can come from any thread while the SDK reports on the main one, so the registry
    // is guarded.
    private static readonly object TrackingAuthorizationLock = new object();
    private static readonly Dictionary<int, Action<AirbridgeTrackingAuthorization>> TrackingAuthorizationCallbacks =
        new Dictionary<int, Action<AirbridgeTrackingAuthorization>>();

    private static int _lastTrackingAuthorizationRequestId;

    [MonoPInvokeCallback(typeof(Action<int, int>))]
    private static void SetOnTrackingAuthorizationCompletedCallback(int requestId, int status)
    {
        Action<AirbridgeTrackingAuthorization> onComplete;
        lock (TrackingAuthorizationLock)
        {
            if (!TrackingAuthorizationCallbacks.TryGetValue(requestId, out onComplete)) { return; }

            TrackingAuthorizationCallbacks.Remove(requestId);
        }

        onComplete((AirbridgeTrackingAuthorization)status);
    }

    // Kept in a field so the delegate is not collected while the request is in flight.
    private static readonly Action<int, int> OnTrackingAuthorizationCompleted =
        SetOnTrackingAuthorizationCompletedCallback;

    public bool RequestTrackingAuthorization(Action<AirbridgeTrackingAuthorization> onComplete)
    {
        int requestId;
        lock (TrackingAuthorizationLock)
        {
            requestId = ++_lastTrackingAuthorizationRequestId;
            TrackingAuthorizationCallbacks[requestId] = onComplete;
        }

        if (native_requestTrackingAuthorization(requestId, OnTrackingAuthorizationCompleted))
        {
            return true;
        }

        lock (TrackingAuthorizationLock)
        {
            TrackingAuthorizationCallbacks.Remove(requestId);
        }

        return false;
    }

    [DllImport("__Internal")] private static extern int native_trackingAuthorizationStatus();
    public AirbridgeTrackingAuthorization TrackingAuthorizationStatus()
    {
        return (AirbridgeTrackingAuthorization)native_trackingAuthorizationStatus();
    }

    #endregion
}

#endif

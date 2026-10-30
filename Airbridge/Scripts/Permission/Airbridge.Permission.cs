using System;

public partial class Airbridge
{
    #region Permission

    /// <summary>
    /// Requests App Tracking Transparency authorization.
    /// The request is passed to Apple as it is, so call it while the app is active.
    /// Android has no matching feature and always reports `Authorized`.
    /// </summary>
    /// <param name="onComplete"> The callback that receives the resulting status on the main thread.</param>
    /// <returns> `true` if the request was accepted, `false` if the SDK is not initialized or is disabled.</returns>
    public static bool RequestTrackingAuthorization(Action<AirbridgeTrackingAuthorization> onComplete)
    {
        return AirbridgeNullCheck.CallMethodWithNullCheck(
            () => AirbridgePlugin.RequestTrackingAuthorization(AirbridgeNullCheck.RequireNonNull(onComplete)),
            false
        );
    }

    /// <summary>
    /// Returns the current App Tracking Transparency authorization status without showing the prompt.
    /// Android has no matching feature and always returns `Authorized`.
    /// </summary>
    /// <returns> The current status, or `NotDetermined` if the SDK is not initialized or is disabled.</returns>
    public static AirbridgeTrackingAuthorization TrackingAuthorizationStatus()
    {
        return AirbridgePlugin.TrackingAuthorizationStatus();
    }

    #endregion
}

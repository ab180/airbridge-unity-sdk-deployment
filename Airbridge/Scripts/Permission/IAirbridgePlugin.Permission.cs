using System;

internal partial interface IAirbridgePlugin
{
    #region Permission

    bool RequestTrackingAuthorization(Action<AirbridgeTrackingAuthorization> onComplete);
    AirbridgeTrackingAuthorization TrackingAuthorizationStatus();

    #endregion
}

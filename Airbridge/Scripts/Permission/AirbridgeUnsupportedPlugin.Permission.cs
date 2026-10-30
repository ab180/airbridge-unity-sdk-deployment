using System;

internal partial class AirbridgeUnsupportedPlugin
{
    public bool RequestTrackingAuthorization(Action<AirbridgeTrackingAuthorization> onComplete) { return UnsupportedPlatform(false); }

    public AirbridgeTrackingAuthorization TrackingAuthorizationStatus() { return UnsupportedPlatform(AirbridgeTrackingAuthorization.NotDetermined); }
}

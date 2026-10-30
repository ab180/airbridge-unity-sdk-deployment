/// <summary>
/// The types of the events that are tracked by the SDK automatically.
///
/// Each type covers the events that are reported under the same name in Airbridge.
/// </summary>
public enum AirbridgeEventType
{
    /// <summary>
    /// Indicates the event that is tracked when the app is launched for the first time
    /// after the installation, whether the app is launched by a deeplink or not.
    /// </summary>
    INSTALL,

    /// <summary>
    /// Indicates the event that is tracked when the app is launched or foregrounded
    /// with a new session, not by a deeplink.
    /// </summary>
    OPEN,

    /// <summary>
    /// Indicates the event that is tracked when the app is launched or foregrounded
    /// by a deeplink, except for the first time after the installation.
    /// </summary>
    DEEPLINK_OPEN
}

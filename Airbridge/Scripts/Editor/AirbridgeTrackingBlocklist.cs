/// <summary>
/// Tracking data types to block from collection
/// </summary>
public enum AirbridgeTrackingBlocklist
{
    /// <summary>
    /// IDFA (Identifier for Advertisers). iOS only.
    /// </summary>
    IDFA,

    /// <summary>
    /// IDFV (Identifier for Vendors). iOS only.
    /// </summary>
    IDFV,

    /// <summary>
    /// SKAdNetwork (Apple's ad attribution framework). iOS only.
    /// When blocked, SKAdNetwork conversion values will not be updated.
    /// </summary>
    SKAdNetwork,

    /// <summary>
    /// Google Advertising ID. Android only.
    /// </summary>
    GAID,

    /// <summary>
    /// Huawei Advertising ID. Android only.
    /// </summary>
    OAID,

    /// <summary>
    /// FireOS Advertising ID. Android only.
    /// </summary>
    FAID,

    /// <summary>
    /// Google Play App Set ID (developer-scoped identifier for analytics). Android only.
    /// </summary>
    AppSetID
}

internal static class AirbridgeTrackingBlocklistExtension
{
    public static readonly string[] Values =
    {
        nameof(AirbridgeTrackingBlocklist.IDFA),
        nameof(AirbridgeTrackingBlocklist.IDFV),
        nameof(AirbridgeTrackingBlocklist.SKAdNetwork),
        nameof(AirbridgeTrackingBlocklist.GAID),
        nameof(AirbridgeTrackingBlocklist.OAID),
        nameof(AirbridgeTrackingBlocklist.FAID),
        nameof(AirbridgeTrackingBlocklist.AppSetID)
    };
}
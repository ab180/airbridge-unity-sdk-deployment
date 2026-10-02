#if UNITY_EDITOR

/// <summary>
/// Enumeration of in-app purchase environment used in [AirbridgeData](@ref AirbridgeData#inAppPurchaseEnvironment).
/// </summary>
internal enum AirbridgeInAppPurchaseEnvironment
{
    Production,
    Sandbox
}

internal static class AirbridgeInAppPurchaseEnvironmentExtension
{
    public static readonly string[] Environments =
    {
        AirbridgeInAppPurchaseEnvironment.Production.ToLowerString(),
        AirbridgeInAppPurchaseEnvironment.Sandbox.ToLowerString()
    };

    public static string ToLowerString(this AirbridgeInAppPurchaseEnvironment environment)
    {
        return environment.ToString().ToLower();
    }
}

#endif
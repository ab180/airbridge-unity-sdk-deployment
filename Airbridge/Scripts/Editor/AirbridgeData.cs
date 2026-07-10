#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data used in %Airbridge Settings
/// </summary>
internal class AirbridgeData : AirbridgeScriptableObject
{
    private const string AssetNamePrefix = "AirbridgeData";

    public enum Variant
    {
        Default,
        Dev,
        Prod
    }

    public static string GetAssetName(Variant variant)
    {
        if (variant == Variant.Default)
        {
            return AssetNamePrefix;
        }

        return AssetNamePrefix + variant;
    }

    private AirbridgeData()
    {
    }

    internal static AirbridgeData Resolve(bool isDevelopment)
    {
        if (isDevelopment)
        {
            var dev = GetInstance<AirbridgeData>(GetAssetName(Variant.Dev));
            if (dev != null && dev.isActive) return dev;
        }
        else
        {
            var prod = GetInstance<AirbridgeData>(GetAssetName(Variant.Prod));
            if (prod != null && prod.isActive) return prod;
        }

        return GetInstance<AirbridgeData>(GetAssetName(Variant.Default));
    }

    public bool isActive;

    /// <summary> App Name.</summary>
    public string appName;

    /// <summary> App SDK Token.</summary>
    public string appToken;

    /// <summary> Adjusts the log record level for %Airbridge.</summary>
    public int logLevel = AirbridgeLogLevel.Warning.ToInt();

    /// <summary> Protects against SDK spoofing. Both sdkSignatureSecretID and sdkSignatureSecret values must be applied.</summary>
    public string sdkSignatureSecretID;

    /// <summary> Protects against SDK spoofing. Both sdkSignatureSecretID and sdkSignatureSecret values must be applied.</summary>
    public string sdkSignatureSecret;

    /// <summary> URI Scheme of the deep link. (iOS Only)</summary>
    public string iOSURIScheme;

    /// <summary> URI Scheme of the deep link. (Android Only)</summary>
    public string androidURIScheme;

    /// <summary> (Deprecated) Previously used custom domains.</summary>
    [HideInInspector] public string customDomain;

    /// <summary>
    /// Customized URLs. Such as `go.my_company.com/abcd` can also be used as tracking links to improve the branding and CTR (Click Through Rate).
    /// @attention
    /// Custom Domain should match the information in the %Airbridge dashboard.
    /// </summary>
    public List<string> customDomainList = new List<string>();

    /// <summary>
    /// An app open event will not be sent when the app is reopened within the designated period.
    /// Session timeout seconds must be between 0 second and 7 days (604800 seconds).
    /// </summary>
    public long sessionTimeoutSeconds = 300;

    /// <summary> When set to `false`, user email and user phone information are sent without being hashed.</summary>
    public bool userInfoHashEnabled = true;

    /// <summary>
    /// When set to `true`, location information is collected. (Android Only)
    /// @note
    /// Two permissions must be allowed in AndroidManifest.xml
    ///  * android.permission.ACCESS_FINE_LOCATION
    ///  * android.permission.ACCESS_COARSE_LOCATION
    /// </summary>
    public bool locationCollectionEnabled = false;

    /// <summary> When set to `true`, deep link events are sent only when app is opened with an %Airbridge deep link.</summary>
    public bool trackAirbridgeLinkOnly = false;

    /// <summary> When set to `false`, no events will be sent until Airbridge#StartTracking is called.</summary>
    public bool autoStartTrackingEnabled = true;

    /// <summary> When set to `true` and the Facebook SDK is installed, Facebook Deferred App Link data is collected.</summary>
    public bool facebookDeferredAppLinkEnabled = false;

    /// <summary>
    /// When timeout is set, Install event is delayed until Request tracking authorization alert is clicked (iOS only).
    /// iOS tracking authorize timeout seconds must be between 0 second and 1 hour (3600 seconds).
    /// </summary>
    public int iOSTrackingAuthorizeTimeoutSeconds = 30;

    /// <summary> When set to `true`, Open and Foreground events during the ongoing session is collected.</summary>
    public bool trackInSessionLifeCycleEventEnabled = false;

    /// <summary> When set to `true`, event transmission will be paused when the app goes to the background.</summary>
    public bool pauseEventTransmitOnBackgroundEnabled = false;

    /// <summary> When set to `true`, each time the app is opened, events stored in the device's internal database are cleared.</summary>
    public bool resetEventBufferEnabled = false;

    /// <summary> When set to `false`, %Airbridge SDK will be deactivated until Airbridge#EnableSDK is called.</summary>
    public bool sdkEnabled = true;

    /// <summary> App market identifier.</summary>
    /// @deprecated Deprecated and will be automatically collected.
    public string appMarketIdentifier;

    /// <summary>
    /// Adjusts the maximum event count.
    /// Event maximum buffer count must be between 0 and 2147483647.
    /// @note
    /// The %Airbridge SDK stores events as long as they do not exceed the maximum event count and maximum event size limitations.
    /// Any excess events are discarded.
    /// </summary>
    public int eventMaximumBufferCount = int.MaxValue;

    /// <summary>
    /// Adjusts the maximum event size in GiB (gibibytes).
    /// Event maximum buffer size must be between 0 byte and 1 tebibyte (1024 gibibytes).
    /// @note
    /// The %Airbridge SDK stores events as long as they do not exceed the maximum event count and maximum event size limitations.
    /// Any excess events are discarded.
    /// 
    /// </summary>
    public double eventMaximumBufferSize = 1024;

    /// <summary>
    /// Adjusts event transmission interval in seconds.
    /// Event transmit interval seconds must be between 0 second and 1 day (86400 seconds).
    /// </summary>
    public long eventTransmitIntervalSeconds = 0;

    /// <summary> Facebook App ID for meta install referrer collection setup.</summary>
    public string facebookAppId;

    /// <summary> When set to `true`, provide only %Airbridge deep links in the Airbridge#SetOnDeeplinkReceived callback</summary>
    public bool isHandleAirbridgeDeeplinkOnly = false;

    /// <summary> Sets an in-app purchase environment.</summary>
    public AirbridgeInAppPurchaseEnvironment inAppPurchaseEnvironment = AirbridgeInAppPurchaseEnvironment.Production;

    /// <summary> When set to `true`, TCF(Transparency & Consent Framework) data will be collected automatically.</summary>
    public bool collectTCFDataEnabled = false;

    /// <summary>
    /// Sets the blocklist for tracking data collection.
    /// 
    /// The specified identifiers will not be automatically collected by the SDK,
    /// which may impact advertising tracking and analytics data.
    /// </summary>
    public List<string> trackingBlocklist = new List<string>();

    /// <summary> Sets whether SKAdNetwork CV calculation should be performed on the server.</summary>
    public bool calculateSKAdNetworkByServer = false;

    private void OnEnable()
    {
        MigrateCustomDomain();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void OnValidate()
    {
        sessionTimeoutSeconds = Math.Max(0, Math.Min(sessionTimeoutSeconds, (long)TimeSpan.FromDays(7).TotalSeconds));
        eventMaximumBufferCount = Math.Max(0, Math.Min(eventMaximumBufferCount, int.MaxValue));
        eventMaximumBufferSize = Math.Max(0.0, Math.Min(eventMaximumBufferSize, 1024.0));
        eventTransmitIntervalSeconds = Math.Max(0, Math.Min(eventTransmitIntervalSeconds, (long)TimeSpan.FromDays(1).TotalSeconds));
        iOSTrackingAuthorizeTimeoutSeconds = Math.Max(0, Math.Min(iOSTrackingAuthorizeTimeoutSeconds, (int)TimeSpan.FromHours(1).TotalSeconds));
    }

    private void MigrateCustomDomain()
    {
        if (string.IsNullOrWhiteSpace(customDomain)) return;

        customDomainList = new List<string>()
            .MergeTrimmed(customDomain, AirbridgeEditorConstant.CustomDomain.CustomDomainSeparator)
            .MergeTrimmed(customDomainList)
            .RemoveDuplicates();
        customDomain = null;
    }
}

#endif
#if UNITY_EDITOR

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

internal abstract class AirbridgeSettingsPlatformProcessor
{
    protected AirbridgeData Data { get; private set; }

    protected abstract void Process();

    public static void Process(BuildTarget platform)
    {
        AirbridgeSettingsPlatformProcessor processor = null;

        if (platform == BuildTarget.Android)
            processor = new AirbridgeSettingsAndroidProcessor();

        if (platform == BuildTarget.iOS)
            processor = new AirbridgeSettingsIosProcessor();

        if (processor == null) return;

        processor.Data = AirbridgeData.Resolve(AirbridgeBuildContext.IsDevelopment);
        processor.Process();
    }
}

#region Android

internal class AirbridgeSettingsAndroidProcessor : AirbridgeSettingsPlatformProcessor
{
    protected override void Process()
    {
        UpdateNativeCode();
        UpdateAirbridgeSettings();
    }

    private void UpdateNativeCode()
    {
        var entry = AirbridgeAndroidApplicationEntry.None;
#if UNITY_2023_1_OR_NEWER
        entry = PlayerSettings.Android.applicationEntry.ConvertToAirbridgeType();
#else
        entry = AirbridgeAndroidApplicationEntry.Activity;
#endif

        UpdateAndroidActivity(entry);
        UpdateAndroidManifest(entry);
    }

    private void UpdateAirbridgeSettings()
    {
        var pluginPath = AirbridgeFileUtils.GetProjectAirbridgePluginPath(AirbridgeFileUtils.Platform.Android);
        if (pluginPath == null) return;
        var path = Path.Combine(pluginPath, "AirbridgeSettings.java");

        try
        {
            var content =
                "package co.ab180.airbridge.unity;\n"
                + "\n"
                + "public class AirbridgeSettings {\n"
                + "\n"
                + $"public static String appName = \"{Data.appName}\";\n"
                + $"public static String appToken = \"{Data.appToken}\";\n"
                + $"public static String sdkSignatureSecretID = \"{Data.sdkSignatureSecretID}\";\n"
                + $"public static String sdkSignatureSecret = \"{Data.sdkSignatureSecret}\";\n"
                + $"public static int logLevel = {Data.logLevel};\n"
                + "public static String customDomain = \"" + string.Join(
                    separator: AirbridgeEditorConstant.CustomDomain.CustomDomainSeparator.ToString(),
                    values: Data.customDomainList
                ) + "\";\n"
                + $"public static int sessionTimeoutSeconds = {Data.sessionTimeoutSeconds};\n"
                + $"public static boolean userInfoHashEnabled = {Data.userInfoHashEnabled.ToString().ToLower()};\n"
                + $"public static boolean locationCollectionEnabled = {Data.locationCollectionEnabled.ToString().ToLower()};\n"
                + $"public static boolean trackAirbridgeLinkOnly = {Data.trackAirbridgeLinkOnly.ToString().ToLower()};\n"
                + $"public static boolean autoStartTrackingEnabled = {Data.autoStartTrackingEnabled.ToString().ToLower()};\n"
                + $"public static boolean facebookDeferredAppLinkEnabled = {Data.facebookDeferredAppLinkEnabled.ToString().ToLower()};\n"
                + $"public static boolean trackInSessionLifeCycleEventEnabled = {Data.trackInSessionLifeCycleEventEnabled.ToString().ToLower()};\n"
                + $"public static boolean pauseEventTransmitOnBackgroundEnabled = {Data.pauseEventTransmitOnBackgroundEnabled.ToString().ToLower()};\n"
                + $"public static boolean clearEventBufferOnInitializeEnabled = {Data.resetEventBufferEnabled.ToString().ToLower()};\n"
                + $"public static boolean sdkEnabled = {Data.sdkEnabled.ToString().ToLower()};\n"
                + $"public static String appMarketIdentifier = \"{Data.appMarketIdentifier}\";\n"
                + $"public static int eventBufferCountLimitInGibibyte = {Data.eventMaximumBufferCount};\n"
                + $"public static double eventBufferSizeLimitInGibibyte = {Data.eventMaximumBufferSize};\n"
                + $"public static long eventTransmitIntervalSeconds = {Data.eventTransmitIntervalSeconds};\n"
                + $"public static String facebookAppId = \"{Data.facebookAppId}\";\n"
                + $"public static boolean isHandleAirbridgeDeeplinkOnly = {Data.isHandleAirbridgeDeeplinkOnly.ToString().ToLower()};\n"
                + $"public static String inAppPurchaseEnvironment = \"{Data.inAppPurchaseEnvironment.ToLowerString()}\";\n"
                + $"public static boolean collectTCFDataEnabled = {Data.collectTCFDataEnabled.ToString().ToLower()};\n"
                + "public static String trackingBlocklist = \"" + string.Join(
                    separator: AirbridgeEditorConstant.BlockList.TrackingBlocklistSeparator,
                    values: Data.trackingBlocklist
                ) + "\";\n"
                + "\n"
                + "}\n";

            AirbridgeFileUtils.PrepareFile(path);
            File.WriteAllText(path, content);
            Debug.Log("[Airbridge] Updated Android Airbridge settings file.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Airbridge] Failed to update Android Airbridge settings file: {e.Message}");
        }
    }

    private static void UpdateAndroidActivity(AirbridgeAndroidApplicationEntry entry)
    {
        var pluginPath = AirbridgeFileUtils.GetProjectAirbridgePluginPath(AirbridgeFileUtils.Platform.Android);
        if (pluginPath == null) return;

        try
        {
            // 기존에 생성된 Airbridge Activity 파일들을 모두 삭제한다.
            var airbridgeActivityPath =
                Path.Combine(pluginPath, AirbridgeAndroidApplicationEntry.Activity.GetActivityFileName());
            var airbridgeGameActivityPath =
                Path.Combine(pluginPath, AirbridgeAndroidApplicationEntry.GameActivity.GetActivityFileName());

            if (File.Exists(airbridgeActivityPath))
            {
                File.Delete(airbridgeActivityPath);
            }

            if (File.Exists(airbridgeGameActivityPath))
            {
                File.Delete(airbridgeGameActivityPath);
            }

            // AirbridgeAndroidApplicationEntry가 None이면 처리를 종료한다.
            if (entry.IsNone()) return;

            // AirbridgeAndroidApplicationEntry 값에 해당하는 Airbridge Activity 파일을 생성한다.
            var defaultActivityPath = Path.Combine(
                AirbridgeFileUtils.GetPackageAirbridgePluginPath(AirbridgeFileUtils.Platform.Android),
                "java/co/ab180/airbridge/unity",
                entry.GetActivityFileName() + ".template"
            );
            var activityPath = Path.Combine(pluginPath, entry.GetActivityFileName());

            File.Copy(defaultActivityPath, activityPath, true);
            Debug.Log("[Airbridge] Updated Airbridge Android Activity file.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Airbridge] Failed to update Airbridge Android Activity file: {e.Message}");
        }
    }

    private void UpdateAndroidManifest(AirbridgeAndroidApplicationEntry entry)
    {
        try
        {
            var androidManifest = UpdateProjectAndroidManifestOrThrow(entry);
            UpdateLibraryAndroidManifestOrThrow(androidManifest);

            Debug.Log("[Airbridge] Updated Airbridge Android Manifest file.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Airbridge] Failed to update Airbridge Android Manifest file: {e.Message}");
        }
    }

    private static AndroidManifest UpdateProjectAndroidManifestOrThrow(AirbridgeAndroidApplicationEntry entry)
    {
        var destAndroidManifestPath = Path.Combine(
            AirbridgeFileUtils.GetProjectPluginPath(AirbridgeFileUtils.Platform.Android),
            "AndroidManifest.xml");

        // AirbridgeAndroidApplicationEntry가 None이 아닌 경우,
        // AndroidManifest.xml이 존재하지 않으면 기본 AndroidManifest.xml 템플릿을 추가한다.
        if (!entry.IsNone() &&
            AirbridgeFileUtils.PrepareFile(destAndroidManifestPath))
        {
            var srcAndroidManifestPath = Path.Combine(
                AirbridgeFileUtils.GetPackageAirbridgePluginPath(AirbridgeFileUtils.Platform.Android),
                entry.GetManifestFileName());

            File.Copy(srcAndroidManifestPath, destAndroidManifestPath, true);
        }

        var androidManifest = new AndroidManifest(destAndroidManifestPath);
        androidManifest.SetPackageName(Application.identifier);
        androidManifest.SetPermission("android.permission.INTERNET");
        androidManifest.SetPermission("android.permission.ACCESS_NETWORK_STATE");

        androidManifest.SetContentProvider(
            "co.ab180.airbridge.unity.AirbridgeContentProvider",
            "${applicationId}.co.ab180.airbridge.unity.AirbridgeContentProvider",
            "false"
        );

        // AirbridgeAndroidApplicationEntry가 None이 아닌 경우,
        // 기존에 추가된 AndroidManifest.xml 템플릿을 현재 빌드 시점의 AirbridgeAndroidApplicationEntry 값에 맞게 수정한다.
        if (!entry.IsNone())
        {
            androidManifest.ReplaceActivityName(
                entry.GetUnityPlayerActivityName(),
                entry.GetAirbridgeActivityName()
            );

            androidManifest.ReplaceTheme(AirbridgeAndroidApplicationEntryExtension.Themes, entry.GetTheme());
            androidManifest.ReplaceActivityName(AirbridgeAndroidApplicationEntryExtension.AirbridgeActivityNames,
                entry.GetAirbridgeActivityName());
        }

        androidManifest.Save(destAndroidManifestPath);
        return androidManifest;
    }

    /// <summary>
    /// Airbridge Setting의 다음 값을 참고하여 라이브러리의 AndroidManifest.xml을 수정한다.<br/>
    /// - appName<br/>
    /// - androidURIScheme<br/>
    /// - customDomain<br/>
    /// <br/>
    /// Default, Dev, Prod 환경별로 서로 다른 값을 가지므로, 라이브러리의 AndroidManifest.xml 파일로 관리한다.
    /// </summary>
    /// <param name="projectAndroidManifest"> 프로젝트의 AndroidManifest</param>
    private void UpdateLibraryAndroidManifestOrThrow(AndroidManifest projectAndroidManifest)
    {
        var destAndroidManifestPath = Path.Combine(
            AirbridgeFileUtils.GetProjectPluginPath(AirbridgeFileUtils.Platform.Android),
            "Airbridge.androidlib", "AndroidManifest.xml");

        if (AirbridgeFileUtils.PrepareFile(destAndroidManifestPath))
        {
            var srcAndroidManifestPath = Path.Combine(
                AirbridgeFileUtils.GetPackageAirbridgePluginPath(AirbridgeFileUtils.Platform.Android),
                "Library_AndroidManifest.xml");

            File.Copy(srcAndroidManifestPath, destAndroidManifestPath, true);
        }

        var androidManifest = new AndroidManifest(destAndroidManifestPath);
        androidManifest.ClearApplicationAndAddActivities(projectAndroidManifest);

        // Airbridge App Links
        if (!string.IsNullOrEmpty(Data.appName))
        {
            androidManifest.SetUnityActivityAppLinksIntentFilter($"{Data.appName}.abr.ge");
            androidManifest.SetUnityActivityAppLinksIntentFilter($"{Data.appName}.airbridge.io");
            androidManifest.SetUnityActivityAppLinksIntentFilter($"{Data.appName}.deeplink.page");
        }

        // URI Scheme of the deep link
        if (!string.IsNullOrEmpty(Data.androidURIScheme))
        {
            androidManifest.SetUnityActivityIntentFilter(
                false,
                "android.intent.action.VIEW",
                new[] { "android.intent.category.DEFAULT", "android.intent.category.BROWSABLE" },
                Data.androidURIScheme
            );
        }

        // Custom Domain
        foreach (var customDomain in Data.customDomainList)
        {
            androidManifest.SetUnityActivityAppLinksIntentFilter(customDomain);
        }

        androidManifest.Save(destAndroidManifestPath);
    }
}

#endregion

#region iOS

internal class AirbridgeSettingsIosProcessor : AirbridgeSettingsPlatformProcessor
{
    protected override void Process()
    {
        UpdateAppSetting();
    }

    private void UpdateAppSetting()
    {
        var pluginPath = AirbridgeFileUtils.GetProjectAirbridgePluginPath(AirbridgeFileUtils.Platform.iOS);
        if (pluginPath == null) return;
        var path = Path.Combine(pluginPath, "AUAppSetting.h");

        try
        {
            var content =
                "#ifndef AUAppSetting_h\n"
                + "#define AUAppSetting_h\n"
                + "\n"
                + $"static NSString* appName = @\"{Data.appName}\";\n"
                + $"static NSString* appToken = @\"{Data.appToken}\";\n"
                + $"static NSString* sdkSignatureSecretID = @\"{Data.sdkSignatureSecretID}\";\n"
                + $"static NSString* sdkSignatureSecret = @\"{Data.sdkSignatureSecret}\";\n"
                + $"static NSUInteger logLevel = {Data.logLevel};\n"
                + $"static NSString* appScheme = @\"{Data.iOSURIScheme}\";\n"
                + "static NSString* customDomain = @\"" + string.Join(
                    separator: AirbridgeEditorConstant.CustomDomain.CustomDomainSeparator.ToString(),
                    values: Data.customDomainList
                ) + "\";\n"
                + $"static NSInteger sessionTimeoutSeconds = {Data.sessionTimeoutSeconds};\n"
                + $"static BOOL autoStartTrackingEnabled = {Data.autoStartTrackingEnabled.ToString().ToLower()};\n"
                + $"static BOOL userInfoHashEnabled = {Data.userInfoHashEnabled.ToString().ToLower()};\n"
                + $"static BOOL trackAirbridgeLinkOnly = {Data.trackAirbridgeLinkOnly.ToString().ToLower()};\n"
                + $"static BOOL facebookDeferredAppLinkEnabled = {Data.facebookDeferredAppLinkEnabled.ToString().ToLower()};\n"
                + $"static NSInteger trackingAuthorizeTimeoutSeconds = {Data.iOSTrackingAuthorizeTimeoutSeconds};\n"
                + $"static BOOL trackInSessionLifeCycleEventEnabled = {Data.trackInSessionLifeCycleEventEnabled.ToString().ToLower()};\n"
                + $"static BOOL pauseEventTransmitOnBackgroundEnabled = {Data.pauseEventTransmitOnBackgroundEnabled.ToString().ToLower()};\n"
                + $"static BOOL clearEventBufferOnInitializeEnabled = {Data.resetEventBufferEnabled.ToString().ToLower()};\n"
                + $"static BOOL sdkEnabled = {Data.sdkEnabled.ToString().ToLower()};\n"
                + $"static NSInteger eventBufferCountLimitInGibibyte = {Data.eventMaximumBufferCount};\n"
                + $"static NSInteger eventBufferSizeLimitInGibibyte = {Data.eventMaximumBufferSize};\n"
                + $"static NSInteger eventTransmitIntervalSeconds = {Data.eventTransmitIntervalSeconds};\n"
                + $"static BOOL isHandleAirbridgeDeeplinkOnly = {Data.isHandleAirbridgeDeeplinkOnly.ToString().ToLower()};\n"
                + $"static NSString* inAppPurchaseEnvironment = @\"{Data.inAppPurchaseEnvironment.ToLowerString()}\";\n"
                + $"static BOOL collectTCFDataEnabled = {Data.collectTCFDataEnabled.ToString().ToLower()};\n"
                + "static NSString* trackingBlocklist = @\"" + string.Join(
                    separator: AirbridgeEditorConstant.BlockList.TrackingBlocklistSeparator,
                    values: Data.trackingBlocklist
                ) + "\";\n"
                + $"static BOOL calculateSKAdNetworkByServer = {Data.calculateSKAdNetworkByServer.ToString().ToLower()};\n"
                + "\n"
                + "#endif\n";

            AirbridgeFileUtils.PrepareFile(path);
            File.WriteAllText(path, content);
            Debug.Log("[Airbridge] Updated iOS Airbridge settings file.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Airbridge] Failed to update iOS Airbridge settings file: {e.Message}");
        }
    }
}

#endregion

#endif
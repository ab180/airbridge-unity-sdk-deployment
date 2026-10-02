#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

internal class AirbridgeSettingsBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        AirbridgeBuildContext.IsDevelopment = (report.summary.options & BuildOptions.Development) != 0;
        var platform = report.summary.platform;

        Debug.Log("[Airbridge] AirbridgeSettingsBuildProcessor.OnPreprocessBuild: " +
                  $"isDevelopment={{{AirbridgeBuildContext.IsDevelopment}}}, platform={{{platform}}}");

        AirbridgeSettingsPlatformProcessor.Process(platform);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

public static class AirbridgeBuildContext
{
    public static bool IsDevelopment;
}

#endif
#if UNITY_EDITOR

using System;
using System.Collections.Generic;

internal enum AirbridgeAndroidApplicationEntry
{
    None,
    Activity,
    GameActivity,
}

internal static class AirbridgeAndroidApplicationEntryExtension
{
/* ===================== AndroidApplicationEntry ===================== */

#if UNITY_EDITOR && UNITY_2023_1_OR_NEWER
    internal static AirbridgeAndroidApplicationEntry ConvertToAirbridgeType(this UnityEditor.AndroidApplicationEntry entry)
    {
        switch (entry)
        {
            case UnityEditor.AndroidApplicationEntry.Activity: return AirbridgeAndroidApplicationEntry.Activity;
            case UnityEditor.AndroidApplicationEntry.GameActivity: return AirbridgeAndroidApplicationEntry.GameActivity;
            
            // Activity와 GameActivity를 모두 선택했거나, 다른 값이 들어온 경우
            default: return AirbridgeAndroidApplicationEntry.None;
        }
    }
#endif
    
/* ===================== AirbridgeAndroidApplicationEntry ===================== */

    /// <summary>
    /// Activity와 GameActivity를 모두 선택했거나, 다른 값이 들어온 경우
    /// </summary>
    internal static bool IsNone(this AirbridgeAndroidApplicationEntry entry)
    {
        return (entry == AirbridgeAndroidApplicationEntry.None);
    }

    internal static readonly List<string> Themes = new List<string>()
    {
        AirbridgeAndroidApplicationEntry.Activity.GetTheme(),
        AirbridgeAndroidApplicationEntry.GameActivity.GetTheme(),
    };

    internal static readonly List<string> AirbridgeActivityNames = new List<string>()
    {
        AirbridgeAndroidApplicationEntry.Activity.GetAirbridgeActivityName(),
        AirbridgeAndroidApplicationEntry.GameActivity.GetAirbridgeActivityName(),
    };
    
    internal static string GetActivityFileName(this AirbridgeAndroidApplicationEntry entry)
    {
        switch (entry)
        {
            case AirbridgeAndroidApplicationEntry.Activity:         return "AirbridgeActivity.java";
            case AirbridgeAndroidApplicationEntry.GameActivity:     return "AirbridgeGameActivity.java";
            
            default: throw new InvalidOperationException();
        }
    }

    internal static string GetManifestFileName(this AirbridgeAndroidApplicationEntry entry)
    {
        switch (entry)
        {
            case AirbridgeAndroidApplicationEntry.Activity:         return "Activity_AndroidManifest.xml";
            case AirbridgeAndroidApplicationEntry.GameActivity:     return "GameActivity_AndroidManifest.xml";

            default: throw new InvalidOperationException();
        }
    }

    internal static string GetTheme(this AirbridgeAndroidApplicationEntry entry)
    {
        switch (entry)
        {
            case AirbridgeAndroidApplicationEntry.Activity:         return "@style/UnityThemeSelector";
            case AirbridgeAndroidApplicationEntry.GameActivity:     return "@style/BaseUnityGameActivityTheme";

            default: throw new InvalidOperationException();
        }
    }

    internal static string GetAirbridgeActivityName(this AirbridgeAndroidApplicationEntry entry)
    {
        switch (entry)
        {
            case AirbridgeAndroidApplicationEntry.Activity:         return "co.ab180.airbridge.unity.AirbridgeActivity";
            case AirbridgeAndroidApplicationEntry.GameActivity:     return "co.ab180.airbridge.unity.AirbridgeGameActivity";

            default: throw new InvalidOperationException();
        }
    }

    internal static string GetUnityPlayerActivityName(this AirbridgeAndroidApplicationEntry entry)
    {
        switch (entry)
        {
            case AirbridgeAndroidApplicationEntry.Activity:         return "com.unity3d.player.UnityPlayerActivity";
            case AirbridgeAndroidApplicationEntry.GameActivity:     return "com.unity3d.player.UnityPlayerGameActivity";

            default: throw new InvalidOperationException();
        }
    }
}

#endif
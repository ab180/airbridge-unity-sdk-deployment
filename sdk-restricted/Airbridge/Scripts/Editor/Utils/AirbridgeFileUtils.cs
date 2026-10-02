#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

internal static class AirbridgeFileUtils
{
    private const string MetaPrefix = ".meta";

    /// <summary>
    /// Airbridge Unity SDK 의 Assets 폴더 경로를 반환합니다.
    /// </summary>
    internal static string GetPackageDataPath()
    {
        // ====================================================
        // Airbridge/Scripts/Editor/Utils/AirbridgeLocator.cs
        // ====================================================
        var script = MonoScript.FromScriptableObject(ScriptableObject.CreateInstance<AirbridgeLocator>());

        var scriptPath = AssetDatabase.GetAssetPath(script);
        var assetPath = scriptPath.ParentDirectory(5);
        return Path.GetFullPath(assetPath);
    }

    /// <summary>
    /// Unity 프로젝트의 Platform 폴더 경로를 반환합니다.
    /// 해당 경로가 존재하지 않으면 새로 생성한 후 반환합니다.
    /// </summary>
    internal static string GetProjectAirbridgePluginPath(Platform platform)
    {
        try
        {
            var pluginDirPath = Path.Combine(Application.dataPath, "Plugins");
            var airbridgeDirPath = (platform == Platform.Android)
                ? Path.Combine(pluginDirPath, "Airbridge", platform.ToString())
                : Path.Combine(pluginDirPath, platform.ToString(), "Airbridge");

            if (!Directory.Exists(airbridgeDirPath))
            {
                Directory.CreateDirectory(airbridgeDirPath);
                Debug.Log($"[Airbridge] Created plugin directory for {platform.ToString()}: {airbridgeDirPath}");
            }

            return airbridgeDirPath;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[Airbridge] Failed to get or create plugin directory for {platform.ToString()}: {exception}");
            return null;
        }
    }

    internal static string GetProjectPluginPath(Platform platform)
    {
        return Path.Combine(Application.dataPath, "Plugins", platform.ToString());
    }

    internal static string GetPackageAirbridgePluginPath(Platform platform)
    {
        return Path.Combine(GetPackageDataPath(), "Plugins", "Airbridge", platform.ToString());
    }

    // ReSharper disable once InconsistentNaming
    internal enum Platform
    {
        Android,
        iOS
    }

    internal static bool PrepareFile(string path, bool forceUpdate = false)
    {
        try
        {
            if (string.IsNullOrEmpty(path) ||
                File.Exists(path) ||
                Directory.Exists(path)) return false;

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Create(path).Dispose();
            if (forceUpdate)
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            Debug.Log($"[Airbridge] File created: '{path}'");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Airbridge] Failed to prepare file '{path}': {e.Message}");
            throw;
        }
    }

    internal static void SafeRemoveFiles(List<string> paths)
    {
        paths.SelectMany(path => new string[] { path + MetaPrefix, path })
            .ToList()
            .ForEach(SafeRemoveFile);
    }

    internal static void SafeRemoveDirectories(List<string> paths)
    {
        paths.ForEach(path =>
        {
            if (!Directory.Exists(path)) return;
            try
            {
                SafeRemoveFile(path + MetaPrefix);
                Directory.Delete(path, true);
                Debug.Log($"[Airbridge] Deleted directory: {path}");
            }
            catch
            {
                /* ignored */
            }
        });
    }

    internal static void ReplaceDirectory(string source, string destination)
    {
        if (!Directory.Exists(source)) return;

        if (!Directory.Exists(destination))
        {
            Directory.CreateDirectory(destination);
        }

        var files = Directory.GetFiles(source);
        var directories = Directory.GetDirectories(source);

        foreach (var file in files)
        {
            var name = Path.GetFileName(file);
            var destinationFile = Path.Combine(destination, name);
            File.Copy(file, destinationFile, true);
        }

        foreach (var directory in directories)
        {
            var name = Path.GetDirectoryName(directory);
            if (name == null) continue;
            var destinationDirectory = Path.Combine(destination, name);
            ReplaceDirectory(directory, destinationDirectory);
        }
    }

    private static void SafeRemoveFile(string path)
    {
        if (!File.Exists(path)) return;
        try
        {
            File.Delete(path);
            Debug.Log($"[Airbridge] Deleted file: {path}");
        }
        catch
        {
            /* ignored */
        }
    }

    private static string ParentDirectory(this string path, int depth = 0)
    {
        if (depth < 0) depth = 0;

        var dir = path;
        for (var i = 0; i < depth; i++)
        {
            var parentDir = Path.GetDirectoryName(dir);
            if (parentDir == null) break;
            dir = parentDir;
        }

        return dir;
    }
}

#endif
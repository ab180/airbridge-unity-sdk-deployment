#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

internal abstract class AirbridgeScriptableObject : ScriptableObject
{
    private const string AssetPath = "Assets/Airbridge/Resources";
    private const string AssetExtension = ".asset";

    private static readonly Dictionary<string, AirbridgeScriptableObject> Instances =
        new Dictionary<string, AirbridgeScriptableObject>();

    public static T GetInstance<T>(string assetName) where T : AirbridgeScriptableObject
    {
        if (!Instances.ContainsKey(assetName))
        {
            Instances[assetName] = LoadResource<T>(assetName);
        }

        return Instances[assetName] as T;
    }

    private static T LoadResource<T>(string assetName) where T : AirbridgeScriptableObject
    {
        T obj = Resources.Load(assetName) as T;

        if (obj == null)
        {
            obj = CreateInstance<T>();
            if (!Directory.Exists(AssetPath))
            {
                Directory.CreateDirectory(AssetPath);
                AssetDatabase.Refresh();
            }

            string fullPath = Path.Combine(AssetPath, GetAssetFileName(assetName));
            AssetDatabase.CreateAsset(obj, fullPath);
        }

        return obj;
    }

    private static string GetAssetFileName(string assetName)
    {
        return assetName + AssetExtension;
    }
}

internal static class AirbridgeScriptableObjectExtension
{
    public static SerializedObject GetSerializedObject(this AirbridgeScriptableObject airbridgeScriptableObject)
    {
        return new SerializedObject(airbridgeScriptableObject);
    }

    public static void Save(this AirbridgeScriptableObject airbridgeScriptableObject)
    {
        EditorUtility.SetDirty(airbridgeScriptableObject);
        AssetDatabase.SaveAssets();
    }
}

#endif
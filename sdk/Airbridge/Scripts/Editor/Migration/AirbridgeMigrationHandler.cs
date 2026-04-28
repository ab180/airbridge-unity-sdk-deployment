#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

[InitializeOnLoad]
internal class AirbridgeMigrationHandler
{
    private const string Version = "4.9.2";

    private static string SavedVersionFilePath => Path.Combine(Application.dataPath, "Airbridge", "SavedVersion.txt");

    private static AirbridgeVersion SavedVersion
    {
        get
        {
            var version = !File.Exists(SavedVersionFilePath)
                ? "0.0.0"
                : File.ReadAllText(SavedVersionFilePath);

            return version.ToAirbridgeVersion();
        }
    }

    private static readonly List<AirbridgeMigration> Migrations = new List<AirbridgeMigration>()
    {
        new AirbridgeMigration.V4_9_0()
    };

    static AirbridgeMigrationHandler()
    {
        if (Application.isBatchMode) return;

        var sortedMigrations = Migrations
            .OrderBy(migration => migration.Version)
            .Where(migration => migration.Version.CompareTo(Version.ToAirbridgeVersion()) <= 0)
            .ToList();

        foreach (var migration in sortedMigrations
                     .TakeWhile(_ => SavedVersion != null)
                     .Where(migration => SavedVersion.CompareTo(migration.Version) < 0))
        {
            try
            {
                Debug.Log($"[Airbridge] Running migration for version: {migration.Version}");
                migration.Migrate();
            }
            catch
            {
                /* ignored */
            }
        }

        // Write latest version
        AirbridgeFileUtils.PrepareFile(SavedVersionFilePath);
        File.WriteAllText(SavedVersionFilePath, Version);
    }
}

#endif
#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEngine;

internal abstract class AirbridgeMigration
{
    internal AirbridgeVersion Version { get; }

    private AirbridgeMigration(int major, int minor, int patch)
    {
        Version = new AirbridgeVersion(major, minor, patch);
    }

    public abstract void Migrate();

    #region V4_9_0

    internal sealed class V4_9_0 : AirbridgeMigration
    {
        public V4_9_0() : base(4, 9, 0)
        {
        }

        public override void Migrate()
        {
            var editorPath = Path.Combine(AirbridgeFileUtils.GetPackageDataPath(), "Airbridge", "Scripts", "Editor");
            var oldMigrationDirPath =
                Path.Combine(AirbridgeFileUtils.GetPackageDataPath(), "Airbridge", "Scripts", "Migration");

            if (!Directory.Exists(Path.Combine(editorPath, "Migration")))
            {
                AirbridgeFileUtils.ReplaceDirectory(oldMigrationDirPath, Path.Combine(editorPath, "Migration"));
                AirbridgeFileUtils.SafeRemoveFiles(new List<string>()
                {
                    Path.Combine(editorPath, "Migration", "AirbridgeUnityVersion.cs"),
                    Path.Combine(editorPath, "Migration", "LatestVersion.txt"),
                });
            }

            AirbridgeFileUtils.SafeRemoveFiles(new List<string>()
            {
                Path.Combine(editorPath, "Extension", "ListExtension.cs"),

                Path.Combine(editorPath, "Utils", "ArrayDataUtils.cs"),
                Path.Combine(editorPath, "Utils", "GetPath.cs"),
                Path.Combine(editorPath, "Utils", "ReplaceDirectory.cs"),
            });

            AirbridgeFileUtils.SafeRemoveDirectories(new List<string>()
            {
                oldMigrationDirPath,
                Path.Combine(Application.dataPath, "Plugins", "Android", "Airbridge"),
                Path.Combine(AirbridgeFileUtils.GetPackageDataPath(), "Plugins", "Android", "Airbridge.androidlib"),
            });
        }
    }

    #endregion
}

#endif
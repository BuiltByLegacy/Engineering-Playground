using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EngineeringPlayground.Editor
{
    public sealed class EngineeringContentSync : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        [MenuItem("Engineering Playground/Sync Content To StreamingAssets")]
        public static void SyncContent()
        {
            var source = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "content"));
            var destination = Path.Combine(Application.streamingAssetsPath, "content");

            if (!Directory.Exists(source))
                throw new DirectoryNotFoundException($"Engineering Playground content directory was not found: {source}");

            if (Directory.Exists(destination))
                Directory.Delete(destination, true);

            CopyDirectory(source, destination);
            AssetDatabase.Refresh();
            Debug.Log($"Engineering Playground content synced to {destination}");
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            SyncContent();
        }

        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);

            foreach (var file in Directory.GetFiles(source))
            {
                var name = Path.GetFileName(file);
                File.Copy(file, Path.Combine(destination, name), true);
            }

            foreach (var directory in Directory.GetDirectories(source))
            {
                var name = Path.GetFileName(directory);
                CopyDirectory(directory, Path.Combine(destination, name));
            }
        }
    }
}

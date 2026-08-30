using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EngineeringPlayground.Editor
{
    public static class EngineeringWebGlBuild
    {
        private const string TempScenePath = "Assets/Editor/GeneratedPagesBootstrap.unity";

        public static void BuildPages()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var outputPath = Path.Combine(projectRoot, "build", "WebGL");

            try
            {
                EngineeringContentSync.SyncContent();

                if (Directory.Exists(outputPath))
                    Directory.Delete(outputPath, true);
                Directory.CreateDirectory(outputPath);

                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                if (!EditorSceneManager.SaveScene(scene, TempScenePath))
                    throw new InvalidOperationException($"Unable to save generated Pages bootstrap scene at {TempScenePath}.");

                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
                PlayerSettings.WebGL.decompressionFallback = false;

                var options = new BuildPlayerOptions
                {
                    scenes = new[] { TempScenePath },
                    locationPathName = outputPath,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None
                };

                var report = BuildPipeline.BuildPlayer(options);
                if (report.summary.result != BuildResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Engineering Playground WebGL build failed with result {report.summary.result} and {report.summary.totalErrors} errors.");
                }

                File.WriteAllText(Path.Combine(outputPath, ".nojekyll"), string.Empty);
                Debug.Log($"Engineering Playground Pages build completed: {outputPath}");
            }
            finally
            {
                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(TempScenePath) != null)
                {
                    AssetDatabase.DeleteAsset(TempScenePath);
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}

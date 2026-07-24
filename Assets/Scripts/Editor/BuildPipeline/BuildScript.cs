using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MoonRabbitRush.Editor.BuildPipeline
{
    /// <summary>
    /// CI entry point. GitHub Actions executeMethod:
    /// MoonRabbitRush.Editor.BuildPipeline.BuildScript.BuildWebGL
    /// </summary>
    public static class BuildScript
    {
        private const string OutputDirectory = "Build/WebGL";

        public static void BuildWebGL()
        {
            if (!TryGetEnabledScenes(out var scenes))
            {
                Fail("Build failed: no enabled scenes in Editor Build Settings.");
                return;
            }

            if (!EnsureWebGLActiveBuildTarget())
            {
                Fail("Build failed: could not switch active build target to WebGL.");
                return;
            }

            PrepareOutputDirectory(OutputDirectory);

            var buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputDirectory,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            };

            var report = UnityEditor.BuildPipeline.BuildPlayer(buildOptions);
            LogReportSummary(report);

            if (report.summary.result != BuildResult.Succeeded)
            {
                Fail($"Build failed: {report.summary.result}.");
                return;
            }

            Debug.Log($"WebGL build succeeded. Output: {Path.GetFullPath(OutputDirectory)}");
            EditorApplication.Exit(0);
        }

        private static bool TryGetEnabledScenes(out string[] scenes)
        {
            scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            return scenes.Length > 0;
        }

        private static bool EnsureWebGLActiveBuildTarget()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
            {
                return true;
            }

            return EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.WebGL,
                BuildTarget.WebGL);
        }

        private static void PrepareOutputDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }

            Directory.CreateDirectory(path);
        }

        private static void LogReportSummary(BuildReport report)
        {
            var summary = report.summary;
            Debug.Log(
                $"Build {summary.result}: platform={summary.platform}, " +
                $"output={summary.outputPath}, size={summary.totalSize} bytes, " +
                $"errors={summary.totalErrors}, warnings={summary.totalWarnings}");
        }

        private static void Fail(string message)
        {
            Debug.LogError(message);
            EditorApplication.Exit(1);
        }
    }
}

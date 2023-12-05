using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public class BuildScript
{
    public static void Build()
    {
        // Define the build path. Modify this path as needed.
        string buildPath = "Builds/RgnExample.apk";

        // Build player options.
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes
                                        .Where(scene => scene.enabled)
                                        .Select(scene => scene.path)
                                        .ToArray(),
            locationPathName = buildPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        // Perform build.
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Console.WriteLine("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            throw new Exception("Build failed");
        }
    }
}

using Newtonsoft.Json.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ImportPackageSamples
{
    [MenuItem("Tools/Import Package Examples")]
    public static void ImportPackageExamples()
    {
        string samplesPath = "Assets/Samples";
        if (Directory.Exists(samplesPath))
        {
            Directory.Delete(samplesPath, true);
        }
        Directory.CreateDirectory(samplesPath);

        string packageCachePath = Path.Combine(Directory.GetCurrentDirectory(), "Library/PackageCache");
        string[] packageDirectories = Directory.GetDirectories(packageCachePath, "io.getready.rgn*", SearchOption.AllDirectories);

        foreach (string packageDir in packageDirectories)
        {
            string packageJsonPath = Path.Combine(packageDir, "package.json");
            if (!File.Exists(packageJsonPath))
            {
                continue;
            }
            string packageJsonContent = File.ReadAllText(packageJsonPath);
            JObject packageJson = JObject.Parse(packageJsonContent);

            if (packageJson["samples"] == null)
            {
                continue;
            }
            JArray samples = (JArray)packageJson["samples"];
            if (samples.Count == 0)
            {
                continue;
            }
            string displayName = packageJson["displayName"].ToString().Replace("/", "_");
            string version = packageJson["version"].ToString();
            string sampleRootPath = Path.Combine(samplesPath, displayName, version);
            Directory.CreateDirectory(sampleRootPath);
            foreach (JObject sample in samples)
            {
                string sampleName = sample["displayName"].ToString().Replace("/", "_");
                string samplePath = sample["path"].ToString();
                string sampleFullPath = Path.Combine(packageDir, samplePath);
                string destinationPath = Path.Combine(sampleRootPath, sampleName);
                CopyDirectory(sampleFullPath, destinationPath);
            }
        }

        // Find and open the UIRootScene scene file
        string[] allSceneFiles = Directory.GetFiles(samplesPath, "*.unity", SearchOption.AllDirectories);
        string uiRootScenePath = string.Empty;
        foreach (var sceneFile in allSceneFiles)
        {
            if (Path.GetFileNameWithoutExtension(sceneFile).Equals("UIRootScene"))
            {
                uiRootScenePath = sceneFile;
                break;
            }
        }

        if (!string.IsNullOrEmpty(uiRootScenePath))
        {
            EditorSceneManager.OpenScene(uiRootScenePath);
        }

        AssetDatabase.Refresh();
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, destFile, true); // Added true to overwrite existing files
        }
        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            string destDir = Path.Combine(destinationDir, Path.GetFileName(dir));
            CopyDirectory(dir, destDir);
        }
    }
}

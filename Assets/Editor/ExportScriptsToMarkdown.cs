using UnityEditor;
using UnityEngine;
using System.IO;

public class ExportScriptsToMarkdown : EditorWindow
{
    [MenuItem("Tools/Export Scripts to Markdown")]
    public static void ExportScriptsToMd()
    {
        string scriptsPath = Application.dataPath; // Assets folder
        string outputDirectory = Path.Combine(Application.dataPath, "ScriptsMarkdown");

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        string[] scriptFiles = Directory.GetFiles(scriptsPath, "*.cs", SearchOption.AllDirectories);
        foreach (string scriptFile in scriptFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(scriptFile);
            string outputPath = Path.Combine(outputDirectory, $"{fileName}.md");

            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                writer.WriteLine($"# {fileName}");
                writer.WriteLine("```csharp");

                string fileContent = File.ReadAllText(scriptFile);
                writer.WriteLine(fileContent);

                writer.WriteLine("```");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"Scripts exported to Markdown files in: {outputDirectory}");
    }
}

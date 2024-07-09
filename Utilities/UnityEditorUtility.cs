#region
using System.Runtime.InteropServices;
using Spectre.Console;
using UnityOps.Models;
#endregion

namespace UnityOps.Utilities;

public class UnityEditorUtility
{
    public static List<UnityEditor> FindUnityEngines(string unityEditorsRootDirectory)
    {
        //Default editor directories
        //Windows  C:/Program Files/Unity/Hub/Editor/<version>/Editor/Unity.exe
        //Linux   Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/Linux/Unity
        List<UnityEditor> editors = new();
        try
        {
            string[] subDirectories = Directory.GetDirectories(unityEditorsRootDirectory);

            if (Program.isDebugging)
                AnsiConsole.MarkupLine($"[{Program.SuccessColor}][[Unity Editors Install Directory]][/] {unityEditorsRootDirectory}");

            foreach (string subDirectory in subDirectories)
            {
                var executablePathComponents = string.Empty;

                //install Directory > subDirectory(version) > "Editor/Unity.exe"
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    executablePathComponents = Path.Combine("Editor", "Unity.exe");

                //install Directory > subDirectory(version) > "Unity.app/Contents/Linux/Unity"
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    executablePathComponents = Path.Combine("Editor", "Unity.app", "Contents", "Linux", "Unity");

                string unityExecutableFile = Path.Combine(subDirectory, executablePathComponents);

                if (Path.Exists(unityExecutableFile))
                {
                    string executableDirectory = Path.Combine(unityEditorsRootDirectory, unityExecutableFile);
                    string version = Path.GetFileName(subDirectory.TrimEnd(Path.DirectorySeparatorChar));

                    UnityEditor unityEditor = new(executableDirectory, version);
                    editors.Add(unityEditor);

                    if (!Program.isDebugging)
                        continue;

                    AnsiConsole.MarkupLine($"[{Program.InfoColor}][[Unity Editor Utility]][/] Found Unity Editor Executable Directory {unityExecutableFile}");
                    AnsiConsole.MarkupLine($"[{Program.InfoColor}][[Unity Editor Utility]][/] Found Editor Version  {unityEditor.version}\n");

                }
                else
                    AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Unity Editor Utility]][/] Unity Editor Executable not found at {unityExecutableFile}");


            }
            return editors;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return null;
        }
    }

}
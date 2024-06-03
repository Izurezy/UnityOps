
using System.Runtime.InteropServices;
using Spectre.Console;
using UnityOps.Structs;

namespace UnityOps.Utilities
{
    public class UnityEditorUtility
    {
        public static List<UnityEditor> FindUnityEngines(string unityEditorsRootDirectory)
        {
            //Default editor directories
            //Windows  C:/Program Files/Unity/Hub/Editor/<version>/Editor/Unity.exe" -projectPath "<project path>
            //Linux   Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/Linux/Unity -projectPath <project path>
            List<UnityEditor> editors = new();
            UnityEditor unityEditor = new();
            if (string.IsNullOrEmpty(unityEditorsRootDirectory))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    unityEditorsRootDirectory = "C:/Program Files/Unity/Hub/Editor/<version>/Editor/Unity.exe";

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    unityEditorsRootDirectory = "Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/Linux/Unity";
            }

            try
            {
                var subDirectories = Directory.GetDirectories(unityEditorsRootDirectory);

                if (Program.isDebugging)
                    AnsiConsole.MarkupLine($"[yellow][[Unity Editors Install Directory]][/] {unityEditorsRootDirectory}");

                foreach (var subDirectory in subDirectories)
                {
                    string executablePathComponents = string.Empty;

                    //install Directory > subDirectory(version) > "Editor/Unity.exe"
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        executablePathComponents = Path.Combine("Editor", "Unity.exe");


                    //install Directory > subDirectory(version) > "Unity.app/Contents/Linux/Unity"
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        executablePathComponents = Path.Combine("Editor", "Unity.app", "Contents", "Linux", "Unity");

                    string unityExecutableFile = Path.Combine(subDirectory, executablePathComponents);

                    if (Path.Exists(unityExecutableFile))
                    {
                        unityEditor.editorExecutableDirectory = Path.Combine(unityEditorsRootDirectory, unityExecutableFile);
                        unityEditor.editorVersion = Path.GetFileName(subDirectory.TrimEnd(Path.DirectorySeparatorChar));
                        editors.Add(unityEditor);
                    }
                    else if (Program.isDebugging)
                        AnsiConsole.MarkupLine($"[red]Unity Editor Executable not found at {unityExecutableFile}[/]");

                    if (Program.isDebugging)
                    {
                        AnsiConsole.MarkupLine($"[yellow][[Unity Editor Executable Directory]][/] {unityExecutableFile}");
                        AnsiConsole.MarkupLine($"[yellow][[Editor Version]][/] {unityEditor.editorVersion}\n");
                    }
                }

                return editors;
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                return null;
            }
        }

        public static int GetProjectsMadeWithEditor(List<UnityProject> unityProjects, UnityEditor unityEditor)
        {
            int ProjectsMadeWithEditor = 0;

            foreach (var project in unityProjects)
            {
                if (project.projectEditorVersion == unityEditor.editorVersion && project.projectEditorVersion != null && unityEditor.editorVersion != null)
                    ProjectsMadeWithEditor++;
                else
                    AnsiConsole.MarkupLine($"[yellow]No matching editor found for project version {project.projectEditorVersion}[/]");
            }

            return ProjectsMadeWithEditor;
        }


    }
}
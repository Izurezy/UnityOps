using System.Diagnostics;
using Spectre.Console;
using UnityOps.Structs;

namespace UnityOps.Utilities
{
    public class UnityProjectUtility
    {
        private static readonly string ProjectVersionFile = "ProjectVersion.txt";
        public static async Task<List<UnityProject>> FindUnityProjects(string unityProjectsRootDirectory, string projectVersionDefaultFilePath)
        {
            List<UnityProject> unityProjects = new();
            string[] SubDirectories = Array.Empty<string>();

            if (string.IsNullOrEmpty(unityProjectsRootDirectory))
                AnsiConsole.MarkupLine("[red]Unity Projects Root Directory is null![/]");

            try
            {

                if (Directory.Exists(unityProjectsRootDirectory))
                    SubDirectories = Directory.GetDirectories(unityProjectsRootDirectory);
                else
                {
                    AnsiConsole.MarkupLine("[red]Unity Projects Directory not found, use UnityOps -config[/]");
                    Environment.Exit(1);
                }


                // Projects Root Directories / subdirectory(Projects)
                foreach (var subDirectory in SubDirectories)
                {

                    UnityProject unityProject = new()
                    {
                        unityProjectDirectory = subDirectory,
                        projectName = Path.GetFileName(subDirectory.TrimEnd(Path.DirectorySeparatorChar))
                    };

                    var versionNumber = await FindProjectVersionFileAsync(subDirectory, projectVersionDefaultFilePath);

                    unityProject.projectEditorVersion = string.IsNullOrEmpty(versionNumber) ? "Not Found" : versionNumber;

                    if (Program.isDebugging)
                        AnsiConsole.MarkupLine($"[yellow][[Unity Project Directory]][/] {unityProject.unityProjectDirectory}\n[yellow][[Project Version]][/] {unityProject.projectEditorVersion}\n");

                    if (unityProject.projectEditorVersion != null && unityProject.projectEditorVersion != "Not Found")
                        unityProjects.Add(unityProject);
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }


            return unityProjects;
        }

        // Does a recurse search from the project root directory and one subdirectory deep
        // e.g. MyUnityProjectRootDirectory / ProjectSettings / ProjectVersion.txt
        // It will not work if the subdirectory is MyUnityProjectRootDirectory / ProjectSettings / AnotherDirectory / ProjectVersion.txt

        public static async Task<string> FindProjectVersionFileAsync(string filePath, string projectVersionDefaultFilePath)
        {
            if (string.IsNullOrEmpty(projectVersionDefaultFilePath))
            {
                AnsiConsole.MarkupLine($"[red]project Version Default File Path is null, using Unity's default path ProjectSettings{Path.DirectorySeparatorChar}ProjectVersion.txt[/]");
                projectVersionDefaultFilePath = $"ProjectSettings{Path.DirectorySeparatorChar}ProjectVersion.txt";
            }

            try
            {
                var fullPath = Path.Combine(filePath, projectVersionDefaultFilePath);
                string VersionNumber;
                if (File.Exists(fullPath))
                {
                    if (Program.isDebugging)
                        AnsiConsole.MarkupLine($"[deeppink1][[Unity Editor Version]] File found in:{fullPath}[/]");

                    //if the returned value from is null then `VersionNumber` still has a value of "Not Found"
                    return VersionNumber = await GetProjectVersionNumberFromFile(fullPath);
                }
                else
                {
                    if (Program.isDebugging)
                        AnsiConsole.MarkupLine("[deeppink1][[Unity Editor Version]] file not found in default directory, doing a Recurse search[/]");

                    var SubDirectories = Directory.GetDirectories(filePath);

                    // Project Root Directory / subDirectories(Folders in the Projects directory)
                    foreach (var SubDirectory in SubDirectories)
                    {
                        var ProjectVersionFilePath = Path.Combine(SubDirectory, ProjectVersionFile);
                        if (File.Exists(ProjectVersionFilePath))
                        {
                            if (Program.isDebugging)
                                AnsiConsole.MarkupLine($"[deeppink1][[Unity Editor Version]] File found in: {SubDirectory}[/]");

                            //if the returned value from is null then `VersionNumber` still has a value of "Not Found"
                            return VersionNumber = await GetProjectVersionNumberFromFile(ProjectVersionFilePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return null;
        }

        // Gets The Project Version(Unity Editor Version) from the given filePath
        // e.g.MyUnityProjectRootDirectory / ProjectSettings / ProjectVersion.txt
        private static async Task<string> GetProjectVersionNumberFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string[] lines = await File.ReadAllLinesAsync(filePath);

                foreach (string line in lines)
                {
                    //17 instead of 16 because their a space between  m_EditorVersion: and version number
                    if (line.Contains("m_EditorVersion:"))
                        return line.Remove(0, 17);
                }
            }
            AnsiConsole.MarkupLine("[red]Project Version NOT FOUND within the Project Version File[/]");
            return string.Empty;
        }

        public static string OpenUnityProject(string projectName, List<UnityProject> unityProjects, List<UnityEditor> unityEditors)
        {
            AnsiConsole.MarkupLine($"[green]Opening {projectName}[/]");

            UnityProject project = UnityProject.FindProjectByProjectName(projectName, unityProjects);
            UnityEditor editor = UnityEditor.FindEditorByProjectVersion(project.projectEditorVersion, unityEditors);

            if (editor.Equals(default(UnityEditor)))
            {
                AnsiConsole.MarkupLine($"[red] No Editor is the same version as [yellow]{projectName}[/], Project Editor Version: [yellow]{project.projectEditorVersion}[/][/]");

                if (AnsiConsole.Confirm($"Do you want to open {projectName} with anyways a different Editor Version?", false))
                {
                    var SelectedEditorVersion = InputUtility.SelectionPrompt("Which editor?", unityEditors, editor => editor.editorVersion);

                    if (!string.IsNullOrEmpty(SelectedEditorVersion))
                        Console.WriteLine($"Selected Editor Version: {SelectedEditorVersion}");
                    else
                        AnsiConsole.MarkupLine("[red]Selected Editor Version is null[/]");

                    editor = UnityEditor.FindEditorByProjectVersion(SelectedEditorVersion, unityEditors);

                    if (Program.isDebugging)
                        AnsiConsole.MarkupLine($"Opening {projectName} with Editor Version: {editor.editorVersion} , Executable Directory {editor.editorExecutableDirectory}");
                }
            }

            try
            {
                ProcessStartInfo startInfo = new()
                {
                    //engineExecutablePath -projectPath UnityProjectPath
                    FileName = editor.editorExecutableDirectory,
                    Arguments = $" -projectPath \"{project.unityProjectDirectory}\" ",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,

                };
                AnsiConsole.MarkupLine($"[green]Starting editor[/] {editor.editorVersion}");
                using Process process = Process.Start(startInfo);

                string error = process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                    Console.WriteLine($"Error {error} ");

                AnsiConsole.MarkupLine($"[green]Successfully opened {projectName} with Editor {editor.editorVersion}[/]");

                return project.projectName;
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }

            return string.Empty;
        }
    }
}

using System.Diagnostics;
using System.Runtime.InteropServices;
using Spectre.Console;
using UnityOps.Structs;

namespace UnityOps.Utilities
{
    public class UnityProjectUtility
    {
        private static readonly string ProjectVersionFile = "ProjectVersion.txt";
        public static async Task<List<UnityProject>> FindUnityProjects()
        {
            List<UnityProject> unityProjects = new();
            string[] SubDirectories = Array.Empty<string>();
            try
            {

                if (Directory.Exists(Program.savedData.UnityProjectsRootDirectory))
                    SubDirectories = Directory.GetDirectories(Program.savedData.UnityProjectsRootDirectory);
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

                    var versionNumber = await FindProjectVersionFileAsync(subDirectory);

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
        public static async Task<string> FindProjectVersionFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(filePath, Program.savedData.ProjectVersionDefaultFilePath);
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

        public static string OpenUnityProject(string projectName)
        {
            UnityEditor editor = UnityEditor.FindEditorByProjectName(projectName, Program.savedData.UnityEditors);
            UnityProject project = UnityProject.FindProjectByProjectName(projectName, editor.projects);
            //engineExecutablePath -projectPath UnityProjectPath
            string FileName = string.Empty;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                FileName = "cmd.exe";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                FileName = "bash";

            var command = Path.Combine(editor.editorExecutableDirectory, " -projectPath", project.unityProjectDirectory);
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = FileName,
                RedirectStandardInput = true,
                UseShellExecute = false,
            };


            Process process = Process.Start(processStartInfo);
            process.StandardInput.WriteLine($" \"{editor.editorExecutableDirectory}\" -projectPath \"{project.unityProjectDirectory}\" ");
            process.StandardInput.AutoFlush = true;
            process.StandardInput.Close();
            process.WaitForExit();


            return string.Empty;
        }






    }
}
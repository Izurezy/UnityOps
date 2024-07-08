#region
using Spectre.Console;
using UnityOps.Models;
#endregion

namespace UnityOps.Utilities
{
    public class UnityProjectUtility
    {

        private const string ProjectVersionFile = "ProjectVersion.txt";

        public static async Task<List<UnityProject>> FindUnityProjects(string unityProjectsRootDirectory, string projectVersionDefaultFilePath)
        {
            List<UnityProject> unityProjects = [];
            string[] SubDirectories = [];

            if (string.IsNullOrEmpty(unityProjectsRootDirectory))
                AnsiConsole.MarkupLine($"[{Program.ErrorColor}]Unity Projects Root Directory is null![/]");

            try
            {

                if (Directory.Exists(unityProjectsRootDirectory))
                    SubDirectories = Directory.GetDirectories(unityProjectsRootDirectory);
                else
                {
                    AnsiConsole.MarkupLine($"[{Program.ErrorColor}]Unity Projects Directory not found, use UnityOps -config[/]");
                    Environment.Exit(1);
                }


                // Projects Root Directories / subdirectory(Projects)
                foreach (string subDirectory in SubDirectories)
                {

                    string versionNumber = await FindProjectVersionFileAsync(subDirectory, projectVersionDefaultFilePath);
                    string editorVersion = string.IsNullOrEmpty(versionNumber) ? "Not Found" : versionNumber;
                    string projectName = Path.GetFileName(subDirectory.TrimEnd(Path.DirectorySeparatorChar));
                    string projectDirectory = subDirectory;

                    UnityProject unityProject = new(projectName, projectDirectory, editorVersion);

                    if (Program.isDebugging)
                        AnsiConsole.MarkupLine($"[{Program.SuccessColor}][[Unity Project Directory]][/] {unityProject.directory}\n[{Program.SuccessColor}][[Project Version]][/] {unityProject.editorVersion}\n");

                    if (unityProject.editorVersion != null && unityProject.editorVersion != "Not Found")
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
        private static async Task<string> FindProjectVersionFileAsync(string filePath, string projectVersionDefaultFilePath)
        {
            if (string.IsNullOrEmpty(projectVersionDefaultFilePath))
            {
                AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Unity Project Utility]][/] project Version Default File Path is null, using Unity's default path ProjectSettings{Path.DirectorySeparatorChar}ProjectVersion.txt");
                projectVersionDefaultFilePath = $"ProjectSettings{Path.DirectorySeparatorChar}ProjectVersion.txt";
            }


            try
            {
                string fullPath = Path.Combine(filePath, projectVersionDefaultFilePath);
                if (File.Exists(fullPath))
                {
                    if (Program.isDebugging)
                        AnsiConsole.MarkupLine($"[{Program.InfoColor}][[Unity Project Utility]] Unity Editor Version File found in:{fullPath}[/]");

                    //if the returned value from is null then `VersionNumber` still has a value of "Not Found"
                    return await GetProjectVersionNumberFromFile(fullPath);
                }

                if (Program.isDebugging)
                    AnsiConsole.MarkupLine($"[{Program.InfoColor}][[Unity Project Utility]] Unity Editor Version file not found in default directory, doing a Recurse search[/]");

                string[] SubDirectories = Directory.GetDirectories(filePath);

                // Project Root Directory / subDirectories(Folders in the Projects directory)
                foreach (string SubDirectory in SubDirectories)
                {
                    string ProjectVersionFilePath = Path.Combine(SubDirectory, ProjectVersionFile);
                    if (!File.Exists(ProjectVersionFilePath))
                        continue;

                    if (Program.isDebugging)
                        AnsiConsole.MarkupLine($"[{Program.InfoColor}][[Unity Project Utility]] Unity Editor Version File found in: {SubDirectory}[/]");

                    //if the returned value from is null then `VersionNumber` still has a value of "Not Found"
                    return await GetProjectVersionNumberFromFile(ProjectVersionFilePath);
                }

            }
            catch (Exception e)
            {
                AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Unity Project Utility]][/] Unable to find Project Version File");
                AnsiConsole.WriteException(e);
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
                    if (line.Contains("m_EditorVersion:"))
                        //17 instead of 16 because there a space between `m_EditorVersion:` and the version number
                        return line.Remove(0, 17);
                }
            }
            AnsiConsole.MarkupLine($"[{Program.ErrorColor}]Project Version NOT FOUND within the Project Version File[/]");
            return string.Empty;
        }

        public static async Task ToggleShouldOpenRecentProjectAsync()
        {
            bool shouldOpenRecentProject = !DataManager.savedData.shouldOpenRecentProject;

            AnsiConsole.MarkupLine($"[{Program.InfoColor}][[Argument Handler]][/] Open Recent Project is {shouldOpenRecentProject}");
            await DataManager.UpdateJsonSectionAsync(shouldOpenRecentProject, nameof(shouldOpenRecentProject));
        }

    }
}
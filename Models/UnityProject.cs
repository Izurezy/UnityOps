#region
using Spectre.Console;
using UnityOps.Utilities;
#endregion

namespace UnityOps.Models
{
    public class UnityProject(string name, string directory, string editorVersion)
    {
        public string name = name;
        public string directory = directory;
        public string editorVersion = editorVersion;
        public List<Application> applicationsToOpenWithProject = [];

        public static UnityProject FindProjectByProjectName(string name, List<UnityProject> unityProjects)
        {
            if (unityProjects != null)
                return unityProjects.FirstOrDefault(project => project.name == name);

            AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Unity Program Model]][/] Unity Projects list is null, Check your config and try running UnityOps -f");
            return null;
        }

        public static async Task OpenAsync(List<UnityProject> unityProjects, List<UnityEditor> unityEditors, bool shouldOpenRecentProject = false, string lastProjectOpenedName = null)
        {
            string projectName;

            if (shouldOpenRecentProject && !string.IsNullOrEmpty(lastProjectOpenedName))
                projectName = lastProjectOpenedName;
            else
                projectName = InputUtility.SelectionPrompt("Witch Project?", unityProjects, unityProject => unityProject.name);

            var project = FindProjectByProjectName(projectName, unityProjects);
            var editor = UnityEditor.FindEditorByVersion(project.editorVersion, unityEditors);

            if (editor == null)
            {
                AnsiConsole.MarkupLine($"[{Program.WarningColor}][[Unity Project Utility]][/] No Editor has the same version as {projectName}, Project Editor Version: {project.editorVersion}");

                if (AnsiConsole.Confirm($"Do you want to open {projectName} with anyways a different Editor Version?", false))
                {
                    string SelectedEditorVersion = InputUtility.SelectionPrompt("Which editor?", unityEditors, editors => editors.version);

                    AnsiConsole.MarkupLine(!string.IsNullOrEmpty(SelectedEditorVersion)
                        ? $"[{Program.InfoColor}][[Unity Project Utility]][/] Selected Editor Version: {SelectedEditorVersion}"
                        : $"[{Program.ErrorColor}][[Unity Project Utility]][/] Selected Editor Version is null");

                    editor = UnityEditor.FindEditorByVersion(SelectedEditorVersion, unityEditors);

                    if (Program.isDebugging)
                        AnsiConsole.MarkupLine($"[{Program.SuccessColor}][[Unity Project Utility]][/] Opening {projectName} with Editor Version {editor.version} , Editor Executable Directory {editor.executableDirectory}");
                }
            }

            try
            {
                Application editorApplication = new($"Unity Editor {editor!.version}", editor.executableDirectory, $" -projectPath \"{project.directory}\" ");
                Application.Open(editorApplication);

                if (project.applicationsToOpenWithProject.Count > 0)
                {
                    if (Program.isDebugging)
                        AnsiConsole.MarkupLine($"[{Program.InfoColor}][[Unity Project Utility]][/] Starting Applications to open unity project");

                    project.applicationsToOpenWithProject.ForEach(application => Application.Open(application));
                }

                AnsiConsole.MarkupLine($"[{Program.SuccessColor}][[Unity Project Utility]][/] Opened {projectName}");
                lastProjectOpenedName = project.name;
                await DataManager.UpdateJsonSectionAsync(lastProjectOpenedName, nameof(lastProjectOpenedName));

            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("Unable to open Editor");
                AnsiConsole.WriteException(ex);
            }


        }

    }
}
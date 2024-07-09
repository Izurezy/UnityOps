#region
using Spectre.Console;
using UnityOps.Models;
#endregion

namespace UnityOps.Utilities
{
    public class ApplicationUtility
    {

        private static async Task AddApplicationsToOpenWithProject(List<Application> applications, List<UnityProject> unityProjects)
        {
            if (applications.Count <= 0)
            {
                AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Application Utility]][/] Application's aren't Found, have you added one?");
                Environment.Exit(0);
            }

            DisplayUtility.DisplayApplications(applications);

            List<string> selectedApplicationNames = InputUtility.MultiSelectionPrompt("Which applications to add?", applications, application => application.name);
            List<string> unityProjectNames = InputUtility.MultiSelectionPrompt("Which Unity Projects?", unityProjects, project => project.name);

            foreach (var project in unityProjectNames.Select(projectName => UnityProject.FindProjectByProjectName(projectName, unityProjects)))
            {
                project.applicationsToOpenWithProject = new(selectedApplicationNames.Select(applicationName => Application.FindApplicationByName(applicationName, applications)));

                await DataManager.UpdateUnityProject(project);
            }
        }

        private static async Task RemoveApplicationToOpenWithProjectAsync(List<Application> applications, List<UnityProject> unityProjects)
        {
            if (applications.Count <= 0)
            {
                AnsiConsole.MarkupLine($"[{Program.WarningColor}][[Application Utility]][/] Application's aren't Found");
                Environment.Exit(0);
            }

            DisplayUtility.DisplayApplications(applications);

            List<string> selectedApplicationNames = InputUtility.MultiSelectionPrompt("Which applications to remove?", applications, application => application.name);
            List<string> unityProjectNames = InputUtility.MultiSelectionPrompt("which applications to remove from Unity Projects?", unityProjects, project => project.name);

            foreach (string projectName in unityProjectNames)
                foreach (string applicationName in selectedApplicationNames)
                    await DataManager.RemoveApplicationsToOpenWithProjectAsync(projectName, applicationName);
        }

        public static async Task ApplicationManager(List<Application> applications, List<UnityProject> unityProjects)
        {
            switch (DisplayUtility.DisplaySelectionPrompt("Select One", ["Create Application", "Add Applications To Open With Project", "Remove Application To Open With Project"]))
            {
                case "Create Application":
                    await Application.Create(applications);
                    break;
                case "Add Applications To Open With Project":
                    await AddApplicationsToOpenWithProject(applications, unityProjects);
                    break;
                case "Remove Application To Open With Project":
                    await RemoveApplicationToOpenWithProjectAsync(applications, unityProjects);
                    break;
                default:
                    AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Application Menu]][/] Selection is null ");
                    break;
            }
        }


    }
}
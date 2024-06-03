using Spectre.Console;
using UnityOps.Utilities;

namespace UnityOps
{
    public class ArgumentHandler
    {
        private static SettingsModel savedData;
        private static SettingsModel unsavedData;

        public static async Task CheckArgsAsync(string[] args, SettingsModel SavedData)
        {
            if (savedData != null)
            {
                savedData = SavedData;
                ExecuteIfArgumentPresent(args, ["-d", "-debug"], () => Program.isDebugging = true);
                ExecuteIfArgumentPresent(args, ["-h", "-help"], HelpMessage, true);
                await ExecuteIfArgumentPresentAsync(args, ["-config"], async () => await SettingsModel.ConfigureSettings(savedData), true);
                await ExecuteIfArgumentPresentAsync(args, ["-f", "-find"], FindUnityProjectsAndEditors, false);
                await ExecuteIfArgumentPresentAsync(args, ["-a", "-auto"], ToggleShouldOpenRecentProject, false);
                await ExecuteIfArgumentPresentAsync(args, ["-o", "-open"], OpenProjectAsync, false);
            }
            else
                AnsiConsole.MarkupLine($"[red]Saved Data is null, please run UnityOps -config[/]");
        }

        #region  Arguments Methods
        private static async Task ToggleShouldOpenRecentProject()
        {
            if (savedData != null)
                unsavedData.shouldOpenRecentProject = !savedData.shouldOpenRecentProject;

            Console.WriteLine($"Open Recent Project is {unsavedData.shouldOpenRecentProject}");
            await DataManager.UpdateJsonSectionAsync(unsavedData.shouldOpenRecentProject, nameof(unsavedData.shouldOpenRecentProject), savedData.configFilePath);
        }
        private static async Task OpenProjectAsync()
        {
            Console.WriteLine("Open projects");
            if (savedData.shouldOpenRecentProject)
            {
                unsavedData.lastProjectOpenedName = UnityProjectUtility.OpenUnityProject(savedData.lastProjectOpenedName, savedData.unityProjects, savedData.unityEditors);
                return;
            }


            string projectName = InputUtility.SelectionPrompt("Which Project to open?", savedData.unityProjects, project => project.projectName);
            unsavedData.lastProjectOpenedName = UnityProjectUtility.OpenUnityProject(projectName, savedData.unityProjects, savedData.unityEditors);
            await DataManager.UpdateJsonSectionAsync(unsavedData.lastProjectOpenedName, nameof(unsavedData.lastProjectOpenedName), savedData.configFilePath);
        }
        private static async Task FindUnityProjectsAndEditors()
        {
            AnsiConsole.MarkupLine("[green]Searching...\n[/]");
            unsavedData.unityProjects = await UnityProjectUtility.FindUnityProjects(savedData.unityProjectsRootDirectory, savedData.projectVersionDefaultFilePath);
            unsavedData.unityEditors = UnityEditorUtility.FindUnityEngines(savedData.unityEditorsRootDirectory);

            await DataManager.UpdateJsonSectionAsync(unsavedData.unityProjects, nameof(unsavedData.unityProjects), savedData.configFilePath);
            await DataManager.UpdateJsonSectionAsync(unsavedData.unityEditors, nameof(unsavedData.unityEditors), savedData.configFilePath);

            DisplayUtility.DisplayEditorsAndProjectInTable(unsavedData.unityEditors, unsavedData.unityProjects);
        }
        private static void HelpMessage()
        {
            var table = new Table();

            table.Title("Commands!");
            table.AddColumn("Option");
            table.AddColumn(new TableColumn("Description").Centered());

            table.AddRow("-d | -debug", "Enables debugging");
            table.AddRow("-h | -help", "Show command line help");
            table.AddRow("-f | -find", "Looks for Unity Projects and Editors");
            table.AddRow("-o | -open", "Opens a Unity Project or the most recent project if the toggle is on");
            table.AddRow("-a | -auto", "Toggles whether -o | -open should open the most recent project. ");
            table.AddRow("-config", "Shows an interactive configurator");

            AnsiConsole.WriteLine("\n");
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine("\n");
        }

        #endregion

        private static void ExecuteIfArgumentPresent(string[] consoleArgs, string[] targetArgs, Action action, bool exitConsoleApp = false)
        {
            foreach (string arg in targetArgs)
                if (consoleArgs.Contains(arg))
                {
                    action();

                    if (exitConsoleApp)
                        Environment.Exit(0);
                    else
                        break;
                }
        }

        private static async Task ExecuteIfArgumentPresentAsync(string[] consoleArgs, string[] targetArgs, Func<Task> asyncAction, bool exitConsoleApp = false)
        {
            foreach (string arg in targetArgs)
                if (consoleArgs.Contains(arg))
                {
                    await asyncAction();

                    if (exitConsoleApp)
                        Environment.Exit(0);
                    else
                        break;
                }
        }


    }
}
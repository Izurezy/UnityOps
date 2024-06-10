using Spectre.Console;
using UnityOps.Utilities;

namespace UnityOps
{
    public class ArgumentHandler
    {
        private static SettingsModel savedData;
        private static SettingsModel unsavedData = new();

        public static async Task CheckArgsAsync(string[] args, SettingsModel SavedData)
        {
            savedData = SavedData;
            if (savedData != null)
            {
                ExecuteIfArgumentPresent(args, ["-d", "-debug"], () => Program.isDebugging = true);
                ExecuteIfArgumentPresent(args, ["-h", "-help"], HelpMessage, true);
                await ExecuteIfArgumentPresentAsync(args, ["-config"], async () => await SettingsModel.ConfigureSettings(savedData), true);
                await ExecuteIfArgumentPresentAsync(args, ["-f", "-find"], FindUnityProjectsAndEditorsAndDisplay, false);
                await ExecuteIfArgumentPresentAsync(args, ["-a", "-auto"], ToggleShouldOpenRecentProject, true);
                await ExecuteIfArgumentPresentAsync(args, ["-o", "-open"], OpenProjectAsync, true);
            }
            else if (savedData == null)
                AnsiConsole.MarkupLine($"[red]Saved Data is null, please run UnityOps -config[/]");
        }


        #region  Arguments Methods
        private static async Task ToggleShouldOpenRecentProject()
        {
            Console.WriteLine($"Before toggle: {savedData.shouldOpenRecentProject}");
            unsavedData.shouldOpenRecentProject = !savedData.shouldOpenRecentProject;

            Console.WriteLine($"Open Recent Project is {unsavedData.shouldOpenRecentProject}");
            await DataManager.UpdateJsonSectionAsync(unsavedData.shouldOpenRecentProject, nameof(unsavedData.shouldOpenRecentProject), savedData.configFilePath);
        }
        private static async Task OpenProjectAsync()
        {
            Console.WriteLine("Open projects");

            unsavedData.lastProjectOpenedName = UnityProjectUtility.OpenUnityProject(savedData.unityProjects, savedData.unityEditors, savedData.shouldOpenRecentProject, savedData.lastProjectOpenedName);
            await DataManager.UpdateJsonSectionAsync(unsavedData.lastProjectOpenedName, nameof(unsavedData.lastProjectOpenedName), savedData.configFilePath);
        }
        private static async Task FindUnityProjectsAndEditorsAndDisplay()
        {
            (unsavedData.unityProjects, unsavedData.unityEditors) = await Program.FindUnityProjectsAndEditors();
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
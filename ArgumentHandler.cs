#region
using Spectre.Console;
using UnityOps.Models;
using UnityOps.Utilities;
#endregion

namespace UnityOps
{
    public class ArgumentHandler
    {
        public static async Task CheckArgsAsync(string[] args)
        {
            ExecuteIfArgumentPresent(args, ["-d", "-debug"], () => Program.isDebugging = true);
            ExecuteIfArgumentPresent(args, ["-h", "-help"], HelpMessage, true);
            await ExecuteIfArgumentPresentAsync(args, ["-config"], Configurator.ConfigureSettings, true);

            if (!await DataManager.IsDataPresentInConfigFileAsync())
            {
                AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Data Manager]][/] No data is found within Settings.json!");
                AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Data Manager]][/] Please run UnityOps -config");
                Environment.Exit(1);
            }

            await ExecuteIfArgumentPresentAsync(args, ["-p", "-petty"], async () => await TogglePettyTables());
            await ExecuteIfArgumentPresentAsync(args, ["-a", "-auto"], UnityProjectUtility.ToggleShouldOpenRecentProjectAsync);
            await ExecuteIfArgumentPresentAsync(args, ["-f", "-find"], Program.FindUnityProjectsAndEditorsAndDisplay, true);
            await ExecuteIfArgumentPresentAsync(args, ["-o", "-open"], async () =>
            {
                if (!DataManager.DoesUnityProjectAndEditorExist())
                    await Program.FindUnityProjectsAndEditorsAndDisplay();

                await UnityProject.OpenAsync(DataManager.savedData.unityProjects, DataManager.savedData.unityEditors, DataManager.savedData.shouldOpenRecentProject, DataManager.savedData.lastProjectOpenedName);
            }
            , true);
        }

        private static void HelpMessage()
        {
            var table = new Table();

            table.Title("Commands!");
            table.AddColumn("Option");
            table.AddColumn(new TableColumn("Description").Centered());
            table.AddRow("-config", "Shows an interactive configurator");
            table.AddRow("-d | -debug", "Enables debugging");
            table.AddRow("-f | -find", "Looks for Unity Projects and Editors");
            table.AddRow("-p | -petty", "Toggles Spectre.Console Tables");
            table.AddRow("-o | -open", "Opens a Unity Project or the most recent project if toggle on");
            table.AddRow("-a | -auto", "Toggles whether -o | -open should open the most recent project");
            table.AddRow("-h | -help", "Shows command line help");

            AnsiConsole.WriteLine("\n");
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine("\n");
        }

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

        private static async Task TogglePettyTables()
        {
            bool usePettyTables = !DataManager.savedData.usePettyTables;

            AnsiConsole.MarkupLine($"[{Program.InfoColor}][[Argument Handler]][/] Use Petty Tables: {usePettyTables}");
            await DataManager.UpdateJsonSectionAsync(usePettyTables, nameof(usePettyTables));
        }

    }
}
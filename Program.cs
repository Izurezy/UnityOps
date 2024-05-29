using System.Runtime.InteropServices;
using Spectre.Console;
using UnityOps.Utilities;

namespace UnityOps;

public class Program
{
    public static bool isDebugging = false;

    private static SettingsModel _SavedData;
    public static SettingsModel savedData
    {
        get => _SavedData;
        private set => _SavedData = value;
    }

    static SettingsModel unsavedData = new();
    static readonly string defaultSettingsPath = Path.Combine(Environment.CurrentDirectory, "Settings.json");

    static string defaultUnityEditorPath;


    /* ---------------------------------------------------
                        TODO
            Add descriptions to all foundations 
            test if Opening unity on linux works
            Fix the space in the Configurator
            ------------------------------------------
            Make All function stop using Program.savedData and pass in the prams require 
            for better code readability and  maintainability
            ------------------------------------------
            Add some docs
            Add Read me
            Fix Error when change configuration location
            Fix get proper permission for saving configuration file in custom directory. Known issue on linux and Window.

     --------------------------------------------------- */
    public static async Task Main(string[] args)
    {
        savedData = await DataManager.LoadFromJsonFile<SettingsModel>(defaultSettingsPath);

        #region Args
        if (args.Contains("-debug") || args.Contains("-d"))
            isDebugging = true;
        if (args.Contains("-help") || args.Contains("-h"))
        {
            // Create a table
            var table = new Table();

            // Add some columns
            table.Title("Commands!");
            table.AddColumn("Option");
            table.AddColumn(new TableColumn("Description").Centered());

            // Add some rows with your command line options and descriptions
            table.AddRow("-d | -debug", "Enables debugging");
            table.AddRow("-h | -help", "Show command line help");
            table.AddRow("-f | -find", "Looks for Unity Projects and Editors");
            table.AddRow("-config", "Shows an interactive configurator");

            AnsiConsole.WriteLine("\n");
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine("\n");
            Environment.Exit(0);
        }
        if (args.Contains("-setup") || args.Contains("-config"))
        {
            await SettingsModel.ConfigureSettings();
            Environment.Exit(0);
        }
        if (args.Contains("-f") || args.Contains("-find"))
        {
            await FindUnityProjectsAndEditors();
            await DataManager.UpdateJsonSectionAsync(unsavedData.UnityProjects, nameof(unsavedData.UnityProjects), savedData.ConfigFilePath);
            await DataManager.UpdateJsonSectionAsync(unsavedData.UnityEditors, nameof(unsavedData.UnityEditors), savedData.ConfigFilePath);
            Environment.Exit(0);
        }
        if (args.Contains("-o") || args.Contains("-open"))
        {
            string projectName = SelectProjectToOpenFromName();
            AnsiConsole.MarkupLine($"[green]Opening {projectName}[/]");
            UnityProjectUtility.OpenUnityProject(projectName);

            Environment.Exit(0);
        }
        #endregion

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            defaultUnityEditorPath = "C:/Program Files/Unity/Hub/Editor/";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            defaultUnityEditorPath = "/Applications/Unity/Hub/Editor/";

        if (savedData == null)
        {
            await SettingsModel.ConfigureSettings();
            savedData = await DataManager.LoadFromJsonFile<SettingsModel>(defaultSettingsPath);
        }

        if (savedData.UnityEditors == null || savedData.UnityProjects == null)
        {
            await FindUnityProjectsAndEditors();

            await DataManager.UpdateJsonSectionAsync(unsavedData.UnityProjects, nameof(unsavedData.UnityProjects), savedData.ConfigFilePath);
            await DataManager.UpdateJsonSectionAsync(unsavedData.UnityEditors, nameof(unsavedData.UnityEditors), savedData.ConfigFilePath);
            savedData ??= await DataManager.LoadFromJsonFile<SettingsModel>(defaultSettingsPath);
        }


        AnsiConsole.WriteLine("\n\n");

        // string projectName = SelectProjectToOpenFromName();
        // Console.WriteLine(projectName);
        // UnityProjectUtility.OpenUnityProject(projectName);


        AnsiConsole.WriteLine("\n\n");
    }

    private static string SelectProjectToOpenFromName()
    {
        var choicesList = new List<string>();

        foreach (var project in savedData.UnityProjects)
            choicesList.Add(project.projectName);

        string[] choices = choicesList.ToArray();

        string ProjectToOpen = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Which Project to open?")
                .PageSize(10)
                .MoreChoicesText("[blue](Move up and down to reveal more projects)[/]")
                .AddChoices(choices));

        return ProjectToOpen;
    }

    private static async Task FindUnityProjectsAndEditors()
    {
        AnsiConsole.MarkupLine("Searching...\n\n");
        unsavedData.UnityProjects = await UnityProjectUtility.FindUnityProjects();

        var projectsTable = new Table();
        projectsTable.Title("[blue]Unity Projects Found[/]");
        projectsTable.AddColumn("Project Name");
        projectsTable.AddColumn("Project Directory");
        projectsTable.AddColumn("Project Editor Version");
        projectsTable.Border(TableBorder.Rounded);
        projectsTable.SafeBorder();

        foreach (var project in unsavedData.UnityProjects)
            projectsTable.AddRow(project.projectName, project.unityProjectDirectory, project.projectEditorVersion);

        AnsiConsole.Write(projectsTable);


        unsavedData.UnityEditors = UnityEditorUtility.FindUnityEngines();

        foreach (var editor in unsavedData.UnityEditors)
            UnityEditorUtility.GetProjectsMadeWithEditor(unsavedData.UnityProjects, editor);


        var editorsTable = new Table();
        editorsTable.Title("[blue]Unity Editors Found[/]");
        editorsTable.AddColumn("Executable");
        editorsTable.AddColumn("Version");
        editorsTable.AddColumn("Projects");

        editorsTable.Border(TableBorder.Rounded);
        editorsTable.SafeBorder();


        foreach (var editor in unsavedData.UnityEditors)
        {
            editorsTable.AddRow(editor.editorExecutableDirectory, editor.editorVersion, editor.projects.Count.ToString());
        }

        AnsiConsole.Write(editorsTable);

        AnsiConsole.MarkupLine($"[green]Done ![/]");


        AnsiConsole.WriteLine("\n");
        AnsiConsole.MarkupLine($"[blue]{unsavedData.UnityProjects.Count} Projects was found[/]");
        AnsiConsole.MarkupLine($"[blue]{unsavedData.UnityEditors.Count} Editors was found[/]");
        AnsiConsole.WriteLine("\n");
    }
}

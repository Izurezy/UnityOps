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


    /* ---------------------------------------------------
                        TODO
            Add descriptions to all foundations 
            test if Opening unity on linux works
            ------------------------------------------
            Make a function to make adding arg more modular 
            Make All function stop using Program.savedData and pass in the prams require 
            for better code readability and  maintainability
            ------------------------------------------
            Add some docs
            Fix the space in the Configurator
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
            string projectName = InputUtility.SelectionPrompt("Which Project to open?", savedData.UnityProjects, project => project.projectName);
            AnsiConsole.MarkupLine($"[green]Opening {projectName}[/]");

            unsavedData.LastProjectOpenedName = UnityProjectUtility.OpenUnityProject(projectName);
            await DataManager.UpdateJsonSectionAsync(unsavedData.LastProjectOpenedName, nameof(unsavedData.LastProjectOpenedName), savedData.ConfigFilePath);
            Environment.Exit(0);
        }
        if (args.Contains("-a") || args.Contains("-auto"))
        {
            if (savedData != null)
                unsavedData.autoOpenRecentProject = !savedData.autoOpenRecentProject;

            Console.WriteLine($"Auto Open Recent Project is {unsavedData.autoOpenRecentProject}");
            await DataManager.UpdateJsonSectionAsync(unsavedData.autoOpenRecentProject, nameof(unsavedData.autoOpenRecentProject), savedData.ConfigFilePath);

            Environment.Exit(0);
        }
        #endregion

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


        if (savedData.autoOpenRecentProject)
            UnityProjectUtility.OpenUnityProject(savedData.LastProjectOpenedName);
    }

    private static async Task FindUnityProjectsAndEditors()
    {
        AnsiConsole.MarkupLine("[green]Searching...\n[/]");
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

        AnsiConsole.MarkupLine("\n");
        unsavedData.UnityEditors = UnityEditorUtility.FindUnityEngines();



        var editorsTable = new Table();
        editorsTable.Title("[blue]Unity Editors Found[/]");
        editorsTable.AddColumn("Executable");
        editorsTable.AddColumn("Version");
        editorsTable.AddColumn("Projects");

        editorsTable.Border(TableBorder.Rounded);
        editorsTable.SafeBorder();


        foreach (var editor in unsavedData.UnityEditors)
        {

            int ProjectsMadeWithEditor = UnityEditorUtility.GetProjectsMadeWithEditor(unsavedData.UnityProjects, editor);
            editorsTable.AddRow(editor.editorExecutableDirectory, editor.editorVersion, ProjectsMadeWithEditor.ToString());
        }


        AnsiConsole.Write(editorsTable);

        AnsiConsole.MarkupLine($"[green]Done![/]");


        AnsiConsole.WriteLine("\n");
        AnsiConsole.MarkupLine($"[blue]{unsavedData.UnityProjects.Count} Projects where found[/]");
        AnsiConsole.MarkupLine($"[blue]{unsavedData.UnityEditors.Count} Editors where found[/]");
        AnsiConsole.WriteLine("\n");
    }
}

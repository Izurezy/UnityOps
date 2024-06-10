using System.Diagnostics;
using System.Runtime.InteropServices.Marshalling;
using Spectre.Console;
using UnityOps.Structs;
using UnityOps.Utilities;

namespace UnityOps;

public class Program
{
    public static bool isDebugging = false;

    public static SettingsModel savedData { get; private set; }

    static SettingsModel unsavedData = new();
    static readonly string defaultSettingsPath = Path.Combine(Environment.CurrentDirectory, "Settings.json");

    /* ---------------------------------------------------
                        TODO
        ------------------------------------------
            Fix the space in the Configurator
            Fix Error when changing configuration location
            Fix get proper permission for saving configuration file in custom directory. Known issue on linux and Window.
        ------------------------------------------------------------------------------------
            Refactor SettingsModel
            revamp the saving system, thats allows the user to select which settings to change
            test if Opening unity on linux works
        ------------------------------------------------------------------------------------
            Add some docs
            Add descriptions to all foundations 
            Add an arg that display the current config settings
        ------------------------------------------------------------------------------------
            Create a future branch
            Create Unit test        

            Create a menu for opening project, change project editor version, etc (WIP)
            Allow the user to select which applications:
                1. to always open, no matter the project or editor
                2. they would like to open with a specific unity project or editor


        ------------------------------------------------------------------------------------
            
     --------------------------------------------------- */
    public static async Task Main(string[] args)
    {
        savedData = await DataManager.LoadFromJsonFile<SettingsModel>(defaultSettingsPath);
        await ArgumentHandler.CheckArgsAsync(args, savedData);

        if (savedData == null)
        {
            AnsiConsole.MarkupLine("[red]Saved settings is null...starting configuration process[/]");
            await SettingsModel.ConfigureSettings(savedData);
            savedData = await DataManager.LoadFromJsonFile<SettingsModel>(defaultSettingsPath);
        }
        OpenApplication("C:/Users/rryan/AppData/Local/Programs/Notion/Notion.exe");

        string selection = InputUtility.MainMenuSelectionPrompt();

        switch (selection)
        {
            case "Config":
                await SettingsModel.ConfigureSettings(savedData);
                break;
            case "Open Project":
                UnityProjectUtility.OpenUnityProject(savedData.unityProjects, savedData.unityEditors, savedData.shouldOpenRecentProject, savedData.lastProjectOpenedName);
                break;
            case "Add Applications To Open":
                break;
            case "Edit Projects":
                break;
            case "Edit Editors":
                break;
            default:
                AnsiConsole.MarkupLine("[red]selection is null[/]");
                break;
        }
    }

    public static async Task<(List<UnityProject>, List<UnityEditor>)> FindUnityProjectsAndEditors()
    {
        AnsiConsole.MarkupLine("[green]Searching...1\n[/]");
        unsavedData.unityProjects = await UnityProjectUtility.FindUnityProjects(savedData.unityProjectsRootDirectory, savedData.projectVersionDefaultFilePath);
        unsavedData.unityEditors = UnityEditorUtility.FindUnityEngines(savedData.unityEditorsRootDirectory);

        await DataManager.UpdateJsonSectionAsync(unsavedData.unityProjects, nameof(unsavedData.unityProjects), savedData.configFilePath);
        await DataManager.UpdateJsonSectionAsync(unsavedData.unityEditors, nameof(unsavedData.unityEditors), savedData.configFilePath);
        return (unsavedData.unityProjects, unsavedData.unityEditors);
    }

    public static void OpenApplication(string executableFullPath, string arguments = null)
    {
        try
        {
            ProcessStartInfo startInfo = new(executableFullPath, arguments);
            Process process = new()
            {
                StartInfo = startInfo
            };
            process.Start();
            Console.WriteLine("Application Started!!!");

        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

}


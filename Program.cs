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
            Create a menu for opening project, change project editor version, etc
        ------------------------------------------------------------------------------------
            
     --------------------------------------------------- */
    public static async Task Main(string[] args)
    {
        savedData = await DataManager.LoadFromJsonFile<SettingsModel>(defaultSettingsPath);

        await ArgumentHandler.CheckArgsAsync(args, savedData);

        if (savedData == null)
        {
            await SettingsModel.ConfigureSettings(savedData);
            savedData = await DataManager.LoadFromJsonFile<SettingsModel>(defaultSettingsPath);
        }

        //Create a validator class or something...
        if (savedData.unityEditors == null || savedData.unityProjects == null)
        {
            AnsiConsole.MarkupLine("[green]Searching...\n[/]");
            unsavedData.unityProjects = await UnityProjectUtility.FindUnityProjects(savedData.unityProjectsRootDirectory, savedData.projectVersionDefaultFilePath);
            unsavedData.unityEditors = UnityEditorUtility.FindUnityEngines(savedData.unityEditorsRootDirectory);

            await DataManager.UpdateJsonSectionAsync(unsavedData.unityProjects, nameof(unsavedData.unityProjects), savedData.configFilePath);
            await DataManager.UpdateJsonSectionAsync(unsavedData.unityEditors, nameof(unsavedData.unityEditors), savedData.configFilePath);

            DisplayUtility.DisplayEditorsAndProjectInTable(unsavedData.unityEditors, unsavedData.unityProjects);
        }


    }


}


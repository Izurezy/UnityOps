#region
using Spectre.Console;
using UnityOps.Models;
using UnityOps.Utilities;
#endregion

namespace UnityOps;

public class Program
{
    public static bool isDebugging { get; set; }

    public const string InfoColor = "bold deeppink1";
    public const string InfoColor2 = "bold blue";
    public const string ErrorColor = "bold red";
    public const string WarningColor = "bold yellow";
    public const string SuccessColor = "bold green";

    public static async Task Main(string[] args)
    {
        DataManager.ValidateConfigFile();

        if (!args.Contains("-config"))
            await DataManager.LoadSettings();

        await ArgumentHandler.CheckArgsAsync(args);

        switch (InputUtility.MainMenuSelectionPrompt())
        {
            case "Config":
                await Configurator.ConfigureSettings();
                break;
            case "Find Unity Editors and Projects":
                await FindUnityProjectsAndEditorsAndDisplay();
                break;
            case "Open Project":
                if (!DataManager.DoesUnityProjectAndEditorExist())
                    await FindUnityProjectsAndEditorsAndDisplay();
                await UnityProject.OpenAsync(DataManager.savedData.unityProjects, DataManager.savedData.unityEditors, DataManager.savedData.shouldOpenRecentProject, DataManager.savedData.lastProjectOpenedName);
                Environment.Exit(0);
                break;
            case "Manage Applications":
                await ApplicationUtility.ApplicationManager(DataManager.savedData.applications, DataManager.savedData.unityProjects);
                break;
            default:
                AnsiConsole.MarkupLine($"[{ErrorColor}][[Main Menu]][/] Selection was null ");
                break;
        }
    }

    public static async Task FindUnityProjectsAndEditorsAndDisplay()
    {
        (List<UnityProject> unityProjects, List<UnityEditor> unityEditors) = await FindUnityProjectsAndEditors();
        DisplayUtility.DisplayFoundEditorsAndProject(unityEditors, unityProjects);
    }
    private static async Task<(List<UnityProject>, List<UnityEditor>)> FindUnityProjectsAndEditors()
    {
        AnsiConsole.MarkupLine($"[{InfoColor}]Searching...\n[/]");
        List<UnityProject> unityProjects = await UnityProjectUtility.FindUnityProjects(DataManager.savedData.unityProjectsRootDirectory, DataManager.savedData.projectVersionDefaultRelativeFilePath);
        List<UnityEditor> unityEditors = UnityEditorUtility.FindUnityEngines(DataManager.savedData.unityEditorsRootDirectory);

        foreach (var project in unityProjects)
            await DataManager.UpdateUnityProject(project);

        foreach (var editor in unityEditors)
            await DataManager.UpdateUnityEditor(editor);

        return (unityProjects, unityEditors);
    }
}
using System.Runtime.InteropServices;
using Spectre.Console;
using UnityOps.Structs;

namespace UnityOps;
public class SettingsModel
{
    public string unityProjectsRootDirectory = "";
    public string projectVersionDefaultFilePath = $"ProjectSettings{Path.DirectorySeparatorChar}ProjectVersion.txt";
    public string unityEditorsRootDirectory = string.Empty;
    public string lastProjectOpenedName = string.Empty;
    public bool shouldOpenRecentProject = false;

    public List<UnityProject> unityProjects = new();
    public List<UnityEditor> unityEditors = new();
    public List<UnityProject> hiddenUnityProjects = new();
    public List<UnityEditor> hiddenUnityEditors = new();
    public List<Application> applications = new();

    public string configFilePath = Path.Combine(Environment.CurrentDirectory, "Settings.json");

    public static async Task ConfigureSettings(SettingsModel savedData)
    {
        SettingsModel unsavedData = new();

        unsavedData.unityProjectsRootDirectory = AnsiConsole.Prompt(
            new TextPrompt<string>($"Specify the where your Unity projects are:")
            .Validate(filePath =>
            {
                if (Directory.Exists(filePath))
                    return ValidationResult.Success();
                else
                    return ValidationResult.Error("[red]Path must exist[/]");
            }
        ));

        //e.g. UnityProjectsRootDirectory / Project / ProjectSettings / ProjectVersion
        unsavedData.projectVersionDefaultFilePath = AnsiConsole.Prompt(
            new TextPrompt<string>($"Default ProjectVersion Directory for all projects [blue][[Default]][/]")
            .DefaultValueStyle("blue")
            .DefaultValue(unsavedData.projectVersionDefaultFilePath)
            .Validate(filePath =>
            {

                if (!string.IsNullOrEmpty(filePath) && filePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                    return ValidationResult.Error("[red]Path must exist[/]");

                return ValidationResult.Success();
            }
        ));

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            unsavedData.unityEditorsRootDirectory = "Applications/Unity/Hub/Editor/";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            unsavedData.unityEditorsRootDirectory = "C:/Program Files/Unity/Hub/Editor";

        //e.g. Windows: "C:/Program Files/Unity/Hub/Editor"
        unsavedData.unityEditorsRootDirectory = AnsiConsole.Prompt(
            new TextPrompt<string>($"Directory with Unity Editors are installed[blue][[Default]][/]")
            .DefaultValueStyle("blue")
            .DefaultValue(unsavedData.unityEditorsRootDirectory)
            .Validate(filePath =>
            {
                if (Directory.Exists(filePath))
                    return ValidationResult.Success();
                else
                    return ValidationResult.Error("[red]Path must exist[/]");
            }
        ));

        unsavedData.configFilePath = AnsiConsole.Prompt(
            new TextPrompt<string>($"Enter the directory where the configuration is saved [blue][[Default]][/]")
            .DefaultValueStyle("blue")
            .DefaultValue(unsavedData.configFilePath)
            .Validate(directory =>
            {
                if (Directory.Exists(directory))
                    return ValidationResult.Success();
                else
                    return ValidationResult.Error("[red]Path must exist[/]");

            })
        );


        #region  Unsaved Configuration Table
        AnsiConsole.Clear();
        AnsiConsole.WriteLine("\n");

        var table = new Table();
        table.AddColumn("Option");
        table.AddColumn("Path");

        table.AddRow("Unity Projects Root Directory", unsavedData.unityProjectsRootDirectory);
        table.AddRow("ProjectVersion Default File Path", unsavedData.projectVersionDefaultFilePath);
        table.AddRow("Unity Editors Directory", unsavedData.unityEditorsRootDirectory);
        table.AddRow("Config File Path", unsavedData.configFilePath);

        AnsiConsole.Write(table);

        AnsiConsole.WriteLine("\n");
        #endregion

        if (AnsiConsole.Confirm("[green]Save?[/]"))
        {
            AnsiConsole.MarkupLine("[green]Saving To File...![/]");

            if (savedData == null)
                await DataManager.SaveToJsonFileAsync(unsavedData, unsavedData.configFilePath);
            else
            {
                await DataManager.UpdateJsonSectionAsync(unsavedData.unityProjectsRootDirectory, nameof(unsavedData.unityProjectsRootDirectory), unsavedData.configFilePath);
                await DataManager.UpdateJsonSectionAsync(unsavedData.projectVersionDefaultFilePath, nameof(unsavedData.projectVersionDefaultFilePath), unsavedData.configFilePath);
                await DataManager.UpdateJsonSectionAsync(unsavedData.unityEditorsRootDirectory, nameof(unsavedData.unityEditorsRootDirectory), unsavedData.configFilePath);

                //Deletes the old config File
                // if (Path.Exists(savedData?.ConfigFilePath))
                // File.Delete(savedData.ConfigFilePath);

                string SettingsFile = Path.Combine(unsavedData.configFilePath, "Settings.json");
                if (Path.Exists(unsavedData.configFilePath) && !File.Exists(SettingsFile))
                {
                    try
                    {
                        File.Create(SettingsFile);
                    }
                    catch (Exception ex)
                    {

                        AnsiConsole.WriteException(ex);
                    }
                }
                await DataManager.UpdateJsonSectionAsync(unsavedData.configFilePath, nameof(unsavedData.configFilePath), unsavedData.configFilePath);
            }
        }
        // return unsavedData;
    }
}
using System.Runtime.InteropServices;
using Spectre.Console;
using UnityOps.Structs;

namespace UnityOps;
public class SettingsModel
{
    public string UnityProjectsRootDirectory = "";
    public string ProjectVersionDefaultFilePath = $"ProjectSettings{Path.DirectorySeparatorChar}ProjectVersion.txt";
    public string UnityEditorsDirectory = "C:/Program Files/Unity/Hub/Editor";
    public List<UnityProject> UnityProjects = new();
    public List<UnityEditor> UnityEditors = new();
    public string ConfigFilePath = Path.Combine(Environment.CurrentDirectory, "Settings.json");

    public static async Task<SettingsModel> ConfigureSettings()
    {
        SettingsModel unsavedData = new();

        unsavedData.UnityProjectsRootDirectory = AnsiConsole.Prompt(
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
        unsavedData.ProjectVersionDefaultFilePath = AnsiConsole.Prompt(
            new TextPrompt<string>($"Default ProjectVersion Directory for all projects [blue][[Default]][/]")
            .DefaultValueStyle("blue")
            .DefaultValue(unsavedData.ProjectVersionDefaultFilePath)
            .Validate(filePath =>
            {

                if (!string.IsNullOrEmpty(filePath) && filePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                    return ValidationResult.Error("[red]Path must exist[/]");

                return ValidationResult.Success();
            }
        ));

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            unsavedData.UnityEditorsDirectory = "Applications/Unity/Hub/Editor/";

        //e.g. Windows: "C:/Program Files/Unity/Hub/Editor"
        unsavedData.UnityEditorsDirectory = AnsiConsole.Prompt(
            new TextPrompt<string>($"Directory with Unity Editors are installed[blue][[Default]][/]")
            .DefaultValueStyle("blue")
            .DefaultValue(unsavedData.UnityEditorsDirectory)
            .Validate(filePath =>
            {
                if (Directory.Exists(filePath))
                    return ValidationResult.Success();
                else
                    return ValidationResult.Error("[red]Path must exist[/]");
            }
        ));

        unsavedData.ConfigFilePath = AnsiConsole.Prompt(
            new TextPrompt<string>($"Enter the directory where the configuration is saved [blue][[Default]][/]")
            .DefaultValueStyle("blue")
            .DefaultValue(unsavedData.ConfigFilePath)
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

        table.AddRow("Unity Projects Root Directory", unsavedData.UnityProjectsRootDirectory);
        table.AddRow("ProjectVersion Default File Path", unsavedData.ProjectVersionDefaultFilePath);
        table.AddRow("Unity Editors Directory", unsavedData.UnityEditorsDirectory);
        table.AddRow("Config File Path", unsavedData.ConfigFilePath);

        AnsiConsole.Write(table);

        AnsiConsole.WriteLine("\n");
        #endregion

        if (AnsiConsole.Confirm("[green]Save?[/]"))
        {
            AnsiConsole.MarkupLine("[green]Saving To File...![/]");

            if (Program.savedData == null)
                await DataManager.SaveToJsonFileAsync(unsavedData, unsavedData.ConfigFilePath);

            else
            {
                await DataManager.UpdateJsonSectionAsync(unsavedData.UnityProjectsRootDirectory, nameof(unsavedData.UnityProjectsRootDirectory), unsavedData.ConfigFilePath);
                await DataManager.UpdateJsonSectionAsync(unsavedData.ProjectVersionDefaultFilePath, nameof(unsavedData.ProjectVersionDefaultFilePath), unsavedData.ConfigFilePath);
                await DataManager.UpdateJsonSectionAsync(unsavedData.UnityEditorsDirectory, nameof(unsavedData.UnityEditorsDirectory), unsavedData.ConfigFilePath);

                if (Path.Exists(Program.savedData?.ConfigFilePath))
                    File.Delete(Program.savedData.ConfigFilePath);

                string SettingsFile = Path.Combine(unsavedData.ConfigFilePath, "Settings.json");
                if (!Path.Exists(unsavedData.ConfigFilePath) || !File.Exists(SettingsFile))
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
                await DataManager.UpdateJsonSectionAsync(unsavedData.ConfigFilePath, nameof(unsavedData.ConfigFilePath), unsavedData.ConfigFilePath);
            }

        }


        return unsavedData;
    }
}
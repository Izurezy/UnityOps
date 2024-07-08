#region
using System.Runtime.InteropServices;
using Spectre.Console;
using UnityOps.Models;
using UnityOps.Utilities;
#endregion

namespace UnityOps
{
    public class Configurator
    {
        public static async Task ConfigureSettings()
        {
            SettingsModel unsavedData = new();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                unsavedData.unityEditorsRootDirectory = "/Applications/Unity/Hub/Editor/";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                unsavedData.unityEditorsRootDirectory = "C:/Program Files/Unity/Hub/Editor";

            unsavedData.unityProjectsRootDirectory = InputUtility.DirectoryValidationPrompt("Directory where all of your Unity projects are");
            unsavedData.unityEditorsRootDirectory = InputUtility.DirectoryValidationPrompt("Directory where all of your Unity Editors are", defaultValue: unsavedData.unityEditorsRootDirectory);
            unsavedData.projectVersionDefaultRelativeFilePath = InputUtility.DirectoryValidationPrompt("Default ProjectVersion Directory for all projects ", defaultValue: unsavedData.projectVersionDefaultRelativeFilePath);

            unsavedData.usePettyTables = AnsiConsole.Prompt(
            new TextPrompt<bool>("Use Petty Tables")
                .DefaultValueStyle("bold blue")
                .DefaultValue(unsavedData.usePettyTables)
            );

#region Display unsaved configuration
            AnsiConsole.Clear();
            AnsiConsole.WriteLine("\n");

            DisplayUtility.DisplayNewConfig(unsavedData);

            AnsiConsole.WriteLine("\n");
#endregion
            if (AnsiConsole.Confirm($"[{Program.SuccessColor}]Save?[/]"))
            {
                DataManager.ValidateConfigFile();

                AnsiConsole.MarkupLine($"[{Program.SuccessColor}][[Configurator]][/] Saving To File...!");

                if (await DataManager.IsDataPresentInConfigFileAsync())
                    await UpdateConfig(unsavedData);
                else
                    await DataManager.SaveAllToSettingsFileAsync(unsavedData);

                AnsiConsole.MarkupLine($"[{Program.SuccessColor}][[Configurator]][/] Saved To File");
            }
        }

        private static async Task UpdateConfig(SettingsModel unsavedData)
        {
            await DataManager.UpdateJsonSectionAsync(unsavedData.unityProjectsRootDirectory, nameof(unsavedData.unityProjectsRootDirectory));
            await DataManager.UpdateJsonSectionAsync(unsavedData.projectVersionDefaultRelativeFilePath, nameof(unsavedData.projectVersionDefaultRelativeFilePath));
            await DataManager.UpdateJsonSectionAsync(unsavedData.unityEditorsRootDirectory, nameof(unsavedData.unityEditorsRootDirectory));
            await DataManager.UpdateJsonSectionAsync(unsavedData.usePettyTables, nameof(unsavedData.usePettyTables));
        }
    }
}
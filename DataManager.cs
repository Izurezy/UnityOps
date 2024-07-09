#region
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using UnityOps.Models;
#endregion

namespace UnityOps;

public class DataManager
{

    public static SettingsModel savedData { get; private set; }

#if DEBUG
    private readonly static string configFilePath = Path.Combine(Environment.CurrentDirectory, "Settings.json");
#else
    private readonly static string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.json");
#endif

    public static async Task SaveAllToSettingsFileAsync(SettingsModel newData)
    {
        try
        {
            string json = JsonConvert.SerializeObject(newData, Formatting.Indented);
            await File.WriteAllTextAsync(configFilePath, json);
            savedData = newData;


            if (Program.isDebugging)
                AnsiConsole.MarkupLine($"[{Program.InfoColor}][[Data Manager]][/] Data saved to Settings.json");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Data Manager]][/] An error occurred while saving to Settings.json ");
            AnsiConsole.WriteException(ex);
        }
    }

    public static async Task LoadSettings()
    {
        try
        {
            string json = await File.ReadAllTextAsync(configFilePath);
            var data = JsonConvert.DeserializeObject<SettingsModel>(json);

            if (Program.isDebugging)
                AnsiConsole.MarkupLine($"[{Program.InfoColor}][[Data Manager]][/] loaded JSON data from Settings.json: {configFilePath}");

            savedData = data;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Data Manager]][/] An error occurred while loading Settings.json");
            AnsiConsole.WriteException(ex);
        }
    }

    public static async Task UpdateJsonSectionAsync<T>(T newData, string propertyName)
    {
        try
        {
            string savedDataFromFile = await File.ReadAllTextAsync(configFilePath);

            var jsonObj = JObject.Parse(savedDataFromFile);

            // Update the specified section
            jsonObj[propertyName] = JToken.FromObject(newData);

            string updatedJson = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            savedData = JsonConvert.DeserializeObject<SettingsModel>(updatedJson);
            await File.WriteAllTextAsync(configFilePath, updatedJson);

            if (Program.isDebugging)
                AnsiConsole.MarkupLine($"[{Program.InfoColor}][[Data Manager]][/] saved {propertyName}");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Data Manager]][/] An error occurred while saving to Settings.json");
            AnsiConsole.WriteException(ex);
        }
    }

    public static async Task UpdateUnityProject(UnityProject newProjectData)
    {
        try
        {

            await LoadSettings();
            var projectToUpdate = savedData.unityProjects.FirstOrDefault(project => project.directory == newProjectData.directory);


            if (projectToUpdate != null)
            {
                if (Program.isDebugging)
                    AnsiConsole.MarkupLine($"[{Program.InfoColor}][[Data Manager]][/] found {projectToUpdate.name} project, updating and saving to file");

                projectToUpdate.name = newProjectData.name;
                projectToUpdate.editorVersion = newProjectData.editorVersion;
                projectToUpdate.directory = newProjectData.directory;


                foreach (var newApplicationData in newProjectData.applicationsToOpenWithProject)
                {
                    var currentApplicationData = projectToUpdate.applicationsToOpenWithProject.FirstOrDefault(x => x.name == newApplicationData.name);

                    if (DoesApplicationExistByName(newApplicationData, projectToUpdate.applicationsToOpenWithProject))
                    {
                        UpdateApplicationsToOpenWithProjectAsync(currentApplicationData, newApplicationData);
                        continue;
                    }

                    if (!DoesApplicationExistByName(newApplicationData, projectToUpdate.applicationsToOpenWithProject))
                        AddApplicationsToOpenWithProject(projectToUpdate, newApplicationData);
                }
            }
            else
            {
                if (Program.isDebugging)
                    AnsiConsole.MarkupLine($"[{Program.InfoColor}][[Data Manager]][/] {newProjectData.name} project doesn't exist, saving to file");
                savedData.unityProjects.Add(newProjectData);
            }

            await SaveAllToSettingsFileAsync(savedData);
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static async Task UpdateUnityEditor(UnityEditor newEditorData)
    {
        var editorToUpdate = savedData.unityEditors.FirstOrDefault(editor => editor.version == newEditorData.version);
        try
        {
            await LoadSettings();

            editorToUpdate ??= savedData.unityEditors.FirstOrDefault(editor => editor.executableDirectory == newEditorData.executableDirectory);

            if (DoesUnityEditorExist(savedData.unityEditors, newEditorData))
            {

                if (editorToUpdate != null)
                {
                    editorToUpdate.version = newEditorData.version;
                    editorToUpdate.executableDirectory = newEditorData.executableDirectory;
                }
                if (Program.isDebugging)
                    AnsiConsole.MarkupLine($"[{Program.InfoColor}][[Data Manager]][/] Editor {newEditorData.version} found, updated and saving to file");
            }
            else
            {
                if (Program.isDebugging)
                    AnsiConsole.MarkupLine($"[{Program.InfoColor}][[Data Manager]][/] Editor {newEditorData.version} doesn't Exist, adding and saving to file");
                savedData.unityEditors.Add(newEditorData);
            }

            await SaveAllToSettingsFileAsync(savedData);
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

#region Applications to open with project
    private static void UpdateApplicationsToOpenWithProjectAsync(Application applicationToUpdate, Application newApplicationData)
    {
        if (applicationToUpdate == null || newApplicationData == null)
        {
            AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Data Manager]][/] Unable to update application");
            return;
        }
        AnsiConsole.MarkupLine($"[{Program.WarningColor}][[Data Manager]][/] Application({newApplicationData.name}) already exist, updating it");
        applicationToUpdate.name = newApplicationData.name;
        applicationToUpdate.executableFullPath = newApplicationData.executableFullPath;
        applicationToUpdate.arguments = newApplicationData.arguments;
    }

    private static void AddApplicationsToOpenWithProject(UnityProject project, Application newApplicationData)
    {
        if (project == null || newApplicationData == null)
        {
            AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Data Manager]][/] Unable to Add application");
            return;
        }
        project.applicationsToOpenWithProject.Add(newApplicationData);
        AnsiConsole.MarkupLine($"[{Program.SuccessColor}][[Data Manager]][/] Added Application, {newApplicationData.name} to open with {project.name}");
    }

    public static async Task RemoveApplicationsToOpenWithProjectAsync(string projectName, string applicationName)
    {
        try
        {
            await LoadSettings();

            var project = UnityProject.FindProjectByProjectName(projectName, savedData.unityProjects);
            var applicationToRemove = Application.FindApplicationByName(applicationName, project.applicationsToOpenWithProject);

            AnsiConsole.MarkupLine(project.applicationsToOpenWithProject.Remove(applicationToRemove)
                ? $"[{Program.SuccessColor}][[Data Manager]][/] Removed application {applicationToRemove.name}, from project {project.name}"
                : $"[{Program.InfoColor}][[Data Manager]][/] Application {applicationToRemove.name} not found");

            await SaveAllToSettingsFileAsync(savedData);
        }
        catch (Exception e)
        {
            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(applicationName))
                AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Data Manager]][/] Unable to remove application");
            else
                AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Data Manager]][/] Unable to remove application {applicationName} from project {projectName}");

            AnsiConsole.WriteException(e);

        }

    }
#endregion

#region Validation
    public static bool DoesApplicationExistByName(Application applicationName, List<Application> applications)
        => DoesItemExistByProperty(applicationName, applications, (x, y) => x.name == y.name);
    private static bool DoesUnityEditorExist(List<UnityEditor> unityEditors, UnityEditor unityEditor)
        => DoesItemExistByProperty(unityEditor, unityEditors, (x, y) => x.version == y.version) || DoesItemExistByProperty(unityEditor, unityEditors, (x, y) => x.executableDirectory == y.executableDirectory);

    private static bool DoesItemExistByProperty<T>(T item, List<T> items, Func<T, T, bool> predicate) => items.Any(x => predicate(x, item));

    public static bool DoesUnityProjectAndEditorExist()
    {
        if (savedData.unityProjects.Count > 0 && savedData.unityEditors.Count > 0)
            return true;

        AnsiConsole.MarkupLine($"[{Program.WarningColor}][[Main Menu]][/] No Unity Projects or Editors Found");
        return false;
    }

    public static void ValidateConfigFile()
    {

        if (Program.isDebugging)
            AnsiConsole.MarkupLine($"[{Program.InfoColor}][[Data Manager]][/] Validating Config file");

        if (!DoesConfigFileExist())
            CreateConfigFile();
        else if (Program.isDebugging)
            AnsiConsole.MarkupLine($"[{Program.InfoColor}][[Data Manager]][/] Settings.json already exist");
    }

    public static async Task<bool> IsDataPresentInConfigFileAsync()
    {
        try
        {
            await LoadSettings();
            return savedData != null;
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Data Manager]][/] Unable to read from Settings.json");
            AnsiConsole.WriteException(e);
        }

        return false;
    }

    private static bool DoesConfigFileExist() => File.Exists(configFilePath);

    private static void CreateConfigFile()
    {

        try
        {
            using (File.Create(configFilePath))
            { }
            AnsiConsole.MarkupLine($"[{Program.WarningColor}][[Data Manager]][/] Settings.json doesn't exist, Created Settings.json");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Data Manager]][/] Unable to create Settings.json");
            AnsiConsole.WriteException(ex);
            Environment.Exit(1);
        }
    }
#endregion
}
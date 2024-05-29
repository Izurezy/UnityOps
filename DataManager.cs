using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using UnityOps.Structs;

namespace UnityOps;

public class DataManager
{
    public static async Task SaveToJsonFileAsync(SettingsModel data, string saveFilePath)
    {
        try
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            await File.WriteAllTextAsync(saveFilePath, json);

            if (Program.isDebugging)
                AnsiConsole.WriteLine("Data saved to JSON file " + json);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("An error occurred while saving JSON file");
            AnsiConsole.WriteException(ex);
        }
    }

    public static async Task UpdateJsonSectionAsync<T>(T newData, string propertyName, string saveFilePath)
    {
        try
        {
            // Read the existing JSON file
            string existingJson = await File.ReadAllTextAsync(saveFilePath);
            var jsonObj = JObject.Parse(existingJson);

            // Update the specified section
            jsonObj[propertyName] = JToken.FromObject(newData);

            // Serialize the updated JSON object back to the file
            string updatedJson = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            await File.WriteAllTextAsync(saveFilePath, updatedJson);

            if (Program.isDebugging)
                AnsiConsole.WriteLine("Data saved to JSON file: " + propertyName);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("An error occurred while saving to JSON file");
            AnsiConsole.WriteException(ex);
        }
    }

    public static async Task<T> LoadFromJsonFile<T>(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                string json = await File.ReadAllTextAsync(filePath);
                T data = JsonConvert.DeserializeObject<T>(json);


                if (Program.isDebugging)
                {
                    AnsiConsole.WriteLine($"\nRaw json\n{json}\n\n");
                    AnsiConsole.MarkupLine("[green]Successfully loaded JSON data from the file:[/] " + filePath);
                }
                return data;
            }
            else
                AnsiConsole.MarkupLine($"[red]{filePath} doesn't exists, running `UnityOps -config` [/]");

            return default;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteLine("An error occurred while loading JSON file");
            AnsiConsole.WriteException(ex);
            return default; // Return default value for type T (e.g., null for reference types)
        }
    }
}

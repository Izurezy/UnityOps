#region
using System.Diagnostics;
using Spectre.Console;
#endregion

namespace UnityOps.Models
{
    public class Application(string name, string executableFullPath, string arguments = default)
    {
        public string name = name;
        public string executableFullPath = executableFullPath;
        public string arguments = arguments;

        public static Application FindApplicationByName(string name, List<Application> applications)
        {
            if (applications != null)
                return applications.FirstOrDefault(application => application.name == name);

            AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Application Model]][/] Applications list is null!! have you added any applications?");
            return default;
        }

        public static void Open(Application applicationData)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.FileName = applicationData.executableFullPath;
                    process.StartInfo.Arguments = applicationData.arguments;
                    process.Start();
                }
                AnsiConsole.MarkupLine($"[{Program.SuccessColor}][[Application Utility]][/] {applicationData.name} Started");
            }
            catch (Exception e)
            {
                AnsiConsole.MarkupLine(applicationData != null
                    ? $"[{Program.ErrorColor}][[Application]][/] Unable to Start Application {applicationData.name}"
                    : $"[{Program.ErrorColor}][[Application]][/] Unable to Start Application");
                AnsiConsole.WriteException(e);
            }
        }



        public static async Task Create(List<Application> applications)
        {
            AnsiConsole.MarkupLine($"\n[{Program.InfoColor2}]Add Application[/]\n");

            var applicationName = AnsiConsole.Ask<string>($"Application Name:");

            string applicationFullExecutablePath = AnsiConsole.Prompt(
            new TextPrompt<string>("Application Full Executable Path:")
                .DefaultValueStyle("bold blue")
                .Validate(applicationFullExecutablePath =>
                {
                    if (Path.GetExtension(applicationFullExecutablePath).Equals(".lnk", StringComparison.OrdinalIgnoreCase))
                        return ValidationResult.Error($"[{Program.ErrorColor}].Lnk extension isn't supported[/]");

                    return File.Exists(applicationFullExecutablePath) ? ValidationResult.Success() : ValidationResult.Error($"[{Program.ErrorColor}]File must exist[/]");
                }));

            string applicationArguments = AnsiConsole.Prompt(
            new TextPrompt<string>($"[{Program.InfoColor2}][[Optional]][/] Application Arguments:")
                .AllowEmpty());

            Application newApplication = new(applicationName, applicationFullExecutablePath, applicationArguments);

            if (DataManager.DoesApplicationExistByName(newApplication, applications))
                if (!AnsiConsole.Confirm("Application Already exist with that name, do you still create this application?", false))
                {
                    AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Application Utility]][/] Application wasn't added");
                    return;
                }

            applications.Add(newApplication);
            await DataManager.UpdateJsonSectionAsync(applications, "applications");

            AnsiConsole.MarkupLine($"[{Program.SuccessColor}][[Application Utility]][/] Added {newApplication.name}");
        }
    }
}
#region
using Spectre.Console;
using UnityOps.Models;
#endregion

namespace UnityOps.Utilities
{
    public class DisplayUtility
    {

        private static void DisplayFoundProjects(List<UnityProject> unityProjects, string tableTitle = $"[blue]Unity Projects Found[/]")
        {
            if (!DataManager.savedData.usePettyTables)
            {
                AnsiConsole.MarkupLine($"[{Program.InfoColor2}]Unity Projects Found[/]");
                AnsiConsole.MarkupLine("Name | Directory | Project Editor Version \n");

                foreach (var project in unityProjects)
                    AnsiConsole.MarkupLine($"{project.name} | {project.directory} | {project.editorVersion}");

                return;
            }

            var projectsTable = new Table();
            projectsTable.Title(tableTitle);
            projectsTable.AddColumn("Project Name");
            projectsTable.AddColumn("Project Directory");
            projectsTable.AddColumn("Project Editor Version");
            projectsTable.Border(TableBorder.Rounded);
            projectsTable.SafeBorder();

            foreach (var project in unityProjects)
                projectsTable.AddRow(project.name, project.directory, project.editorVersion);

            AnsiConsole.Write(projectsTable);
        }

        private static void DisplayFoundEditors(List<UnityEditor> unityEditors, List<UnityProject> unityProjects, string tableTitle = "[blue]Unity Editors Found[/]")
        {
            if (!DataManager.savedData.usePettyTables)
            {
                AnsiConsole.MarkupLine($"[{Program.InfoColor2}]Unity Editors Found[/]");
                foreach (var editor in unityEditors)
                {
                    int ProjectsMadeWithEditor = unityProjects.Count(project => project.editorVersion == editor.version);
                    AnsiConsole.MarkupLine("Executable  | Editor Version | Projects \n");
                    AnsiConsole.MarkupLine($"{editor.executableDirectory} | {editor.version} | {ProjectsMadeWithEditor.ToString()}");
                }
                return;
            }

            var editorsTable = new Table();
            editorsTable.Title(tableTitle);
            editorsTable.AddColumn("Executable");
            editorsTable.AddColumn("Version");
            editorsTable.AddColumn("Projects");

            editorsTable.Border(TableBorder.Rounded);
            editorsTable.SafeBorder();

            foreach (var editor in unityEditors)
            {
                int ProjectsMadeWithEditor = unityProjects.Count(project => project.editorVersion == editor.version);
                editorsTable.AddRow(editor.executableDirectory, editor.version, ProjectsMadeWithEditor.ToString());
            }

            AnsiConsole.Write(editorsTable);
        }

        public static void DisplayFoundEditorsAndProject(List<UnityEditor> unityEditors, List<UnityProject> unityProjects)
        {

            DisplayFoundProjects(unityProjects);
            AnsiConsole.Write("\n");

            DisplayFoundEditors(unityEditors, unityProjects);
            AnsiConsole.MarkupLine($"[{Program.SuccessColor}]Done![/]");

            AnsiConsole.Write("\n");

            AnsiConsole.MarkupLine($"[{Program.InfoColor2}]{unityProjects.Count} Projects where found[/]");
            AnsiConsole.MarkupLine($"[{Program.InfoColor2}]{unityEditors.Count} Editors where found[/]");

        }

        public static void DisplayApplications(List<Application> applications)
        {
            if (!DataManager.savedData.usePettyTables)
            {
                AnsiConsole.MarkupLine($"[{Program.InfoColor2}]Applications[/]");
                foreach (var application in applications)
                {
                    AnsiConsole.MarkupLine("Name  | Executable Full Path | Arguments");
                    AnsiConsole.MarkupLine($"{application.name} | {application.executableFullPath} | {(string.IsNullOrEmpty(application.arguments) ? "" : application.arguments)}");
                }
                return;
            }

            var applicationTable = new Table();
            applicationTable.Title($"[{Program.InfoColor2}]Applications[/]");
            applicationTable.AddColumn("Name");
            applicationTable.AddColumn("Executable Path");

            applicationTable.Border(TableBorder.Rounded);
            applicationTable.SafeBorder();

            foreach (var application in applications)
                applicationTable.AddRow(application.name, application.executableFullPath);

            AnsiConsole.Write(applicationTable);
        }

        public static void DisplayNewConfig(SettingsModel data)
        {
            if (!data.usePettyTables)
            {

                AnsiConsole.MarkupLine($"Unity Projects Root Directory | {data.unityProjectsRootDirectory}");
                AnsiConsole.MarkupLine($"ProjectVersion Default File Path | {data.projectVersionDefaultRelativeFilePath}");
                AnsiConsole.MarkupLine($"Unity Editors Directory | {data.unityEditorsRootDirectory}");
                AnsiConsole.MarkupLine($"Use Petty Tables | {data.usePettyTables}");
                return;
            }

            var table = new Table();
            table.AddColumn("Option");
            table.AddColumn("Path");

            table.Border(TableBorder.Rounded);
            table.SafeBorder();

            table.AddRow("Unity Projects Root Directory", data.unityProjectsRootDirectory);
            table.AddRow("ProjectVersion Default File Path", data.projectVersionDefaultRelativeFilePath);
            table.AddRow("Unity Editors Directory", data.unityEditorsRootDirectory);
            table.AddRow("Use Petty Tables", data.usePettyTables.ToString());

            AnsiConsole.Write(table);
        }


        public static string DisplaySelectionPrompt(string title, string[] choices, string moreChoicesText = "[blue](Move up and down to reveal more)[/]", int pageSize = 10)
        {
            return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"{title}")
                .PageSize(pageSize)
                .MoreChoicesText(moreChoicesText)
                .AddChoices(choices));
        }

        public static List<string> DisplayMultiSelectionPrompt(string title, string[] choices, string moreChoicesText = "[blue](Move up and down to reveal more)[/]", int pageSize = 10)
        {
            return AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title($"{title}")
                .PageSize(pageSize)
                .MoreChoicesText(moreChoicesText)
                .AddChoices(choices));
        }
    }
}
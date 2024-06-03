using Spectre.Console;
using UnityOps.Structs;

namespace UnityOps.Utilities
{
    public class DisplayUtility
    {
        public static void DisplayFoundProjectsInTable(List<UnityProject> unityProjects)
        {
            var projectsTable = new Table();
            projectsTable.Title("[blue]Unity Projects Found[/]");
            projectsTable.AddColumn("Project Name");
            projectsTable.AddColumn("Project Directory");
            projectsTable.AddColumn("Project Editor Version");
            projectsTable.Border(TableBorder.Rounded);
            projectsTable.SafeBorder();

            foreach (var project in unityProjects)
                projectsTable.AddRow(project.projectName, project.unityProjectDirectory, project.projectEditorVersion);

            AnsiConsole.Write(projectsTable);
        }
        public static void DisplayFoundEditorInTable(List<UnityEditor> unityEditors, List<UnityProject> unityProjects)
        {
            var editorsTable = new Table();
            editorsTable.Title("[blue]Unity Editors Found[/]");
            editorsTable.AddColumn("Executable");
            editorsTable.AddColumn("Version");
            editorsTable.AddColumn("Projects");

            editorsTable.Border(TableBorder.Rounded);
            editorsTable.SafeBorder();

            foreach (var editor in unityEditors)
            {
                //improve this somehow???
                int ProjectsMadeWithEditor = UnityEditorUtility.GetProjectsMadeWithEditor(unityProjects, editor);
                editorsTable.AddRow(editor.editorExecutableDirectory, editor.editorVersion, ProjectsMadeWithEditor.ToString());
            }

            AnsiConsole.Write(editorsTable);
        }
        public static void DisplayEditorsAndProjectInTable(List<UnityEditor> unityEditors, List<UnityProject> unityProjects)
        {
            // display Projects table
            DisplayFoundProjectsInTable(unityProjects);
            AnsiConsole.MarkupLine("\n");

            // display Editors table 
            DisplayFoundEditorInTable(unityEditors, unityProjects);
            AnsiConsole.MarkupLine($"[green]Done![/]");

            AnsiConsole.WriteLine("\n");
            AnsiConsole.MarkupLine($"[blue]{unityProjects.Count} Projects where found[/]");
            AnsiConsole.MarkupLine($"[blue]{unityEditors.Count} Editors where found[/]");
            AnsiConsole.WriteLine("\n");
        }

    }
}
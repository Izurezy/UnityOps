using Spectre.Console;

namespace UnityOps.Structs
{
    public struct UnityProject(string projectName, string unityProjectDirectory, string projectEditorVersion)
    {
        public string projectName = projectName;
        public string unityProjectDirectory = unityProjectDirectory;
        public string projectEditorVersion = projectEditorVersion;

        public static UnityProject FindProjectByProjectName(string projectName, List<UnityProject> unityProjects)
        {
            if (unityProjects == null)
            {
                AnsiConsole.MarkupLine("[red] Unity Projects list is null[/]\n [yellow]Check your config and try running UnityOps -f[/]");
                return default;
            }

            return unityProjects.FirstOrDefault(project => project.projectName == projectName);

        }
    }
}
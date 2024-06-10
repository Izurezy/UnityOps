using Spectre.Console;

namespace UnityOps.Structs
{
    public struct UnityProject(string name, string projectDirectory, string projectEditorVersion)
    {
        public string name = name;
        public string projectDirectory = projectDirectory;
        public string projectEditorVersion = projectEditorVersion;

        public static UnityProject FindProjectByProjectName(string projectName, List<UnityProject> unityProjects)
        {
            if (unityProjects == null)
            {
                AnsiConsole.MarkupLine("[red] Unity Projects list is null[/]\n [yellow]Check your config and try running UnityOps -f[/]");
                return default;
            }

            return unityProjects.FirstOrDefault(project => project.name == projectName);

        }
    }
}
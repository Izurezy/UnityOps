using Spectre.Console;

namespace UnityOps.Structs
{
    public struct UnityEditor(string executableDirectory, string version)
    {
        public string executableDirectory = executableDirectory;
        public string version = version;

        public static UnityEditor FindEditorByProjectVersion(string projectEditorVersion, List<UnityEditor> editors)
        {
            if (editors == null)
            {
                AnsiConsole.MarkupLine("[red] Unity Editor list is null[/]\n [yellow]Check your config and try running UnityOps -f[/]");
                return default;
            }

            return editors.FirstOrDefault(editor => editor.version == projectEditorVersion);
        }
    }
}
using Spectre.Console;

namespace UnityOps.Structs
{
    public struct UnityEditor(string editorExecutableDirectory, string engineVersion)
    {
        public string editorExecutableDirectory = editorExecutableDirectory;
        public string editorVersion = engineVersion;

        public static UnityEditor FindEditorByProjectVersion(string projectEditorVersion, List<UnityEditor> unityEditors)
        {
            if (unityEditors == null)
            {
                AnsiConsole.MarkupLine("[red] Unity Editor list is null[/]\n [yellow]Check your config and try running UnityOps -f[/]");
                return default;
            }

            return unityEditors.FirstOrDefault(editor => editor.editorVersion == projectEditorVersion);
        }
    }
}
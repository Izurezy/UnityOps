#region
using Spectre.Console;
#endregion

namespace UnityOps.Models
{
    public class UnityEditor(string executableDirectory, string version)
    {
        public string executableDirectory = executableDirectory;
        public string version = version;

        public static UnityEditor FindEditorByVersion(string projectEditorVersion, List<UnityEditor> editors)
        {
            if (editors != null)
                return editors.FirstOrDefault(editor => editor.version == projectEditorVersion);

            AnsiConsole.MarkupLine($"[{Program.ErrorColor}][[Unity Editor]][/] Unity Editor list is null, Check your config and try running UnityOps -f");
            return default;
        }
    }
}
namespace UnityOps.Structs
{
    public struct UnityEditor(string editorExecutableDirectory, string engineVersion, List<UnityProject> projects)
    {
        public string editorExecutableDirectory = editorExecutableDirectory;
        public string editorVersion = engineVersion;
        public List<UnityProject> projects = projects;

        public static UnityEditor FindEditorByProjectName(string projectName, List<UnityEditor> unityEditors)
        {
            return unityEditors
                .FirstOrDefault(editor => editor.projects.Any(project => project.projectName == projectName));
        }
    }
}
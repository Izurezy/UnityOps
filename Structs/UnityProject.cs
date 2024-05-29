namespace UnityOps.Structs
{
    public struct UnityProject(string projectName, string unityProjectDirectory, string projectEditorVersion)
    {
        public string projectName = projectName;
        public string unityProjectDirectory = unityProjectDirectory;
        public string projectEditorVersion = projectEditorVersion;

        public static UnityProject FindProjectByProjectName(string projectName, List<UnityProject> unityProjects)
        {
            return unityProjects
                .FirstOrDefault(project => project.projectName == projectName);
        }
    }
}
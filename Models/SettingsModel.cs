namespace UnityOps.Models
{
    public class SettingsModel
    {
        public string unityProjectsRootDirectory = "";
        public string projectVersionDefaultRelativeFilePath = $"ProjectSettings{Path.DirectorySeparatorChar}ProjectVersion.txt";
        public string unityEditorsRootDirectory = string.Empty;
        public string lastProjectOpenedName = string.Empty;
        public bool shouldOpenRecentProject = false;
        public bool usePettyTables = true;

        public List<UnityProject> unityProjects = new();
        public List<UnityEditor> unityEditors = [];
        public List<Application> applications = [];

    }
}
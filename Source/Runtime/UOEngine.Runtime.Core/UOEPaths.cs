namespace UOEngine.Runtime.Core;

public static class UOEPaths
{
    public static readonly string ExeDir;

    public static readonly string ProjectDir;

    public static readonly string SourceDir;

    public static readonly string ShadersDir;

    public static readonly string PluginDir;

    static UOEPaths()
    {
        ExeDir = AppContext.BaseDirectory;

        // Project root possible application root when packaged?
        ProjectDir = ExeDir;

        var dir = new DirectoryInfo(ExeDir);

        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "UOEngine.slnx")))
            {
                ProjectDir = dir.FullName;

                break;
            }

            dir = dir.Parent;
        }

        SourceDir = Path.Combine(ProjectDir, "Source");
        ShadersDir = Path.Combine(SourceDir, "Shaders");

        PluginDir = Path.Combine(ExeDir, "Plugins");
    }
}

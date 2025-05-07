namespace ClaudeCodeCurator.Common;

/// <summary>
/// AppSettings-Configuration for the app
/// </summary>
public class AppSettings
{
    public string? ConnectionString { get; set; }
    
    // Application version from csproj file (major.minor.bugfix)
    public static string Version
    {
        get
        {
            var version = typeof(AppSettings).Assembly.GetName().Version;
            return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "0.0.0";
        }
    }
}
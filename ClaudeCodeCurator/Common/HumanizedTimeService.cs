namespace ClaudeCodeCurator.Common;

/// <summary>
/// Service for formatting DateTime values into human-readable relative time strings.
/// </summary>
public class HumanizedTimeService
{
    /// <summary>
    /// Converts a DateTime to a human-readable relative time string (e.g., "2 minutes ago").
    /// </summary>
    /// <param name="dateTime">The DateTime to format (should be in UTC).</param>
    /// <returns>A human-readable string representing the relative time.</returns>
    public string GetRelativeTimeText(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;
        
        if (timeSpan.TotalMinutes < 1)
            return "just now";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes == 1 ? "" : "s")} ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours == 1 ? "" : "s")} ago";
        if (timeSpan.TotalDays < 30)
            return $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays == 1 ? "" : "s")} ago";
        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) == 1 ? "" : "s")} ago";
            
        return $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) == 1 ? "" : "s")} ago";
    }
    
    /// <summary>
    /// Gets the time elapsed in seconds since the given DateTime.
    /// </summary>
    /// <param name="dateTime">The DateTime to measure from (should be in UTC).</param>
    /// <returns>A string representing the seconds elapsed (e.g., "42s").</returns>
    public string GetSecondsElapsed(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
            return string.Empty;
            
        var timeSpan = DateTime.UtcNow - dateTime.Value;
        return $"{(int)timeSpan.TotalSeconds}s";
    }
    
    /// <summary>
    /// Gets the time elapsed in minutes since the given DateTime.
    /// </summary>
    /// <param name="dateTime">The DateTime to measure from (should be in UTC).</param>
    /// <returns>A string representing the minutes elapsed (e.g., "5m").</returns>
    public string GetMinutesElapsed(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
            return string.Empty;
            
        var timeSpan = DateTime.UtcNow - dateTime.Value;
        return $"{(int)timeSpan.TotalMinutes}m";
    }
}
namespace WinformsVibes.Models;

public class ApplicationInfo
{
    public virtual int Id { get; set; }
    public virtual string ApplicationName { get; set; } = string.Empty;
    public virtual string Author { get; set; } = string.Empty;
    public virtual string Version { get; set; } = string.Empty;
    public virtual string? Description { get; set; }
    public virtual string? Framework { get; set; }
    public virtual string? Dependencies { get; set; }
    public virtual DateTime CreatedAt { get; set; }
    public virtual DateTime UpdatedAt { get; set; }

    // Not mapped — set at runtime for display purposes
    public string DatabaseName { get; set; } = string.Empty;
}

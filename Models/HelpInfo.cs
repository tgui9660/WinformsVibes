namespace WinformsVibes.Models;

public class HelpInfo
{
    public virtual int Id { get; set; }
    public virtual string Category { get; set; } = string.Empty;
    public virtual string Topic { get; set; } = string.Empty;
    public virtual string Content { get; set; } = string.Empty;
}

namespace FlowBridge.Web.Models;

public sealed class ServiceOption
{
    public int Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string Practice { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string BulletPoints { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public bool IsPublished { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTimeOffset UpdatedUtc { get; set; } = DateTimeOffset.UtcNow;
}

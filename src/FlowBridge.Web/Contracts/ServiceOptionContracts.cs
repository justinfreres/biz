using FlowBridge.Web.Models;

namespace FlowBridge.Web.Contracts;

public sealed record PublicServiceOption(
    string Practice,
    string Title,
    string Summary,
    IReadOnlyList<string> BulletPoints,
    string Audience,
    int SortOrder);

public sealed record AdminServiceOption(
    int Id,
    string Practice,
    string Title,
    string Summary,
    string BulletPoints,
    string Audience,
    bool IsPublished,
    int SortOrder,
    DateTimeOffset UpdatedUtc);

public sealed record ServiceOptionInput(
    string? Practice,
    string? Title,
    string? Summary,
    string? BulletPoints,
    string? Audience,
    bool IsPublished,
    int SortOrder);

public sealed record AdminLoginRequest(string? Password);

public sealed record AntiforgeryTokenResponse(string? Token);

public sealed record AdminSessionResponse(string Name);

public static class ServiceOptionMapper
{
    public static PublicServiceOption ToPublicDto(ServiceOption service) => new(
        service.Practice,
        service.Title,
        service.Summary,
        SplitBulletPoints(service.BulletPoints),
        service.Audience,
        service.SortOrder);

    public static AdminServiceOption ToAdminDto(ServiceOption service) => new(
        service.Id,
        service.Practice,
        service.Title,
        service.Summary,
        service.BulletPoints,
        service.Audience,
        service.IsPublished,
        service.SortOrder,
        service.UpdatedUtc);

    public static Dictionary<string, string[]> Validate(ServiceOptionInput input)
    {
        var errors = new Dictionary<string, string[]>();
        AddRequired(errors, nameof(input.Practice), input.Practice, 80);
        AddRequired(errors, nameof(input.Title), input.Title, 120);
        AddRequired(errors, nameof(input.Summary), input.Summary, 600);
        AddRequired(errors, nameof(input.Audience), input.Audience, 300);

        var bullets = SplitBulletPoints(input.BulletPoints);
        if (bullets.Count == 0)
        {
            errors[nameof(input.BulletPoints)] = ["Add at least one bullet point."];
        }
        else if (bullets.Count > 8 || bullets.Any(bullet => bullet.Length > 220))
        {
            errors[nameof(input.BulletPoints)] = ["Use up to eight bullet points of 220 characters or fewer each."];
        }

        if (input.SortOrder is < 0 or > 999)
        {
            errors[nameof(input.SortOrder)] = ["Sort order must be between 0 and 999."];
        }

        return errors;
    }

    public static void ApplyInput(ServiceOption service, ServiceOptionInput input)
    {
        service.Practice = input.Practice!.Trim();
        service.Title = input.Title!.Trim();
        service.Summary = input.Summary!.Trim();
        service.BulletPoints = string.Join('\n', SplitBulletPoints(input.BulletPoints));
        service.Audience = input.Audience!.Trim();
        service.IsPublished = input.IsPublished;
        service.SortOrder = input.SortOrder;
        service.UpdatedUtc = DateTimeOffset.UtcNow;
    }

    public static string CreateSlug(string title)
    {
        var characters = title.ToLowerInvariant().Select(character => char.IsLetterOrDigit(character) ? character : '-').ToArray();
        var slug = string.Join(string.Empty, characters).Trim('-');
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(slug) ? "service" : slug[..Math.Min(slug.Length, 80)];
    }

    private static List<string> SplitBulletPoints(string? bulletPoints) =>
        (bulletPoints ?? string.Empty)
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();

    private static void AddRequired(Dictionary<string, string[]> errors, string field, string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[field] = ["This field is required."];
        }
        else if (value.Trim().Length > maximumLength)
        {
            errors[field] = [$"Use {maximumLength} characters or fewer."];
        }
    }
}

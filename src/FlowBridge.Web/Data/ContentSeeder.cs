using FlowBridge.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBridge.Web.Data;

public static class ContentSeeder
{
    private static readonly SeedServiceOption[] DefaultServices =
    [
        new(
            "PRACTICE 01",
            "Flow & automation",
            "K2 and Nintex lifecycle support for organizations carrying legacy workflows forward without losing process knowledge.",
            "Install, configure, upgrade, and troubleshoot\nMigration assessment and cloud transition roadmap\nWorkflow repair, integration support, and documentation\nRelease readiness, security review, and knowledge transfer",
            "Teams with an aging K2/Nintex estate, stalled upgrade, or thin internal coverage.",
            10),
        new(
            "PRACTICE 02",
            "ERP & operations",
            "Odoo development and production support that turns a fragile custom instance into a dependable operating system.",
            "Custom modules, integrations, reporting, and automation\nUpgrade readiness, regression repair, and release planning\nPerformance, security, data, and production support\nFractional technical ownership for internal teams",
            "Growing organizations that rely on Odoo but need senior technical continuity.",
            20),
        new(
            "PRACTICE 03",
            "Network & infrastructure",
            "Practical networking and infrastructure support that makes the platform beneath your applications reliable, visible, and easier to operate.",
            "Network design, troubleshooting, and lifecycle planning\nServer, identity, endpoint, and connectivity integration\nMonitoring, change control, and operational documentation\nInfrastructure readiness for migrations and releases",
            "Teams that need a software-minded infrastructure partner for a fragile or changing environment.",
            30),
        new(
            "PRACTICE 04",
            "Business Central extensions",
            "Microsoft Dynamics 365 Business Central AL extension development and support for integrations, tailored processes, and safer releases.",
            "AL extension design, development, troubleshooting, and maintenance\nIntegration, data exchange, reporting, and workflow enablement\nUpgrade readiness, regression repair, and release planning\nDocumentation and handoff for internal or partner teams",
            "Business Central teams that need dependable customization without losing upgrade discipline.",
            40),
        new(
            "PRACTICE 05",
            "Cybersecurity & resilience",
            "Cybersecurity master’s experience paired with production operations: practical risk reduction that supports delivery instead of slowing it down.",
            "Security posture reviews for applications, integrations, and operations\nAccess, logging, backup, and recovery improvement planning\nSecure release practices, dependency awareness, and change control\nClear risk documentation for owners and technical teams",
            "Organizations that want an actionable security baseline connected to real operations.",
            50),
        new(
            "PRACTICE 06",
            "CompTIA A+ education",
            "Educator-led CompTIA A+ continuing education that strengthens the practical support, troubleshooting, networking, endpoint, and security skills IT teams use every day.",
            "Guided learning plans for foundational IT support topics\nHands-on troubleshooting, endpoint, networking, and security practice\nContinuing education and skills-refresh sessions for support teams\nMentoring for learners building confident help desk and field support habits",
            "Aspiring IT professionals, support teams, and learners building stronger technical support fundamentals.",
            60)
    ];

    public static async Task SeedAsync(ContentDbContext database, CancellationToken cancellationToken = default)
    {
        var existingSlugs = new HashSet<string>(
            await database.ServiceOptions
                .Select(service => service.Slug)
                .ToListAsync(cancellationToken),
            StringComparer.OrdinalIgnoreCase);

        foreach (var service in DefaultServices)
        {
            var slug = FlowBridge.Web.Contracts.ServiceOptionMapper.CreateSlug(service.Title);
            if (existingSlugs.Contains(slug))
            {
                continue;
            }

            database.ServiceOptions.Add(new ServiceOption
            {
                Slug = slug,
                Practice = service.Practice,
                Title = service.Title,
                Summary = service.Summary,
                BulletPoints = service.BulletPoints,
                Audience = service.Audience,
                IsPublished = true,
                SortOrder = service.SortOrder,
                UpdatedUtc = DateTimeOffset.UtcNow
            });
        }

        await database.SaveChangesAsync(cancellationToken);
    }

    private sealed record SeedServiceOption(
        string Practice,
        string Title,
        string Summary,
        string BulletPoints,
        string Audience,
        int SortOrder);
}


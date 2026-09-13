using Microsoft.EntityFrameworkCore;

namespace FlowBridge.Web.Tests;

public sealed class StaticSiteTests
{
    [Fact]
    public void Homepage_contains_the_business_brand_launch_dashboard_and_new_practices()
    {
        var homepage = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "src", "FlowBridge.Web", "wwwroot", "index.html"));

        Assert.Contains("FlowBridge Systems", homepage, StringComparison.Ordinal);
        Assert.Contains("30-DAY LAUNCH SYSTEM", homepage, StringComparison.Ordinal);
        Assert.Contains("Network &amp; infrastructure", homepage, StringComparison.Ordinal);
        Assert.Contains("Microsoft Dynamics 365 Business Central", homepage, StringComparison.Ordinal);
        Assert.Contains("Nintex Automation K2", homepage, StringComparison.Ordinal);
        Assert.Contains("Microsoft 365 &amp; Power Platform", homepage, StringComparison.Ordinal);
        Assert.Contains("Odoo Community and Enterprise", homepage, StringComparison.Ordinal);
        Assert.Contains("PAID CONSULTATION", homepage, StringComparison.Ordinal);
        Assert.Contains("$50 initial booking fee", homepage, StringComparison.Ordinal);
        Assert.Contains("data-consultation-form", homepage, StringComparison.Ordinal);
        Assert.Contains("FLOWBRIDGE DIGITAL PRODUCTS", homepage, StringComparison.Ordinal);
        Assert.Contains("FlowBridge Funny Tech Coloring Book: Cosmic Desk Mayhem", homepage, StringComparison.Ordinal);
        Assert.Contains("$5.99", homepage, StringComparison.Ordinal);
        Assert.Contains("data-product-order-form", homepage, StringComparison.Ordinal);
        var siteScript = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "src", "FlowBridge.Web", "wwwroot", "app.js"));
        Assert.Contains("info@flowbridge-systems-llc.odoo.com", siteScript, StringComparison.Ordinal);
        Assert.Contains("Manual fulfillment: send payment instructions", siteScript, StringComparison.Ordinal);
        Assert.Contains("Cybersecurity &amp; resilience", homepage, StringComparison.Ordinal);
        Assert.Contains("PROFESSIONAL SKILLS", homepage, StringComparison.Ordinal);
        Assert.Contains("Data recovery, backup-and-recovery systems", homepage, StringComparison.Ordinal);
        Assert.Contains("IEEE &amp; NASA graduate research", homepage, StringComparison.Ordinal);
        Assert.Contains("Artificial Intelligence for Defect Examination", homepage, StringComparison.Ordinal);
    }

    [Fact]
    public void Production_project_targets_net_10()
    {
        var project = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "src", "FlowBridge.Web", "FlowBridge.Web.csproj"));

        Assert.Contains("<TargetFramework>net10.0</TargetFramework>", project, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Local_content_database_seeds_all_seven_service_options()
    {
        var databasePath = Path.Combine(GetRepositoryRoot(), "artifacts", $"content-test-{Guid.NewGuid():N}.db");
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

        try
        {
            var options = new DbContextOptionsBuilder<FlowBridge.Web.Data.ContentDbContext>()
                .UseSqlite($"Data Source={databasePath};Pooling=False")
                .Options;
            await using (var database = new FlowBridge.Web.Data.ContentDbContext(options))
            {
                await database.Database.EnsureCreatedAsync();
                await FlowBridge.Web.Data.ContentSeeder.SeedAsync(database);

                var serviceTitles = await database.ServiceOptions
                    .OrderBy(service => service.SortOrder)
                    .Select(service => service.Title)
                    .ToListAsync();

                Assert.Equal(7, serviceTitles.Count);
                Assert.Contains("Network & infrastructure", serviceTitles);
                Assert.Contains("Business Central extensions", serviceTitles);
                Assert.Contains("Cybersecurity & resilience", serviceTitles);
                Assert.Contains("CompTIA A+ education", serviceTitles);
                Assert.Contains("Access & Office automation", serviceTitles);
            }
        }
        finally
        {
            foreach (var suffix in new[] { string.Empty, "-shm", "-wal" })
            {
                var file = new FileInfo(databasePath + suffix);
                if (file.Exists)
                {
                    file.Delete();
                }
            }
        }
    }

    private static string GetRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "FlowBridge.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the FlowBridge repository root.");
    }
}

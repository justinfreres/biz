[CmdletBinding()]
param(
    [string]$SiteDirectory = "src\FlowBridge.Web\wwwroot"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$siteRoot = Join-Path $repositoryRoot $SiteDirectory
$requiredFiles = @("index.html", "style.css", "app.js", "favicon.svg", "favicon.ico", "assets\flowbridge-cosmos-mark.png", "admin\index.html", "admin\login.html", "admin\admin.js", "admin\login.js", "admin\admin.css")

foreach ($file in $requiredFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $siteRoot $file))) {
        throw "Missing required site asset: $file"
    }
}

$homepage = Get-Content -LiteralPath (Join-Path $siteRoot "index.html") -Raw
if ($homepage -notmatch "FlowBridge Systems") {
    throw "The production homepage is missing the FlowBridge Systems brand."
}

$faviconBytes = [System.IO.File]::ReadAllBytes((Join-Path $siteRoot "favicon.ico"))
if ($faviconBytes.Length -lt 4 -or $faviconBytes[0] -ne 0 -or $faviconBytes[1] -ne 0 -or $faviconBytes[2] -ne 1 -or $faviconBytes[3] -ne 0) {
    throw "The FlowBridge browser icon is not a valid ICO file."
}

foreach ($brandElement in @("flowbridge-cosmos-mark.png", "favicon.ico", "THE FLOWBRIDGE MARK", "FlowBridge Systems constellation bridge logo")) {
    if ($homepage -notmatch [regex]::Escape($brandElement)) {
        throw "The production homepage is missing cosmic branding: $brandElement."
    }
}

foreach ($service in @("Nintex Automation K2 &amp; workflow", "Odoo ERP &amp; operations", "Microsoft Dynamics 365 Business Central", "Cybersecurity &amp; resilience", "CompTIA A+ education", "Microsoft Access &amp; Microsoft 365 automation")) {
    if ($homepage -notmatch [regex]::Escape($service)) {
        throw "The production homepage is missing $service."
    }
}

foreach ($platform in @("Nintex Automation K2", "Nintex Workflow", "Microsoft Dynamics 365 Business Central", "Microsoft 365", "Power Platform", "Odoo Community and Enterprise", "independent consultancy")) {
    if ($homepage -notmatch [regex]::Escape($platform)) {
        throw "The production homepage is missing commercial-platform branding: $platform."
    }
}

foreach ($bookingElement in @("PAID CONSULTATION", '$50 initial booking fee', "30–120 minutes", "Manual payment confirmation", "data-consultation-form", "Odoo One App Free", "Dynamics 365 Business Central trial")) {
    if ($homepage -notmatch [regex]::Escape($bookingElement)) {
        throw "The production homepage is missing the Odoo-connected booking flow: $bookingElement."
    }
}

foreach ($productElement in @("FLOWBRIDGE DIGITAL PRODUCTS", "FlowBridge Funny Tech Coloring Book: Cosmic Desk Mayhem", "20 printable pages", "Final page: FlowBridge services and booking guide", '$5.99', "data-product-order-form", "Manual payment and email fulfillment")) {
    if ($homepage -notmatch [regex]::Escape($productElement)) {
        throw "The production homepage is missing the digital-product storefront: $productElement."
    }
}

$siteScript = Get-Content -LiteralPath (Join-Path $siteRoot "app.js") -Raw
foreach ($bookingScriptElement in @("info@flowbridge-systems-llc.odoo.com", "Manual payment: `$50 initial booking fee", "data-consultation-form", "data-product-order-form", "Manual fulfillment: send payment instructions")) {
    if ($siteScript -notmatch [regex]::Escape($bookingScriptElement)) {
        throw "The production booking workflow is missing: $bookingScriptElement."
    }
}

foreach ($skill in @("PROFESSIONAL SKILLS", "Data recovery, backup-and-recovery systems", "CompTIA A+ certification", "IEEE &amp; NASA graduate research", "Artificial Intelligence for Defect Examination")) {
    if ($homepage -notmatch [regex]::Escape($skill)) {
        throw "The production homepage is missing skills content: $skill."
    }
}

foreach ($credential in @("MCSA: Web Applications", "MCSD: App Builder", "MCPS: Microsoft Certified Professional", "MS: Programming in HTML5 with JavaScript and CSS3", "MCSD: Web Applications", "K2 Five: Core", "CompTIA Security+ ce Certification", "ISC2 Candidate", "CompTIA A+ Certification")) {
    if ($homepage -notmatch [regex]::Escape($credential)) {
        throw "The production homepage is missing credential history: $credential."
    }
}

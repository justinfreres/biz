[CmdletBinding()]
param(
    [string]$SiteDirectory = "src\FlowBridge.Web\wwwroot"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$siteRoot = Join-Path $repositoryRoot $SiteDirectory
$requiredFiles = @("index.html", "style.css", "app.js", "favicon.svg", "admin\index.html", "admin\login.html", "admin\admin.js", "admin\login.js", "admin\admin.css")

foreach ($file in $requiredFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $siteRoot $file))) {
        throw "Missing required site asset: $file"
    }
}

$homepage = Get-Content -LiteralPath (Join-Path $siteRoot "index.html") -Raw
if ($homepage -notmatch "FlowBridge Systems") {
    throw "The production homepage is missing the FlowBridge Systems brand."
}

foreach ($service in @("Network &amp; infrastructure", "Business Central extensions", "Cybersecurity &amp; resilience", "CompTIA A+ education", "Access &amp; Office automation")) {
    if ($homepage -notmatch [regex]::Escape($service)) {
        throw "The production homepage is missing $service."
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


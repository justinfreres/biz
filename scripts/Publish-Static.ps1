[CmdletBinding()]
param(
    [string]$OutputDirectory = "dist"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repositoryRoot "src\FlowBridge.Web\FlowBridge.Web.csproj"
$publishDirectory = Join-Path $repositoryRoot "artifacts\publish"
$destination = Join-Path $repositoryRoot $OutputDirectory

if (Test-Path -LiteralPath $publishDirectory) {
    Remove-Item -LiteralPath $publishDirectory -Recurse -Force
}

dotnet publish $projectPath --configuration Release --no-restore --output $publishDirectory
if ($LASTEXITCODE -ne 0) {
    throw "ASP.NET Core publish failed."
}

$siteRoot = Join-Path $publishDirectory "wwwroot"
if (-not (Test-Path -LiteralPath (Join-Path $siteRoot "index.html"))) {
    throw "The publish output does not contain a static website."
}

if (Test-Path -LiteralPath $destination) {
    Remove-Item -LiteralPath $destination -Recurse -Force
}

New-Item -ItemType Directory -Path $destination -Force | Out-Null
Copy-Item -Path (Join-Path $siteRoot "*") -Destination $destination -Recurse -Force

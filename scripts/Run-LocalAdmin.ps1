[CmdletBinding()]
param(
    [string]$Urls = "http://127.0.0.1:5010"
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($env:FLOWBRIDGE_ADMIN_PASSWORD)) {
    throw "Set FLOWBRIDGE_ADMIN_PASSWORD to a unique local password before running the content administrator."
}

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repositoryRoot "src\FlowBridge.Web\FlowBridge.Web.csproj"
dotnet run --project $projectPath --urls $Urls

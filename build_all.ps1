param(
    [string]$GameDir = "C:\Program Files (x86)\Steam\steamapps\common\smt3hd",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$dotnet = "C:\Program Files\dotnet\dotnet.exe"

if (-not (Test-Path -LiteralPath $dotnet)) {
    $dotnet = "dotnet"
}

$projects = @(
    "source\KeptSuspense\KeptSuspense.csproj",
    "source\MoonKing\MoonKing.csproj",
    "source\PreyEyes2\PreyEyes2.csproj",
    "source\QuickPass\QuickPass.csproj",
    "source\SafePassage\SafePassage.csproj",
    "source\SkipIntro\SkipIntro.csproj",
    "source\SuspendSafe\SuspendSafe.csproj"
)

foreach ($project in $projects) {
    Write-Host "Building $project"
    & $dotnet build $project -c $Configuration /p:GameDir="$GameDir"
}

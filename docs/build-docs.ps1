# Build script for generating documentation using DocFX
# Usage: Run from repo root in PowerShell: .\docs\build-docs.ps1

Push-Location docs

# If docfx is not available, try installing it as a global dotnet tool
if (-not (Get-Command docfx -ErrorAction SilentlyContinue)) {
    Write-Host "docfx not found. Installing docfx.console as a .NET global tool (this requires internet)."
    dotnet tool install -g docfx.console
    if (-not (Get-Command docfx -ErrorAction SilentlyContinue)) {
        Write-Error "docfx installation failed or docfx not available on PATH. Please install DocFX manually: https://dotnet.github.io/docfx/"
        Pop-Location
        exit 1
    }
}

Write-Host "Running 'docfx metadata' to extract API metadata from the project..."
# Generate metadata (reads csproj and produces api YAML)
docfx metadata

Write-Host "Running 'docfx build' to produce the static site in docs/_site..."
docfx build

Write-Host "Docs built at: $(Resolve-Path .\_site)"
Pop-Location

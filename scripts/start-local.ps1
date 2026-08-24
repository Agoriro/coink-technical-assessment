[CmdletBinding()]
param(
    [switch]$Detach
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$environmentPath = Join-Path $repositoryRoot '.env'
$environmentExamplePath = Join-Path $repositoryRoot '.env.example'
$passwordPlaceholder = 'replace_with_a_local_password'

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw 'Docker was not found. Install and start Docker Desktop, then try again.'
}

if (-not (Test-Path -LiteralPath $environmentPath)) {
    $randomBytes = New-Object byte[] 32
    $randomNumberGenerator = [System.Security.Cryptography.RandomNumberGenerator]::Create()

    try {
        $randomNumberGenerator.GetBytes($randomBytes)
    }
    finally {
        $randomNumberGenerator.Dispose()
    }

    $generatedPassword = [System.BitConverter]::ToString($randomBytes).Replace('-', '')
    $environmentTemplate = [System.IO.File]::ReadAllText($environmentExamplePath)

    if (-not $environmentTemplate.Contains($passwordPlaceholder)) {
        throw "Expected password placeholder was not found in $environmentExamplePath."
    }

    $environmentContent = $environmentTemplate.Replace(
        $passwordPlaceholder,
        $generatedPassword)
    $utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText(
        $environmentPath,
        $environmentContent,
        $utf8WithoutBom)

    Write-Host 'Created ignored .env with a generated local database password.'
}
else {
    Write-Host 'Using existing ignored .env configuration.'
}

Push-Location $repositoryRoot

try {
    & docker compose version *> $null
    if ($LASTEXITCODE -ne 0) {
        throw 'Docker Compose v2 is unavailable. Start or update Docker Desktop.'
    }

    $composeArguments = @('compose', 'up', '--build')
    if ($Detach) {
        $composeArguments += @('--detach', '--wait')
    }

    & docker @composeArguments
    if ($LASTEXITCODE -ne 0) {
        throw "Docker Compose exited with code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}

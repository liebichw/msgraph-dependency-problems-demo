#requires -PSEdition Core

$SetupRoot="Setup"

if ( Test-Path -Path $SetupRoot ) {
    Remove-Item -Recurse -Force $SetupRoot
}

try {
    
    New-Item -Path "." -Name "$SetupRoot" -ItemType "directory"

    Copy-Item -Path $(Join-Path "pluginimpl" "bin" "Debug" "*") -Destination "$SetupRoot"
    Copy-Item -Path $(Join-Path "TestMSTeams" "bin" "Debug" "*") -Destination "$SetupRoot"
} catch {
    Write-Error "Error copying data: $($error[0])"
}


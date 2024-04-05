#requires -PSEdition Core

$SetupRoot="Setup"

try {
    cd "$SetupRoot"

    $ENV:TENANT_ID="<PLEASE INSERT TENANT ID HERE>"
    $ENV:CLIENT_ID="<PLEASE INSERT CLIENT ID HERE>"
    $ENV:REDIRECT_URL="<PLEASE INSERT REDIRECT URL HERE>"
    $PluginPath=$(Join-Path "." "pluginimpl.dll")
    Write-Host "PluginPath = $PluginPath"
    $ENV:PLUGIN_PATH=$(Resolve-Path "$PluginPath")
    $ENV:PLUGIN_FACTORY="pluginimpl.PluginMsGraph5Factory"
    $ENV:USE_PROXY="true" # Assuming proxy usage here - also works w/o proxy

    $exe = $(Join-Path "." "TestMSTeams.exe")

    Write-Host "Starting $exe"
    Invoke-Expression "$exe"
    Write-Host "Started $exe"
    cd ..
} catch {
    Write-Error "Error executing test: $($error[0])"
} finally {
  if (-Not $(Test-Path -Path $SetupRoot)) {
    cd ..
  }
}

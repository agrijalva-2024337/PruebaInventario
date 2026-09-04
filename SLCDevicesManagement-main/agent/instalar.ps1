param(
    [Parameter(Mandatory=$true)][string]$BackendUrl,
    [Parameter(Mandatory=$true)][string]$InstallKey
)

$installPath = "C:\Program Files\SLCDM"
New-Item -ItemType Directory -Force -Path $installPath | Out-Null
Copy-Item -Path ".\publish\*" -Destination $installPath -Recurse -Force

$config = @{
    "Backend" = @{
        "BaseUrl" = $BackendUrl
        "InstallKey" = $InstallKey
    }
} | ConvertTo-Json
Set-Content -Path "$installPath\appsettings.Production.json" -Value $config

New-Service -Name "SLCDMAgente" `
  -BinaryPathName "$installPath\SLCDMAgente.exe" `
  -DisplayName "SLCDM Agente de Rastreo" `
  -StartupType Automatic

# Recovery: si el servicio se cae, Windows lo reinicia solo.
sc.exe failure SLCDMAgente reset= 86400 actions= restart/60000/restart/60000/restart/60000

Start-Service -Name "SLCDMAgente"
Get-Service -Name "SLCDMAgente" | Format-List Name, Status, StartType
Write-Host "Si Status no es Running, mira Visor de eventos > Application (origen SLCDM Agente de Rastreo)."

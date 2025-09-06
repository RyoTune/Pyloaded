# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/Pyloaded.Reloaded/*" -Force -Recurse
dotnet publish "./Pyloaded.Reloaded.csproj" -c Release -o "$env:RELOADEDIIMODS/Pyloaded.Reloaded" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location
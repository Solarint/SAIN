# Fetch the version from EscapeFromTarkov.exe
$tarkovPath = '{0}\..\..\..\..\EscapeFromTarkov.exe' -f $PSScriptRoot
$tarkovVersion = (Get-Item -Path $tarkovPath).VersionInfo.FileVersionRaw.Revision

# Update AssemblyVersion
$pluginSourcePath = '{0}\..\SAINPlugin.cs' -f $PSScriptRoot
$versionPattern = '^([ \t]+public const int TarkovVersion = )\d+;'
(Get-Content $pluginSourcePath) | ForEach-Object {
    if ($_ -match $versionPattern){
        '{0}{1};' -f $matches[1],$tarkovVersion
    } else {
        $_
    }
} | Set-Content $pluginSourcePath
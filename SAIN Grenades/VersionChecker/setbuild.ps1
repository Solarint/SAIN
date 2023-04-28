# Fetch the version from EscapeFromTarkov.exe
$tarkovPath = '{0}\..\..\..\EscapeFromTarkov.exe' -f $PSScriptRoot
$tarkovVersion = (Get-Item -Path $tarkovPath).VersionInfo.FileVersionRaw.Revision

# Update AssemblyVersion
$assemblyPath = '{0}\..\Properties\AssemblyInfo.cs' -f $PSScriptRoot
$versionPattern = '^\[assembly: TarkovVersion\(.*\)\]'
(Get-Content $assemblyPath) | ForEach-Object {
    if ($_ -match $versionPattern){
    	$versionType = $matches[1]
        '[assembly: TarkovVersion({0})]' -f $tarkovVersion
    } else {
        $_
    }
} | Set-Content $assemblyPath

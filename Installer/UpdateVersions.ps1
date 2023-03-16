# This script is intended to be run from the root of the repo, like .\Installer\UpdateVersions.ps1

$newVersion = $args[0]
$versionNotes = $args[1]

if ($args.count -eq 0) {
    $newVersion = Read-Host "Enter the new version number (without 'v' and without trailing '.0')"
}

# Directory.Build.props
$directoryBuildPropsFile = Get-Content "Directory.Build.props"
for ($i = 0; $i -lt $directoryBuildPropsFile.Length; $i += 1) {
    $line = $directoryBuildPropsFile[$i]
    
    if ($line -match "AssemblyVersion") {
        $directoryBuildPropsFile[$i] = "        <AssemblyVersion>$($newVersion).0</AssemblyVersion>"
    }

    if ($line -match "FileVersion") {
        $directoryBuildPropsFile[$i] = "        <FileVersion>$($newVersion).0</FileVersion>"
    }

    if ($line -match "InformationalVersion") {
        $directoryBuildPropsFile[$i] = "        <InformationalVersion>$($newVersion).0</InformationalVersion>"
    }
}

Set-Content "Directory.Build.props" $directoryBuildPropsFile

# RDPoverSSHSetupScript.iss
$setupScript = Get-Content "Installer\RDPoverSSHSetupScript.iss"
for ($i = 0; $i -lt $setupScript.Length; $i += 1) {
    $line = $setupScript[$i]

    if ($line -match "#define MyAppVersion") {
        $setupScript[$i] = "#define MyAppVersion ""$($newVersion)"""
    }
}

Set-Content "Installer\RDPoverSSHSetupScript.iss" $setupScript

# VersionInfo.xml
$versionInfo = Get-Content "RDPoverSSH\VersionInfo.xml"
for ($i = 0; $i -lt $versionInfo.Length; $i += 1) {
    $line = $versionInfo[$i]

    if ($line -match "<Version>") {
        $versionInfo[$i] = "  <Version>$($newVersion).0</Version>"
    }

    if ($line -match "ReleaseDate") {
        $versionInfo[$i] = "  <ReleaseDate>$(Get-Date -Format "yyyy-MM-dd")</ReleaseDate>"
    }

    if ($line -match "DownloadLink") {
        $versionInfo[$i] = "  <DownloadLink>https://github.com/micahmo/RDPoverSSH/releases/download/v$($newVersion)/RDPoverSSHSetup-$($newVersion).exe</DownloadLink>"
    }

    if ($line -match "DownloadFileName") {
        $versionInfo[$i] = "  <DownloadFileName>RDPoverSSHSetup-$($newVersion).exe</DownloadFileName>"
    }

    if ($line -match "<VersionNotes") {
        $startVersionNotesLines = $i
    }
    
    if ($line -match "</VersionNotes") {
        for ($j = $startVersionNotesLines; $j -le $i; $j += 1) {
            $versionInfo[$j] = $null
        }

        $versionInfo[$startVersionNotesLines] = "  <VersionNotes>$($versionNotes)</VersionNotes>"
    }
}

Set-Content "RDPoverSSH\VersionInfo.xml" $versionInfo

Write-Host -ForegroundColor Red "Don't forget to update VersionNotes!"
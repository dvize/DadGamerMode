param (
    [string]$TargetPath,
    [string]$TargetName,
    [string]$TargetDir,
    [string]$ConfigurationName,
    [string]$DestinationRoot = "F:\SPT-AKI-DEV\BepInEx\plugins",
	[string]$SourceFilePath = "C:\Users\dvize\Desktop\repos\SPT-Tarkov\Projects\DadGamerMode\Plugin.cs"  # Adjust this path as needed
)

Write-Output "TargetPath: $TargetPath"
Write-Output "TargetName: $TargetName"
Write-Output "TargetDir: $TargetDir"
Write-Output "ConfigurationName: $ConfigurationName"
Write-Output "DestinationRoot: $DestinationRoot"
Write-Output "SourceFilePath: $SourceFilePath"

# Extract version from the source file
$versionPattern = '\[BepInPlugin\(".*?", ".*?", "(.*?)"\)\]'
$version = Select-String -Path $SourceFilePath -Pattern $versionPattern | ForEach-Object {
    if ($_ -match $versionPattern) {
        return $matches[1]
    }
}

if (-not $version) {
    Write-Error "Version information not found in the source file."
    exit 1
}

Write-Output "Extracted Version: $version"

# Define the source and destination paths
$sourceDll = "$TargetPath"
$destinationDll = "$DestinationRoot\$TargetName.dll"
$zipFileName = "$TargetName$version.zip"
$zipPath = "$DestinationRoot\$zipFileName"

# Copy the DLL file
Copy-Item -Path $sourceDll -Destination $destinationDll -Force

# Handle the PDB file based on the configuration
if ($ConfigurationName -eq "Debug") {
    $sourcePdb = "$TargetDir\$TargetName.pdb"
    $destinationPdb = "$DestinationRoot\$TargetName.pdb"
    Copy-Item -Path $sourcePdb -Destination $destinationPdb -Force
} else {
    $destinationPdb = "$DestinationRoot\$TargetName.pdb"
    Remove-Item -Path $destinationPdb -ErrorAction SilentlyContinue
}

# Only run the packaging for Release configuration
if ($ConfigurationName -eq "Release") {
    # Check if there is an additional folder
    $subFolders = Get-ChildItem -Path $DestinationRoot -Directory
    $additionalFolder = $null
    foreach ($folder in $subFolders) {
        if (Test-Path -Path "$folder\$TargetName.dll") {
            $additionalFolder = $folder
            break
        }
    }

    $tempDir = Join-Path -Path $env:TEMP -ChildPath "temp"
    $bepinexDir = Join-Path -Path $tempDir -ChildPath "BepInEx"
    $pluginsDir = Join-Path -Path $bepinexDir -ChildPath "plugins"

    if ($null -eq $additionalFolder) {
        # No additional folder, package the DLL directly
        New-Item -ItemType Directory -Path $bepinexDir -Force
        New-Item -ItemType Directory -Path $pluginsDir -Force
        Copy-Item -Path $destinationDll -Destination "$pluginsDir\$TargetName.dll" -Force
    } else {
        # Additional folder found, package with the folder
        $additionalFolderName = Split-Path -Leaf $additionalFolder
        $destinationAdditionalFolder = Join-Path -Path $pluginsDir -ChildPath $additionalFolderName
        New-Item -ItemType Directory -Path $bepinexDir -Force
        New-Item -ItemType Directory -Path $destinationAdditionalFolder -Force
        Copy-Item -Path $destinationDll -Destination "$destinationAdditionalFolder\$TargetName.dll" -Force
    }

    Compress-Archive -Path "$tempDir\*" -DestinationPath $zipPath -Force

    # Clean up temporary files
    Remove-Item -Path "$tempDir" -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Output "Packaging completed."

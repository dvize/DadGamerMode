param (
    [string]$ConfigurationName,
    [string]$TargetPath,
    [string]$TargetName,
    [string]$TargetDir
)

# Define the base directory
$baseDir = "F:\SPT-AKI-DEV\BepInEx\plugins"

# Function to log messages to the console
function Log {
    param (
        [string]$message
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "$timestamp - $message"
}

Log "Script started"
Log "ConfigurationName: $ConfigurationName"
Log "TargetPath: $TargetPath"
Log "TargetName: $TargetName"
Log "TargetDir: $TargetDir"

# Get the assembly version
$assembly = [System.Reflection.Assembly]::LoadFile($TargetPath)
$version = $assembly.GetName().Version.ToString()
Log "Assembly version: $version"

# Determine the directory of the deployed DLL
$deployDir = Split-Path -Parent $TargetPath
Log "DeployDir: $deployDir"

# Check if the deploy directory is the base directory or one level further
if ($deployDir -ne $baseDir) {
    $relativePath = $deployDir.Substring($baseDir.Length + 1) # Get the relative path beyond the base directory
    Log "RelativePath: $relativePath"
    if (($relativePath -split '\\').Count -eq 1) { # Check if it's exactly one directory level further
        $directoryName = (Get-Item $deployDir).Name
        $zipPath = "F:\SPT-AKI-DEV\BepInEx\plugins\$directoryName-v$version.zip"
        Log "DirectoryName: $directoryName"
        Log "ZipPath: $zipPath"

        # Remove existing zip file if it exists
        if (Test-Path $zipPath) {
            Log "ZipPath exists, removing"
            Remove-Item $zipPath -Force
        }

        # Create the temp directory structure
        $tempZipDir = "F:\SPT-AKI-DEV\tempZip"
        if (Test-Path $tempZipDir) {
            Log "TempZipDir exists, removing"
            Remove-Item $tempZipDir -Recurse -Force
        }
        New-Item -ItemType Directory -Path $tempZipDir
        Log "TempZipDir created: $tempZipDir"

        $newZipStructure = Join-Path $tempZipDir "Bepinex\plugins\$directoryName"
        New-Item -ItemType Directory -Path $newZipStructure -Force
        Log "New zip structure directory created: $newZipStructure"

        # Copy files to the new zip structure
        Copy-Item -Path "$TargetDir\*" -Destination $newZipStructure -Recurse -Force
        Log "Files copied to new zip structure"

        # Create the final zip file
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::CreateFromDirectory($tempZipDir, $zipPath)
        Log "Final zip file created: $zipPath"

        # Clean up temp directory
        Remove-Item $tempZipDir -Recurse -Force
        Log "TempZipDir removed"
    } else {
        Log "RelativePath is not one directory level further"
    }
} else {
    $zipPath = "F:\SPT-AKI-DEV\BepInEx\plugins\$TargetName-v$version.zip"
    Log "ZipPath: $zipPath"

    # Remove existing zip file if it exists
    if (Test-Path $zipPath) {
        Log "ZipPath exists, removing"
        Remove-Item $zipPath -Force
    }

    # Create the temp directory structure
    $tempZipDir = "F:\SPT-AKI-DEV\tempZip"
    if (Test-Path $tempZipDir) {
        Log "TempZipDir exists, removing"
        Remove-Item $tempZipDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $tempZipDir
    Log "TempZipDir created: $tempZipDir"

    # Create the required folder structure within the temp directory
    $bepinexPluginsDir = Join-Path $tempZipDir "Bepinex\plugins"
    New-Item -ItemType Directory -Path $bepinexPluginsDir -Force
    Log "Bepinex\plugins directory created: $bepinexPluginsDir"

    # Copy the single DLL to the new structure
    Copy-Item -Path $TargetPath -Destination $bepinexPluginsDir -Force
    Log "DLL copied to Bepinex\plugins directory"

    # Create the final zip file
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory($tempZipDir, $zipPath)
    Log "Final zip file created: $zipPath"

    # Clean up temp directory
    Remove-Item $tempZipDir -Recurse -Force
    Log "TempZipDir removed"
}

Log "Script finished"

param(
  [Parameter(Mandatory = $true)]
  [string]$version
)

$ErrorActionPreference = 'Stop'
$scriptLocation = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$sdkRepository = $scriptLocation + "/.."

Function UpdateVersion($newVersion, $regexToParseTheVersion) {
  $currentBranch = git -C "$sdkRepository" rev-parse --abbrev-ref HEAD
  $manifestJsonFilePath = "$sdkRepository/Packages/manifest.json"
  $manifestJsonContent = Get-Content -Path $manifestJsonFilePath -Raw
  $oldVersion = [regex]::Match($manifestJsonContent, """io\.getready\.rgn\.core"": ""https://github\.com/readyio/RGNCore\.git#($regexToParseTheVersion)").Groups[1].Value
  Write-Host "Old version: $oldVersion"
  $updatedManifestJsonContent = $manifestJsonContent -replace $oldVersion, $newVersion
  Set-Content -Path $manifestJsonFilePath -Value $updatedManifestJsonContent -Encoding UTF8 -NoNewline
  Write-Output "Updated manifest.json from version $oldVersion to $newVersion in $currentBranch branch."
  Read-Host "Press Enter to continue..."
  
  # Get Unity Version
  $projectVersionPath = Join-Path -Path $sdkRepository -ChildPath "ProjectSettings/ProjectVersion.txt"
  $unityVersion = Get-Content $projectVersionPath | Select-Object -First 1 | ForEach-Object { $_ -replace "m_EditorVersion: ", "" }
  Write-Host "Unity Version: $unityVersion"
  Read-Host "Press Enter to continue..."

  # Locate Unity Executable
  $unityExecPath = "C:\Program Files\Unity\Hub\Editor\$unityVersion\Editor\Unity.exe"
  if (-Not (Test-Path $unityExecPath)) {
    Write-Error "Unity version $unityVersion not found in $unityExecPath."
    return
  }

  # Run Unity Method
  Start-Process -FilePath $unityExecPath -ArgumentList "-batchmode -quit -projectPath `"$sdkRepository`" -executeMethod ImportPackageSamples.ImportPackageExamples" -NoNewWindow -Wait
  if ($LASTEXITCODE -ne 0) {
    Write-Error "Unity Editor method execution failed. Error code: $LASTEXITCODE."
    return
  }
  Read-Host "Press Enter to continue..."

  # Add to Git and Commit
  git -C "$sdkRepository" add .
  $commitMessage = "Updated project to Unity SDK version $newVersion and imported package examples."
  git -C "$sdkRepository" commit -m $commitMessage
  Write-Host "Commit done: $commitMessage"
  Read-Host "Press Enter to continue..."

  git -C "$sdkRepository" push
}

git -C "$sdkRepository" status
Read-Host "This script will revert all your local repository changes and pull the latest version. Press Enter to continue..."
git -C "$sdkRepository" checkout .
git -C "$sdkRepository" clean -fd
git -C "$sdkRepository" pull

UpdateVersion $version "\d+\.\d+\.\d+"

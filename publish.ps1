$targetRef = $args[0]
$archiveName = $targetRef
if (!$targetRef) {
    $archiveName = "current"
}
$targetDirPath = "dist"
$targetArchPath = Join-Path $targetDirPath "$archiveName.zip"
$projects = "MiniAiCup.Paperio.Client", "MiniAiCup.Paperio.Core"
$filesToExclude = "*AssemblyInfo.cs"

if ($targetRef) {
    git checkout $targetRef
}

$projects | Get-ChildItem -Include *.cs -Exclude $filesToExclude -Recurse | Compress-Archive -DestinationPath $targetArchPath -Force -Confirm:$false

if ($targetRef) {
    git checkout -
}

$fullTargetDirPath = Resolve-Path -Path $targetDirPath
Write-Output "Published to file://$fullTargetDirPath"

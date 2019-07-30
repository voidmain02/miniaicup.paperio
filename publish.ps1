$targetRef = $args[0]
$archiveName = $targetRef
if (!$targetRef) {
    $archiveName = "current"
}
$targetArchPath = Join-Path "dist" "$archiveName.zip"
$projects = "MiniAiCup.Paperio.Client", "MiniAiCup.Paperio.Core"
$filesToExclude = "*AssemblyInfo.cs"

if ($targetRef) {
    git checkout $targetRef
}

$projects | Get-ChildItem -Include *.cs -Exclude $filesToExclude -Recurse | Compress-Archive -DestinationPath $targetArchPath -Force -Confirm:$false

if ($targetRef) {
    git checkout -
}

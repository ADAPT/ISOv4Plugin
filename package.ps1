Param(
  [Parameter(Mandatory = $true)]
  [string] $Tag,
  [bool] $UseBranch,
  [string] $ApiKey
)

$ErrorActionPreference = "Stop";

if ($Tag -match '(?<=^v)\d+\.\d+\.\d+(?:.+)?$') {
  $version = $Matches[0]
}
else {
  throw "Incorrect tag format"
}

if ($version -match '^\d+\.\d+\.\d+') {
  $versionNumber = $Matches[0]
}
else {
  throw "Incorrect tag format"
}

if (-not $UseBranch) {
  $branch = git symbolic-ref --short HEAD

  git fetch -a
  git checkout $Tag
  if ($LastExitCode -ne 0) {
    throw "Could not checkout tag"
  }
}

Remove-Item -Path ./ISOv4Plugin/bin -Recurse -Force
Remove-Item -Path ./ISOv4Plugin/obj -Recurse -Force

nuget restore
if ($LastExitCode -ne 0) {
  throw "NuGet packages could not be restored"
}

msbuild ./ISOv4Plugin/ISOv4Plugin.csproj /m /p:Configuration=Release /p:Version=$version /p:FileVersion=$versionNumber.0 /t:rebuild /v:minimal
if ($LastExitCode -ne 0) {
  throw "Solution could no be built"
}

nuget pack ./AgGatewayISOPlugin.nuspec -verbosity detailed -outputdirectory ./dist -version $version
if ($LastExitCode -ne 0) {
  throw "Package could no be built"
}

if (-not ([string]::IsNullOrEmpty($ApiKey))) {
  nuget push ./dist/AgGatewayISOPlugin.$version.nupkg -apikey $ApiKey -source https://api.nuget.org/v3/index.json
  if ($LastExitCode -ne 0) {
    throw "Package could no be published"
  }
}

if (-not $UseBranch) {
  git checkout $branch
}

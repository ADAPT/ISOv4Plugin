Param(
  [Parameter(Mandatory = $true)]
  [string] $tag,
  [string] $apiKey
)

$ErrorActionPreference = "Stop";

if ($tag -match '(?<=^v)\d+\.\d+\.\d+(?:.+)?$') {
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

$branch = git symbolic-ref --short HEAD

git fetch -a
git checkout $tag
if ($LastExitCode -ne 0) {
  throw "Could not checkout tag"
}

nuget restore
if ($LastExitCode -ne 0) {
  throw "NuGet packages could not be restored"
}

msbuild IsoPlugin.sln /m /p:Configuration=Release /p:Version=$version /p:FileVersion=$versionNumber.0 /t:rebuild /v:minimal
if ($LastExitCode -ne 0) {
  throw "Solution could no be built"
}

nuget pack ./AgGatewayISOPlugin.nuspec -verbosity detailed -outputdirectory ./dist -version $version
if ($LastExitCode -ne 0) {
  throw "Package could no be built"
}

if (-not ([string]::IsNullOrEmpty($apiKey))) {
  nuget push ./dist/AgGatewayISOPlugin.$version.nupkg -apikey $apiKey -source https://api.nuget.org/v3/index.json
  if ($LastExitCode -ne 0) {
    throw "Package could no be published"
  }
}

git checkout $branch

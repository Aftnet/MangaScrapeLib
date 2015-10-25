$NugetPath =  Get-ChildItem -Recurse -Path ".\packages" -Filter "NuGet.exe";
$NugetPath = $NugetPath.FullName;

$OutputDir = ".\GeneratedPackages";
New-Item "$OutputDir" -type directory -Force;

$ProjectPaths = Get-ChildItem -Recurse -Path "*.csproj";
foreach($i in $ProjectPaths)
{
    $NuspecExists = (Get-ChildItem -Path $i.Directory -Filter "*.nuspec").Count -gt 0;
    if($NuspecExists)
    {
        &$NugetPath pack "$i" -IncludeReferencedProjects -Symbol -Prop Configuration=Release -OutputDirectory "$OutputDir";
    }
}

$PackagesPaths = Get-ChildItem -Path "$OutputDir" -Exclude "*.symbols.nupkg";
foreach($i in $PackagesPaths)
{
    &$NugetPath push $i.FullName;
}

Remove-Item  "$OutputDir" -Recurse -Force
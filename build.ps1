# download copy of godot 3.6, if not already downloaded
if (-Not (Test-Path('.\Godot_v3.6-stable_mono_win64'))) {
    Invoke-WebRequest https://github.com/godotengine/godot-builds/releases/download/3.6-stable/Godot_v3.6-stable_mono_win64.zip -OutFile Godot_v3.6-stable_mono_win64.zip
    Expand-Archive Godot_v3.6-stable_mono_win64.zip -DestinationPath .
    Remove-Item Godot_v3.6-stable_mono_win64.zip
}

$modName = "ExampleMod"
$version = "1.0.0.0"

# create dist folder
New-Item -ItemType Directory -Force -Path "dist"

# build godot project
Godot_v3.6-stable_mono_win64\Godot_v3.6-stable_mono_win64.exe --no-window --path . --export-pack "Build" "dist\Mod.pck" | Out-Default

# build c# project
Remove-Item -Path ".mono" -Recurse -Force
dotnet build Mod.csproj /t:Restore /p:Configuration=ExportDebug | Out-Default
dotnet build Mod.csproj /t:Build /p:Configuration=ExportDebug /p:Version="$($version)" | Out-Default
Copy-Item ".mono\temp\bin\ExportDebug\Mod.dll" -Destination "dist"

# pack mod
Compress-Archive -Force -Path "dist\Mod.dll", ".\dist\Mod.pck" -DestinationPath "dist\$($modName).zip"

# this should be the correct target folder for mods in most cases
$targetDir = "$($env:APPDATA)\Godot\app_userdata\Card Stuff\mods"
New-Item -ItemType Directory -Force -Path $targetDir

# copy dll to target folder
Copy-Item "dist\$($modName).zip" -Destination $targetDir
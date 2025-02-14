# download copy of godot 3.6, if not already downloaded
if (-Not (Test-Path('.\Godot_v3.6-stable_mono_win64'))) {
    Invoke-WebRequest https://github.com/godotengine/godot-builds/releases/download/3.6-stable/Godot_v3.6-stable_mono_win64.zip -OutFile Godot_v3.6-stable_mono_win64.zip
    Expand-Archive Godot_v3.6-stable_mono_win64.zip -DestinationPath .
    Remove-Item Godot_v3.6-stable_mono_win64.zip
}

# this should be the correct target folder for mods in most cases
$targetDir = "$($env:APPDATA)\Godot\app_userdata\Card Stuff\mods"
New-Item -ItemType Directory -Force -Path $targetDir

# build godot project
Godot_v3.6-stable_mono_win64\Godot_v3.6-stable_mono_win64.exe --no-window --path . --export-pack "Build" "$($targetDir)\ExampleMod.pck" | Out-Default

# build c# project
dotnet build ExampleMod.csproj /t:Restore /p:Configuration=ExportDebug | Out-Default
dotnet build ExampleMod.csproj /t:Build /p:Configuration=ExportDebug | Out-Default

# copy dll to target folder
Copy-Item ".mono\temp\bin\ExportDebug\ExampleMod.dll" -Destination $targetDir
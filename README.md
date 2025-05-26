# Modding for Lost For Swords

So you want to make a mod for the game Lost For Swords? Add your own cards, change the game? You've come to the right place! This repository contains an example mod that should help you get started! :)

It's not easy, but I will try to improve the experience and make it easier over time!

## Disclaimer

Adding mods to your game can make the game unstable, crash or freeze. It can also lead to your current run becoming corrupt. It should however not nuke your overall progress and not affect the rest of your computer. You can always remove all mods from the mod folder to restore the original game.

## Requirements
* Windows
* Lost For Swords installed from Steam (version 1.47 or later)
* some knowledge of programming, preferably in C#. If you don't, you may be able to learn by the examples provided.
* .NET 8.0 (later version probably works too): https://dotnet.microsoft.com/en-us/download
* Godot 3.6 (will be automatically downloaded when building, you don't need to download it yourself)
* a copy of this repository
* Visual Studio Code for editing code and running the build script (recommended, but not required)

## Quickstart
After installing all the requirements, either clone the repository (using `git clone`) or download a snapshot of the repository (here: https://github.com/max-bytes/lfs-example-mod/archive/refs/heads/main.zip) and unpack(!) the zip file to a suitable location on your pc.

Create a config.json file in the root folder by duplicating the config.sample.json file and setting appropriate values for name, title, description and version. They will be used when building and publishing the game on Steam Workshop.

After that, open a command line and navigate to the directory containing the `build.ps1` file. This is important, as the build script might not work when called from any other directory. Run the `build.ps1` script on the command line with `.\build.ps1`. It should automatically:
* download a suitable version of Godot in the current folder
* fully build the included example mod
* install it in the main game by copying the built mod to the target folder `C:\Users\[youruser]\AppData\Roaming\Godot\app_userdata\Card Stuff\mods`

You should see two files in the target folder: 
* `ExampleMod.dll`, which contains the code
* `ExampleMod.pck`, which contains the assets

(Re)start the game and it should display that the example mod was loaded in the bottom left corner of the main menu screen. The mod contains:
* a Hero ("Stickman"), including its default starting deck
* a weapon card ("Test Card") for Knight and Stickman. You should see it in the Card Library.

The code of the example mod is very basic and fully contained within `Main.cs`.

## Examples
The `examples` folder contains all `behaviors` for armor, auras, potions, scrolls, spells, trinkets and weapons currently in the base game. That should give you a good introduction into how you can create your own card behaviors. You could start by copying and modifying an existing card behavior and create your own version of it!

## Running And Testing
The general way of working with a mod is

1. make changes to the .cs files (like `Main.cs`) and/or files in the `assets` folder
2. run `build.ps1`
3. (re)start the game

The best way to test your changes is in the Hero Academy! Create your own level that uses the new and modified cards to test them thorougly. That's much better than starting a tower run and hoping to see the changes there. Note that uploading levels made with modded cards is not possible.

## Assets
The only currently supported asset is making your own card portraits. You can include them by creating a .png file in the `assets/Textures` folder. Its filename should be the same as the card ID you defined in code. The recommended size for armor, potion, trinket and weapon portraits is 32x32 pixels. For auras, scrolls and spells its 64x32 pixels. See `testCard` for an example.

## Preparing and Uploading to Steam Workshop
When you are ready to share your mod with the Steam community, follow these instructions to publish your mod on the Steam Workshop:

* Make sure your `config.json` file is created and up-to-date, as described above
* Make sure Steam is running and you are logged in with a user that has the full version of the game
* Run `.\upload.ps1`. The first time it runs, it will create a Workshop item and store its ID (publishedFileID) in the `config.json` file. On subsequent runs it will update this exact item.
* When it has run successfully, the workshop item should soon after appear in the list of workshop items for the game: https://steamcommunity.com/workshop/browse/?appid=2638050
* Others can now install your mod by subscribing to it, which will automatically download and install through Steam.

## Troubleshooting
When a mod is loaded, it should show up in the bottom left corner of the main menu of the game. If it does not, or it does not work as expected, a look into the log files might help. The log files are located at `<YourUserDirectory>\AppData\Roaming\Godot\app_userdata\Card Stuff\logs`. Look for errors and warnings that relate to your mod.

## Current Status
What is working and what isn't:
### Working
* creating cards and making them part of the card pool for heroes
* removing existing cards
* modifying existing cards (via removal and re-adding with different behavior)
* adding new heroes
* adding alternative starting decks
* advanced card behavior: action interceptors

### Not working yet (list not exhaustive)
* adding or modifying towers
* adding or modifying quests
* adding or modifying mezzanines
* card translations
* advanced card behavior: buffs

## Updating
From time to time, new updates for the game might introduce breaking changes. Such changes may break a mod and will require the mod-author to update their mod for a fix.

The way to update a mod to the latest version is generally:
* update the dependencies LFSBase, LFSComponent and LFSCore to the latest version. The example mod is set to use the latest version (in the mod's .csproj file, the PackageReference lines specify "*" for the version). You may need to tell Nuget to fetch the latest version using `dotnet build Mod.csproj /t:Restore --no-cache /p:Configuration=ExportDebug | Out-Default`
* after that, fix all compile errors to ensure your mod is compatible with the latest base game interface (IModRegister)
* upload a new version to Steam Workshop (don't forget to bump the version number in `config.json`)
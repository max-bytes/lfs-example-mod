# Modding for Lost For Swords

So you want to make a mod for the game Lost For Swords? Add your own cards, change the game? You've come to the right place! This repository contains an example mod that should help you get started! :)

It's not easy, but I will try to improve the experience and make it easier over time!

## Disclaimer

Adding mods to your game can make the game unstable, crash or freeze. It can also lead to your current run becoming corrupt. It should however not nuke your overall progress and not affect the rest of your computer. You can always remove all mods from the mod folder to restore the original game.

## Requirements
* Windows
* Lost For Swords (demo) installed from Steam or itch.io (version 1.44 or later)
* some knowledge of programming, preferably in C#. If you don't, you may be able to learn by the examples provided.
* .NET 8.0 (later version probably works too): https://dotnet.microsoft.com/en-us/download
* Godot 3.6 (will be automatically downloaded when building, you don't need to download it yourself)
* a copy of this repository, either via download (https://github.com/max-bytes/lfs-example-mod/archive/refs/heads/main.zip, unpacked in a folder of your choice) or git checkout
* Visual Studio Code for editing code and running the build script (recommended, but not required)

## Quickstart
After installing all the requirements, run the `build.ps1` script from this repository on the command line. It should automatically:
* fully build the included example mod
* install it in the main game by copying the built mod to the target folder `C:\Users\[youruser]\AppData\Roaming\Godot\app_userdata\Card Stuff\mods`

You should then see two files in the target folder: 
* `ExampleMod.dll`, which contains the code
* `ExampleMod.pck`, which contains the assets

(Re)start the game and it should display that the example mod was loaded in the bottom left corner of the main menu screen. The mod contains only a single test card: a weapon for Knight called "Test Card". You should see it in the Card Library.

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
The only currently supported asset is making your own card portraits. You can include them by creating a .png file in the `assets/Textures` folder. Its filename should be the same as the card ID you defined in code. The recommended size for armor, potion, trinket and weapon portraits is 32x32 pixels. For auras, scrolls and spells its 64x36 pixels. See `testCard` for an example.

## Current Status
What is working and what isn't:
### Working
* creating cards and making them part of the card pool for heroes
* removing existing cards
* modifying existing cards (via removal and re-adding with different behavior)
* advanced card behavior: action interceptors

### Not working yet (list not exhaustive)
* adding new heroes
* adding or modifying towers
* adding alternative starting decks
* adding or modifying quests
* adding or modifying mezzanines
* card translations
* advanced card behavior: buffs
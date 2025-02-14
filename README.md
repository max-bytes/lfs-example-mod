# example mod for Lost For Swords

So you want to make a mod for the game Lost For Swords? Add your own cards, change the game? You've come to the right place! :)

It's not easy, but I will try to improve the experience and make it easier over time!

## Requirements
* Windows
* Lost For Swords (demo) installed from Steam or itch.io (version 1.44 or later)
* some knowledge of programming, preferably in C#. If you don't, you may be able to learn by the examples provided.
* .NET 8.0 (later version probably works too): https://dotnet.microsoft.com/en-us/download
* Godot 3.6 (will be automatically downloaded when building, you don't need to download it yourself)
* a copy of this repository, either via download (https://github.com/max-bytes/lfs-example-mod/archive/refs/heads/main.zip) or git checkout
* Visual Studio Code for editing code and running the build script (recommended, but not required)

## Quickstart
After installing all the requirements, run the `build.ps1` script from this repository on the command line. It should automatically:
* fully build the included example mod
* install it in the main game by copying the built mod to the target folder `C:\Users\[youruser]\AppData\Roaming\Godot\app_userdata\Card Stuff\mods`

You should then see two files in the target folder: 
* `ExampleMod.dll`, which contains the code
* `ExampleMod.pck`, which contains the assets

(Re)start the game and it should display that the example mod was loaded in the bottom left corner of the main menu screen. The mod contains only a single test card: a weapon for Knight called "Test Card". You should see it in the Card Library.







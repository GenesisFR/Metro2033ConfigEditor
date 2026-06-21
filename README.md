# Metro 2033 Config Editor

## Description

This is a config editor for the original Steam version of _Metro 2033_ (not Redux).  
It allows you to edit most game settings as well as some hidden settings (such as FOV, cheats or windowed mode) without running the game.  

## Requirements

- Windows 7 or later
- [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)
- [Visual Studio 2026](https://visualstudio.microsoft.com/vs/) (developers only)

## Installation

You can run the program anywhere on your computer. However, it'll be easier for Steam/game detection if it's placed where the game executable is located.

## Usage

1. Run the program.
2. Customize your settings.
3. Make sure all paths are specified.
4. Hit `Save`.
5. Run the game.

You can reload the config by pressing the Reload button.  
You can use the `Start game (non-Steam)` button to run the game from the specified path.  
You can use the `Start game (Steam)` button to run the game using the Steam browser protocol.  

## Arguments

`-log`: when closing the program, it will create a log file in the same directory.

## Credits

- [andulv](https://github.com/andulv): author of the Steam library function
- [Binman88](http://www.tested.com/forums/games/10051-metro-2033-pc-config-file-edits/): provided some useful config file edits
- [cdorst](http://twitch.tv/cdorst): helped for the update notification system
- [JottiLF](https://community.pcgamingwiki.com/files/file/416-nox360/): tweaked x360ce to disable Xbox 360 controllers
- [Paul_NZ](https://community.pcgamingwiki.com/files/file/533-metro-2033-no-intro-fix/): created the no intro fix

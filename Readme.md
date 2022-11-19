# Higher Sidebar
## About
Increases the height of the info text box.

![Showcase](https://raw.githubusercontent.com/MSchmoecker/StacklandsHigherUI/master/Docs/Showcase.png)

## Config
The height of the info box can be set custom and the remaining space is filled with the sidebar.
By default the info box is scaled dynamically by the needed text space, this can be toggled with "Use Dynamic Height" to use a static height.

The config entries are generated after the first start at `BepInEx/config/com.maxsch.stacklands.higherui.cfg`.
The config file can also be edited while the game is running and the changes will be applied in-game upon saving the file.


## Manual Installation
1. Download and install BepInEx from [Thunderstore](https://stacklands.thunderstore.io/package/BepInEx/BepInExPack_Stacklands)
2. Download this mod and extract it into `BepInEx/plugins/`
3. Launch the game! If everything works, you should already see that the height of the sidebar elements is increased.


## Development
1. Install BepInEx
2. Set your `GAME_PATH` in `HigherSidebar.csproj`
3. This mod requires publicized game code, this removes the need to get private members via heavy Reflection code. Use https://github.com/CabbageCrow/AssemblyPublicizer for example to publicize `Stacklands/Stacklands_Data/Managed/GameScripts.dll`
4. Compile the project. This copies the resulting dll into `<GAME_PATH>/BepInEx/plugins/`


## Links
- Github: [https://github.com/MSchmoecker/StacklandsHigherUI](https://github.com/MSchmoecker/StacklandsHigherUI)
- Thunderstore: [https://stacklands.thunderstore.io/package/MSchmoecker/HigherSidebar](https://stacklands.thunderstore.io/package/MSchmoecker/HigherSidebar)
- Nexus: [https://www.nexusmods.com/stacklands/mods/4](https://www.nexusmods.com/stacklands/mods/4)
- [Offical Stacklands Discord](https://discord.gg/sokpop), my Discord tag: Margmas#9562


## Changelog
0.2.0
- Changed the static height to a dynamic height which is calculated by the needed text space. Can be toggled in the config

0.1.5
- Added a config watcher to reload the config in-game when the file changes

0.1.4
- Updated mod for game version 1.2.2 (dark forest update). This removes the configuration for the debug panel, since it is now at full height anyway (thanks benediktwerner!)

0.1.3
- Updated Thunderstore Readme

0.1.2
- Added ability to change the debug panel height (thanks benediktwerner!)

0.1.1
- Updated mod for game version 1.1.4 (islands update). As the sidebar now takes the remaining space, only the info box height has to be configured (thanks benediktwerner!)
- Changed BepInEx display name and dll name to Higher Sidebar. Make sure to delete the old `StacklandsHigherUI.dll`

0.1.0
- Initial release

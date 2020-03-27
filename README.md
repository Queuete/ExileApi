# ExileApi
Main project can be found here https://github.com/Qvin0000/ExileApi

Forum Thread discussing this repository can be found here https://www.ownedcore.com/forums/showthread.php?t=885371&p=4184338#post4184338

# Current Status
For information on the status please look into the posted owenedcore Thread.

# Release
https://github.com/Queuete/ExileApi/releases

# Pull Requests or other forms of input are appreciated
# How To Setup a Developer Version
Please look into the base repository the needed software is described there, including some troubleshooting https://github.com/Qvin0000/ExileApi

To use this version you need git, the plugins are handled as submodules. https://git-scm.com/downloads
1. Create a PoeHUD (any name) folder
2. Open a git bash inside the folder
3. Run `git clone https://github.com/Queuete/ExileApi`
4. Change into the directory `cd ExileApi/`
5. Clone the submodules `git submodule update --init --recursive`
6. Open the solution file in Visual Studio
(7.) This should compile already, there could be some reference errors, those are mostly fixed by removing the reference (solution exlporer-plugin_project->references) the ones with warnings need to be readded
8. There are some static files (yet) not included in the source. Therefore you need to once download the latest release and unpack it in the PoeHUD folder. After that you got an PoeHelper folder which has the release inside. Your new builds from visual studio will overwrite this release state.

Tools used for reverse engineering are mostly: ReClass.NET, CheatEngine, Ghidra
When you want to learn something about it, get comfortable with at least the first tool or second tool and try to comprehend already updated offsets. (For 3.10 I advise you to start with the LifeComponent)


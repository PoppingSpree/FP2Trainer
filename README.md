### GENERAL INFORMATION:

A speedrun-focused trainer toolkit mod for Freedom Planet 2. Based on [MelonLoader](https://github.com/LavaGang/MelonLoader).

Includes features such as:
+ Mark locations within the level as warp points to return to with a hotkey.
+ Switch to any level quickly with the new in-level Level Select Menu.
+ Character / Enemy data inspector with viewers for Movement, Combat, and DPS check.
+ Load custom content from Assetbundles at the press of a hotkey. This opens up the possibility of custom levels or extra features.

### HOW TO INSTALL:

1: Get Melon Loader https://github.com/LavaGang/MelonLoader/releases

2: Get FP2Trainer.dll https://github.com/PoppingSpree/FP2Trainer/releases/tag/FP2Trainer-Release

3: Install Melon Loader into your copy of FP2. (You may want to copy your game folder and mod the copy instead because updates are coming fast right now and mods will break.)

4: Add the FP2Trainer.dll to your Mods folder in your modded copy of the game. (If the Mods folder didn't appear when you installed Melon Loader, try running the game again, close it, and then check if the folder has been generated.)

5: If the version of the trainer you downloaded includes an AssetBundles folder, that entire folder and its contents goes within the Mods folder as a subfolder. <FP2InstallDir/Mods/AssetBundles/>

6: Launch the game. After the first set of intros, the game should immediately skip to the MainMenu (Or a level of your choice, see below). Once in-level, you will see the data viewer on screen. Good luck, and happy training!


### HOW TO USE:

**WARNING:** Use of this Trainer has the potential to corrupt your game save data depending on how you use it. Because of this, I highly recommend you backup your save data and the entire game folder before using this.
Typically, the save files will be at C:\Users\<YOUR username here>\AppData\LocalLow\GalaxyTrail\Freedom Planet 2
The files 1.json, 2.json, 3.json are your save slots from the main menu. The global.json contains things that apply regardless of which slot you pick. I recommend backing these up.

+ Warps: Hold Guard and tap Jump to set a warp point. Hold Guard and tap Special to warp to it. I don't have a way to bind these to unique buttons from the options yet. It might make more sense if I bind those to function keys on the keyboard or something.
+ More data. A bunch of data points from the FPPlayer object that might be useful. I don't entirely know what they do. I'll try to make this configurable later.
+ F9: Debug Room (Additive, the stage you were in previously is still in memory.)
+ F8: Main Menu
+ F7: Load Asset Bundles from <FP2 Install Dir>/Mods/AssetBundles. Nobody else is making asset bundle mods for FP2 yet as far as I know.
+ F6: Level Select. Use Up and Down keys/buttons to change the number and Jump/Confirm to select. A bit buggy atm, will sometimes read the confirm input 20 times and play a loud sound. Will fix later.
+ F5: Supposed to toggle between level select and pause menu. Doesn't work.
+ F4: Toggles the currently displayed data-page on the data viewer: [MOVEMENT, COMBAT, DPS, DPS_ALL, BATTLESPHERE, BOSS, NONE]
+ F3: Loads the scene with the highest load index number. For vanilla FP2 1.0.7r this should be Neera's tutorial. If you loaded Asset Bundles from the mods folder with F7, this will probably be a scene imported from one of those.
+ F2: ????
+ F1: ????
+ Numpad Plus: Make the Data Viewer text bigger.
+ Numpad Minus: Make the Data-Viewer text smaller.


DPS tracker is a bit funky, on normal DPS mode it only detects very specific kinds of enemies, focusing on whichever is closest to you.
On DPSAll, it tries to check for all generic enemies (not bosses) that are currently active near camera range.
Boss only works if there is a boss HUD on screen.

Battlesphere option doesn't do anything useful. I'd remove it but then I'd need to bugtest it and right now I'd much rather go to bed.

I don't know how good DPS meters work, so the gist of what I'm doing is that the very first second after the DPS queue is clear, you get the flat DPS for that second. From seconds 2-5, you get the total DPS divided by amount of time since the first hit. From second 10 onwards, it drops all hits that are more than 5seconds old and keeps checking against the 5 second range until the "combo drops" (empty hits queue).

If you just want an invincible training dummy to test DPS on, set it on DPS All and find some of those big bells in Shenlen Park. Some of them register damage for some reason. No clue why.


### QUESTIONS AND ANSWERS
Q: I encountered a bug in FP2 while using this trainer. Should I report it?

A: You can let me know about bugs you encounter if you believe they are trainer related and I may look into it. 
If you think you've found a bug native to FP2 itself, you should try to reproduce the behavior with MelonLoader completely disabled before reporting it as a bug to GalaxyTrail to prevent wasting their time on bugs they're not responsible for.


Q: Is there a quick way to disable mods when I want to do a run?

A: Close the game, temporarily rename the version.dll that from MelonLoader to something else (like version.dll.off), then relaunch the game.


Q: The game seems to be crashing more often when I have this tool installed. What happened?

A: Honestly, I have no clue. Crashes more often on me too. Most likely this is an error in my own code. If you happen to have ideas on how to fix it, feel free to report the situation of the crash to me, or to code up a pull request and submit to me.


Q: Why are there 6 listings for position and speed in the Battlesphere view?

A: There's 5 slots for players in this system that seem to never be used and I have no flipping clue what's up with that.


Q: Does this mod contain malware?

A: If you got it from the PoppingSpree Github releases, it __shouldn't__ but there is always the chance that files are compromised. You should always run your own antivirus scans on mods and AssetBundles to be sure. 

### LICENSING & CREDITS:

FP2Trainer is licensed under the Apache License, Version 2.0. See [LICENSE](https://github.com/LavaGang/TestMod/blob/master/LICENSE.md) for the full License.
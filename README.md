# Library of Ruina Archipelago
Library of Ruina Archipelago (LORAP for short) is a Harmony mod for Library of Ruina to make it work with Archipelago.

[Archipelago](https://archipelago.gg) is a cross-game modification system which randomizes different games, then uses the result to build a single unified multi-player game. Items from one game may be present in another, and you will need your fellow players to find items you need in their games to help you complete your own.

**Small FAQ later in this text btw!**

## Installation
If you're entirely new to Archipelago and GitHub, consider using [this Guide](https://az-lastpenguin.github.io/guide).

Download the mod from [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3430626765).
The mod also requires [BaseMod](https://steamcommunity.com/workshop/filedetails/?id=2603522001) (Enable Priority Loader, please)

If you wish to install this mod manually, you have to download the mod from the release page and drop it into Library Of Ruina/LibraryOfRuina_Data/Mods folder. Basemod still required. Make sure to unsubscribe from the mod in workshop so it doesn't break.
I don't know where to find BaseMod not in workshop, so i can't provide a link for it, sorry.

## About other mods support
The mod has **NO SUPPORT** for other **gameplay** mods. You can still install visual mods, most of which shouldn't break anything. But gameplay altering mods might and **WILL** break stuff.
Support for other mods *might* be implemented later on.

## Features
Planned and existing features, not in any particular order:
- [ ] Removed Story (As to not get distracted)
- [X] Randomized reception tree
- [X] Randomized abnormality suppression order
- [X] Randomized book requirements
- [X] Randomized book pages
- [X] Randomized floor Abno/EGO Pages 
- [ ] Randomized combat/key pages
- [ ] Randomized enemies
- [ ] Filler/Trap items
- [ ] Pop Tracker
- [ ] In-game Client
- [ ] YAML Generator
- [ ] Extended player/AI capabilities (Acquiring enemies pages, enemies using abno pages and etc.)

## FAQ
### What's being randomized?
- Order of Receptions
- Book requirements of Receptions
- Order of Abnormality Suppressions & Realizations
- Combat/Key Pages in Books

### What are the goals?
- X Receptions of Reverberation Ensemble
- Black Silence Reception
- Keter Realization
- Distorted Ensemble

### What are the items?
- Every book in the game
- Special "Book of Everything" that contains random pages
- Every floor
- Abnormality Pages
- EGO Pages
- Librarians
- Passive attribution points/max passives
- Max Emotion Level

## Track progress
You can check progress for the mod, things that are completed, will be done, or still just an idea in [this project](https://github.com/users/Az-LastPenguin/projects/1/views/1?layout=board).

## Share ideas
Visit [AfterDark Archipelago server](https://discord.gg/Sbhy4ykUKn) and [this channel](https://ptb.discord.com/channels/1085716850370957462/1137643745051955200) where me and the testers reside. You can discuss the mod, ideas for it, or anything relevant there. I'm always checking the chat. Another good way to let me know about your ideas is to [create an issue](https://github.com/Az-LastPenguin/LORAP/issues/new) with your idea, i will tag it appropriately.
You can also contact me on Discord directly: last_penguin

## Bug report
You can send screenshots and descriptions of bugs you found in the aforementioned channel. But i would **REALLY** appreciate if you could [create an issue](https://github.com/Az-LastPenguin/LORAP/issues/new), just so i could keep track of things more easily.

## Versioning
I'm using [SemVer 2.0.0](https://semver.org) as a guideline:
X.Y.Z-W where
- X is major version which is only incremented when something really big is added/changed which breaks everything.
- Y is the minor version which is only incremented when something not that big is added/changed which doesn't break anything.
- X is the patch version which is incremented when really small fixes/updates are made.
- W is the testing version before full release, denoted by a greek alphabet letter. Next letter is picked from the alphabet for each update to the testing version.

## Compiling and Contributing
If you wish to try and build this beast (not in a good way) of a spaghetti monster:

0. Install Visual Studio Community. You'll also need the ".NET app development" workload installed.
1. Clone the repository
2. Open the project and reference every DLL from DLLs folder to the project (right click references > Add Reference > Browse)
3. Install NuGet Packages:
  - Archipelago.MultiClient.Net
  - Krafs.Publicizer (Should publicize Assembly-CSharp.dll on it's own, if not, follow [this tutorial from PMCH](https://imgur.com/a/FxTU1nD))
  - HarmonyX 2.9.0 (SPECIFICALLY 2.9.0, it's the same BaseMod Uses)
4. You can build it now. Click Build > Build Solution. After that, right click the project > Open Folder in File Explorer. Compiled mod should be in LORAP/bin/Release.
5. To play it, copy mod files from the cloned repo ("Mod" folder), to the game's "Mods" folder, then from compiled mod folder copy LORAP.dll, Archipelago.MultiClient.Net.dll and Newtonsoft.Json.dll to Mod/Assemblies folder. Now you should be able to play.

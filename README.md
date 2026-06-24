# Library of Ruina Archipelago
Library of Ruina Archipelago (LORAP for short) is a Harmony mod for Library of Ruina to make it work with Archipelago.

[Archipelago](https://archipelago.gg) is a cross-game modification system which randomizes different games, then uses the result to build a single unified multi-player game. Items from one game may be present in another, and you will need your fellow players to find items you need in their games to help you complete your own.

## Installation
If you're entirely new to Archipelago and GitHub, consider using [this Guide](https://az-lastpenguin.github.io/guide).

Download the mod from [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3430626765).
The mod also requires [BaseMod](https://steamcommunity.com/workshop/filedetails/?id=2603522001) (Enable Priority Loader, please)

If you wish to install this mod manually, you have to download the mod from the release page and drop it into Library Of Ruina/LibraryOfRuina_Data/Mods folder. Basemod still required. Make sure to unsubscribe from the mod in workshop so it doesn't break.
I don't think BaseMod has a public github page, so you have to install it from the Workshop.

## About other mods support
The mod has **NO SUPPORT** for other **gameplay** mods. You can still install visual mods, most of which shouldn't break anything. But gameplay altering mods might and **WILL** break stuff.
Support for other mods is **NOT PLANNED**.

## Features
Planned and existing features, not in any particular order:
- [x] Removed Story (You can play LORAP without getting spoiled of the game's entire plot, but it is still recommended that you play vanilla game beforehand!)
- [X] Randomized reception tree
- [X] Randomized abnormality suppression order
- [X] Randomized book requirements
- [X] Randomized book pages
- [X] Randomized floor Abno/EGO Pages 
- [ ] Randomized combat/key pages
- [ ] Randomized enemies
- [ ] Filler/Trap items
- [ ] Pop Tracker (?)
- [ ] In-game Text (and more!) Client
- [ ] YAML Generator
- [ ] Extended player/AI capabilities (Acquiring enemies pages, enemies using abno pages and etc.)

## How do i..? / Can i..?
Most of the questions asked before (and some of those that were never asked) can be found in the [Questions & Answers](https://github.com/Az-LastPenguin/LORAP/blob/dev/QNA.md). Please make sure to look up your question in Q&A before asking it in the thread directly. (Though i won't hate you if you do! I will try to answer to every question regarding the mod whenever i'm available, as long as someone else doesn't answer before me, that is)

## Track progress
You can track progress of the mod, things that are completed, will be done, or still just an idea in [this project](https://github.com/users/Az-LastPenguin/projects/1/views/1?layout=board). (Note: this doesn't contain ALL the ideas and things i'm gonna do, most of them are minor enough to not be tracked in any way other than a TODO comment in the code)

## Share ideas
Visit [AfterDark Archipelago server](https://discord.gg/Sbhy4ykUKn) and [this channel](https://ptb.discord.com/channels/1085716850370957462/1137643745051955200) where me and the testers reside. You can discuss the mod, ideas for it, or anything relevant there. I'm always checking the chat. Another good way to let me know about your ideas is to [create an issue](https://github.com/Az-LastPenguin/LORAP/issues/new) with your idea, i will tag it appropriately.

You can also contact me on Discord directly: last_penguin. Please do mention how you found my tag and why you are messaging me. 

## Bug report
You can send screenshots and descriptions of bugs you found in the aforementioned discord channel. But i would **REALLY** appreciate it if you could [create an issue](https://github.com/Az-LastPenguin/LORAP/issues/new) for it, just so i can keep track of things more easily.

## Versioning
I'm using [SemVer 2.0.0](https://semver.org) as a guideline:
X.Y.Z-W where
- X is major version which is only incremented when something really big is added/changed which breaks literally everything. (Like entire mod rewrites. Likely will never happen after 1.0 release)
- Y is the minor version which is only incremented when something not that big is added/changed which can pontentially break something and require a new run. 
- X is the patch version which is incremented when really small fixes/updates are made.
- W is the testing version, denoted by a greek alphabet letter. Testing versions happen when i want to get feedback to things i'm doing but don't want to release an update to workshop just yet. Next letter is picked from the alphabet every time a testing version update is released.

## Compiling and Contributing
If you wish to try and build this beast (not in a good way) of a spaghetti monster:

0. Install Visual Studio Community. You'll also need the ".NET app development" workload installed.
1. Clone the repository
2. Open the project and reference every DLL from DLLs folder to the project (right click references > Add Reference > Browse)
3. Install NuGet Packages:
  - Archipelago.MultiClient.Net
  - Krafs.Publicizer (Should publicize Assembly-CSharp.dll on it's own, if not, follow [this tutorial from PMCH](https://imgur.com/a/FxTU1nD))
  - HarmonyX 2.9.0 (SPECIFICALLY 2.9.0, it's the same BaseMod uses, required for compatibility)
4. You can build it now. Click Build > Build Solution. After that, right click the project > Open Folder in File Explorer. Compiled mod should be in LORAP/bin/Release.
5. To play it, copy mod files from the cloned repo ("Mod" folder), to the game's "Mods" folder, then from compiled mod folder copy LORAP.dll, Archipelago.MultiClient.Net.dll and Newtonsoft.Json.dll to Mod/Assemblies folder. Now you should be able to play.

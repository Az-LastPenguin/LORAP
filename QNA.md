# Library of Ruina Archipelago (LORAP) Questions And Answers (Q&A)
## How do i install the mod?
You can follow [this Guide i made](https://az-lastpenguin.github.io/guide) that describes all steps for installing the stable version of the mod.

If you want a more thorough guide, you can use the [Official Archipelago Guide](https://archipelago.gg/tutorial/Archipelago/setup_en).

You can find stable .apworld and .yaml [here](https://github.com/Az-LastPenguin/LORAP/releases/latest).

## I want to play in-dev versions of the mod, how?
In-dev versions of the mod are called testing versions. You can find them on the [Releases page](https://github.com/Az-LastPenguin/LORAP/releases) of this github repository. (It's the Pre-release versions with a greek alphabet symbol after a dash)

Installing testing versions is same as installing stable version of the mod, except that you have to install the mod manually and install other version of the .apworld. Here's a small guide:

0. **Unsubscribe from the workshop version of the mod!**
1. Download the LORAP.7z archive of the version you want to play.
2. Unarchive it into LibraryOfRuina/LibraryOfRuina_Data/Mods folder. (Must be: Mods/LORAP/LORAP.dll)
3. Download the relevant .apworld file and do same things as with the stable version. (Setting up YAML, generating, hosting)

Playing testing versions is always appreciated. Make sure to report bugs and share ideas!

## I found a bug and want to report it / have something i want to suggest, how and where?
1. You can create an [create an issue](https://github.com/Az-LastPenguin/LORAP/issues/new). (Best option! Best value!)
2. You can visit the [AfterDark Archipelago server](https://discord.gg/Sbhy4ykUKn) and [this channel](https://ptb.discord.com/channels/1085716850370957462/1137643745051955200) specifically where the development is happening. (Good option)
3. You can contact me on discord: last_penguin. Please do mention how you found my tag and why you are messaging me. (Fine option)
4. You can leave a comment on the [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3430626765) page of the mod. (Worst option)

## I want to play a ruina randomizer without installing Archipelago, is it possible?
Sadly, not yet. But i do plan on making a standalone mod after i'm done with v1.0 of LORAP. Stay tuned.

## Why no mod support?
Adding a support for gameplay mods is going to be really really taxing on my free time and sanity. Modding a Project Moon coded game is hell on earth sometimes (half-jokingly).

Considering that most good mods for ruina go far beyond the base features of the game (enter Library of Babel or practically any big Chinese/Korean mod), i'd have to somehow incorporate their game changing mechanics automatically (or by hand) into LORAP, which isn't realistically possible.

Also due to the way Archipelago functions, i would have to make a tool to extract playable receptions from the mod to make .apworld create correct paths, and considering the fact tht every mod handles everything differently, this is also not really feasible. 

And i'm not ready to start adding mod support only to be hit with tens of requests to incorporate a certain mod into LORAP. Sorry.

If you are capable of modding Ruina and want to see a Ruina mod in Archipelago, feel free to fork the mod, it's open source for a reason.

## I want to compile the mod from source, how do i do it?
0. Install Visual Studio Community. You'll also need the ".NET app development" workload installed.
1. Clone the repository
2. Open the project and reference every DLL from DLLs folder to the project (right click references > Add Reference > Browse)
3. Install NuGet Packages:
  - Archipelago.MultiClient.Net
  - Krafs.Publicizer (Should publicize Assembly-CSharp.dll on it's own, if not, follow [this tutorial from PMCH](https://imgur.com/a/FxTU1nD))
  - HarmonyX 2.9.0 (SPECIFICALLY 2.9.0, it's the same BaseMod uses, required for compatibility)
4. You can build it now. Click Build > Build Solution. After that, right click the project > Open Folder in File Explorer. Compiled mod should be in LORAP/bin/Release.
5. To play it, copy mod files from the cloned repo ("Mod" folder), to the game's "Mods" folder, then from compiled mod folder copy LORAP.dll, Archipelago.MultiClient.Net.dll and Newtonsoft.Json.dll to Mod/Assemblies folder. Now you should be able to play.

## I want to contribute, what can i do for the mod?
You can test the mod! It's the simplest thing you can do and it's very very helpful!

If you want to contribute to the code, the best way to go about it is to ask me directly either in the discord thread or by DMing me in discord. I'll tell you want task you can take, will help you set up the dev enviroment and explain how to mod ruina if you don't already know (provided you know how to program, i won't teach you that), AND i will tell you the way i would try to do that task (That'll help you understand how to start working on it).

Overall, any task from the [project](https://github.com/users/Az-LastPenguin/projects/1/views/1?layout=board) is free to be taken (if i'm not already working on it).

# **IMPORTANT: MOST OF THE ANSWERS AFTER THIS TEXT ARE RELATED TO 1.0 IN-DEV VERSION OF THE MOD AND SOME FEATURES ARE NOT YET FINISHED**

## What is the difference to vanilla gameplay? (What is being randomized?)
- The way you acquire Combat/Key Pages is entirely different. You get them in a random manner from special books you get as items from other players (or yourself).
- The order of Receptions and the book requirements to access them are randomized.
- The order of Abnormality Suppressions/Floor Realizations, the floors they're in, and their requirements are randomized. Suppressions/Realizations are now found on the map along Receptions and only require books to access.
- Stats of the Abnormality Pages (Floor, emotion level, emotion state, and rate) are randomized.
- You can get more than 5 Emotion levels (provided you received enough Emotion Limit Break items). Max light and Speed die count are scaled accordingly.
- When getting an Abnormality page, instead of getting a page of a certain level (I for Emotion levels 1-2, II for 3-4, III for 5), you can get Abnormality pages of lower levels (I for Emotion levels 1-2, I and II for 3-4, I, II and III for level 5+). **[UNFINISHED FEATURE]**
- The way Passive Attribution works is changed: instead of having maximum of 8 passives, you stack passives on top of the key page's passives (You get 1 additional slot of every 1 Passive Limit Break item); Maximum Passive Attribution point number depends on the Passive Attribution Points item (You get 2 points for every item); You can attribute up to 16 different key pages to a single key page.

## What are the goals?
- 1-10 Receptions of Reverberation Ensemble (Configurable)
- Black Silence Reception
- Keter Realization
- Distorted Ensemble

## What are the Archipelago items?
- Every book in the game (They function as "keys" to battles)
- Special book items that give you Combat/Key Pages ("Book of Everything" & "Booster Pack"s)
- Floors
- Abnormality Pages
- EGO Pages
- Librarians
- Map Passive Attribution Points & Max passives
- Max Emotion Level

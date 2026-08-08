# Library of Ruina Archipelago (LORAP) Troubleshooting
## First and foremost!
**If you're having trouble INSTALLING the mod, please make sure you followed the guides in Q&A to the T before reading this!**

**Make sure the versions of .apworld and the mod are matching!**

**If you still have trouble with something, don't hesitate to ask for help in the AP AfterDark server!**

Some .apworld/mod versions can be mismatched by design (e.g. when .apworld/mod is updated without any changes on the other one) and every mod version starting from in-dev 1.0 has and will specify which .apworld version it works with.

## When starting the game with 1.0 in-dev version of the mod, the game doesn't load after selecting mods without any errors!
That's probably a bug i still haven't fixed due to always forgetting about it, ignore and try again, it has a really small chance of happening.

## When trying to connect i receive an error "The given key was not present in the dictionary" / "Option is missing from SlotData" / "battle node is missing visual coordinates"!
There is 99% chance that your mod version is not matching the server's .apworld version. Make sure to follow the installation guide correctly, or if you're not the host, that the host has used the correct .apworld version.

## When trying to connect i receive a really long error message about some "async datastore"!
This is a rare error when the game cannot reliably download the save file from the server. Try reopening the game/reconnecting again, it might go away. Will be fixed in 1.0 release.

## When trying to connect i receive an error "Connection timed out"!
It's either:
1. The server is not up
2. Your internet connection is unstable
3. Your AP game was generated with Hitman as one of the games. It's a really obscure bug that will hopefully be fixed in the next update. For the solution, check [this message in the AD server](https://discord.com/channels/1085716850370957462/1137643745051955200/1535266259300515920)

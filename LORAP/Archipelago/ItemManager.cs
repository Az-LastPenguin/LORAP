using System.Collections;
using Archipelago.MultiClient.Net.Models;
using GameSave;
using LORAP.Playthru;
using LORAP.Utils;
using UnityEngine;

namespace LORAP.Archipelago
{
    internal enum APItemType
    {
        FloorUnlock,
        AbnoPages,
        EgoPage,
        Librarian,
        Book,
        Other,
    }

    internal enum OtherItem
    {
        PassivePoints,
        PassiveLimitBreak,
        EmotionLimitBreak,
        Binah,
        BlackSilence
    }

    internal class LORItemInfo
    {
        public APItemType ItemType;

        public long Id;

        public long RealId;
    }

    internal static class ItemManager
    {
        private static Coroutine _itemQueueCoroutine;

        internal static bool Suspended = false;

        internal static int ItemsReceived = 0; // Total items received this session. Used to silently receive (restore) items when loading the game
        internal static int TotalItemsReceived = 0; // Total items received this run

        private static IEnumerator PopItemQueue()
        {
            while (true)
            {
                if (!Suspended && SessionManager.Items.Any())
                {
                    LORItemInfo item = ConvertItemInfo(SessionManager.Items.DequeueItem());

                    bool newItems = false;

                    if (ItemsReceived < TotalItemsReceived) // Means this item was received before, so we silently receive (restore) it
                    {
                        if (item.ItemType != APItemType.Book) // Unless it's a book. We don't want copies, that's cheating
                            ParseAndGiveItem(item, true);

                        yield return new WaitForEndOfFrame(); //WaitForSeconds(0.01f);
                    }
                    else // Means this is a new item, receive it as it should be received
                    {
                        ParseAndGiveItem(item);

                        newItems = true;
                        TotalItemsReceived++;
                        yield return new WaitForEndOfFrame(); //WaitForSeconds(0.1f);
                    }

                    ItemsReceived++;

                    if (!SessionManager.Items.Any() && newItems)
                        Gameplay.SaveManager.SaveGame();

                    continue;
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        internal static void Init()
        {
            Debug.Log("[LORAP] Initializing AP Item Manager");

            ItemsReceived = 0;
            TotalItemsReceived = 0;
        }

        internal static void Start()
        {
            if (_itemQueueCoroutine == null)
                _itemQueueCoroutine = Timing.Coroutine(PopItemQueue());
            else
                Suspended = false;
        }

        internal static long GetItemId(int id, APItemType type)
        {
            return ((int)type) << 28 | id;
        }

        internal static long GetOtherItemId(OtherItem type)
        {
            return ((int)APItemType.Other) << 28 | (int)type;
        }

        internal static LORItemInfo ConvertItemInfo(ItemInfo item)
        {
            return new LORItemInfo() { ItemType = (APItemType)(item.ItemId >> 28), Id = item.ItemId, RealId = (item.ItemId & 0x0FFFFFFF) }; 
        }

        internal static void ParseAndGiveItem(LORItemInfo item, bool silent = false)
        {
            Debug.Log($"[LORAP] Receiving Item {item.Id} ({item.ItemType}:{item.RealId})");

            switch (item.ItemType)
            {
                case APItemType.FloorUnlock:
                    PlaythruManager.OpenFloor((SephirahType)item.RealId, silent);
                    break;
                case APItemType.AbnoPages:
                    PlaythruManager.GiveAbnoPages((SephirahType)item.RealId, silent);
                    break;
                case APItemType.EgoPage:
                    PlaythruManager.GiveEGOPage((SephirahType)item.RealId, silent);
                    break;
                case APItemType.Librarian:
                    PlaythruManager.GiveLibrarian((SephirahType)item.RealId, silent);
                    break;
                case APItemType.Book:
                    PlaythruManager.GiveBook((int)item.RealId, 1, silent);
                    break;
                case APItemType.Other:
                    // Yay switch case of doom!!
                    switch (item.RealId)
                    {
                        case 0:
                            PlaythruManager.UpMaxAttributionPoints(silent);
                            break;
                        case 1:
                            PlaythruManager.UpMaxPassives(silent);
                            break;
                        case 2:
                            PlaythruManager.UpMaxEmotion(silent);
                            break;
                        case 3:
                            PlaythruManager.UnlockBinah(silent);
                            break;
                        case 4:
                            PlaythruManager.UnlockBlackSilence(silent);
                            break;
                    }
                    break;
            }
        }


        // Saving/loading game state
        internal static SaveData GetSaveData()
        {
            SaveData saveData = new SaveData();

            // Save completed receptions
            saveData.AddData("itemsReceived", new SaveData(TotalItemsReceived));

            // Some other data maybe

            return saveData;
        }

        internal static void LoadFromSaveData(SaveData saveData)
        {
            TotalItemsReceived = saveData.GetInt("itemsReceived");
        }
    }
}

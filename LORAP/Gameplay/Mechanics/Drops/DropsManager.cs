using System;
using System.Collections.Generic;
using GameSave;
using LORAP.Archipelago;
using LORAP.Gameplay.Systems.Drops;
using LORAP.Playthru;

namespace LORAP.Gameplay.Mechanics.Drops
{
    internal static class DropsManager
    {
        internal static Dictionary<DropSystem, BaseDropSystem> Drops = new Dictionary<DropSystem, BaseDropSystem>()
        {
            [DropSystem.BookOfEverything] = new BookOfEverything(),
            [DropSystem.BookOfEverythingBalanced] = new BookOfEverythingBalanced(),
        };

        private static DropSystem currentType = DropSystem.BookOfEverything;

        private static BaseDropSystem currentSystem => Drops[currentType];

        internal static void Setup(SlotDataStruct SlotData, int Seed)
        {
            currentType = SlotData.DropSystem;
            currentSystem.Setup(SlotData, Seed);
        }

        internal static List<BookDropResult> GenerateDrops(LorId id)
        {
            DropBookInventoryModel.Instance.RemoveBook(id);

            if (id.packageId != "lorap")
            {
                CheckManager.BookCheck(id.id); // Try to give check for that book

                return new List<BookDropResult>();
            }

            return currentSystem.GenerateDrops(id);
        }

        internal static void LoadSaveData(SaveData saveData) => currentSystem.loadSaveData(saveData);

        internal static SaveData GetSaveData() => currentSystem.getSaveData();
    }
}

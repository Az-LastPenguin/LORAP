using System.Collections.Generic;
using GameSave;
using LORAP.Playthru;

namespace LORAP.Gameplay.Systems.Drops
{
    internal abstract class BaseDropSystem
    {
        internal Dictionary<int, DropBookXmlInfo> CustomBooks { get; set; }

        internal abstract void Setup(SlotDataStruct SlotData, int Seed);

        internal abstract SaveData getSaveData();

        internal abstract void loadSaveData(SaveData saveData);

        public abstract List<BookDropResult> GenerateDrops(LorId id);
    }
}

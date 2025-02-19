using System.Collections.Generic;
using System.Linq;
using GameSave;
using LORAP.Playthru;
using UnityEngine;

namespace LORAP.Gameplay.Systems.Drops
{
    internal class BookOfEverything : BaseDropSystem
    {
        private int BookId = 123456;

        private int BooksBurned = 0;

        internal override void Setup(SlotDataStruct SlotData, int Seed)
        {
            
        }

        internal override SaveData getSaveData()
        {
            SaveData data = new SaveData();
            data.AddData("booksBurned", new SaveData(BooksBurned));

            return data;
        }

        internal override void loadSaveData(SaveData saveData)
        {
            if (saveData.GetData("booksBurned") != null)
                BooksBurned = saveData.GetInt("booksBurned");
        }

        public override List<BookDropResult> GenerateDrops(LorId id)
        {
            BooksBurned++;

            List<BookDropResult> Drops = new List<BookDropResult>();

            var Random = new System.Random(PlaythruManager.Seed / 2 + BooksBurned); // Did what first came to mind lol
            
            var KeyPages = ContentManager.CustomBooks[BookId].DropItemList.Where(d => d.itemType == DropItemType.Equip).ToList();
            var CombatPages = ContentManager.CustomBooks[BookId].DropItemList.Where(d => d.itemType == DropItemType.Card).ToList();

            for (int i = 0; i < 16; i++)
            {
                var KeyPage = Random.Next(0, 100) >= 80;
                var Pool = KeyPage ? KeyPages.Where(d => BookInventoryModel.Instance.GetBookCount(d.id) < (BookXmlList.Instance.GetData(d.id).Rarity == Rarity.Unique ? 1 : 5 - (int)BookXmlList.Instance.GetData(d.id).Rarity)) : CombatPages;

                var Selected = Pool.ElementAt(Random.Next(Pool.Count()));
                BookDropResult Drop = new BookDropResult();
                Drop.id = Selected.id;
                Drop.itemType = Selected.itemType;
                Drop.number = 1;

                if (KeyPage)
                    Drop.bookInstanceId = BookInventoryModel.Instance.CreateBook(Selected.id).instanceId;
                else
                    InventoryModel.Instance.AddCard(Selected.id);

                Drops.Add(Drop);
            }

            // FOR BALANCED VERSION OF THE BOOK
            // For each 8 items:
            // Decide if it's a keypage or just a page
            // If page:
            // Generate number from min-power to max-power but distribution is closer to burned books% of it
            // Binary Search to find the closest item
            //
            // If keypage:
            // Somethin else

            return Drops;
        }
    }
}

using GameSave;
using LORAP.Archipelago;
using LORAP.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LORAP.Gameplay
{
    internal static class BookDropManager
    {
        internal struct BookDrop
        {
            public LorId id;
            public int chapter;
            public DropItemType type;
            public bool collectible;
        }

        internal static Dictionary<Rarity, int> KeyPageRarityLimits = new Dictionary<Rarity, int>()
        {
            [Rarity.Common] = 15,
            [Rarity.Uncommon] = 12,
            [Rarity.Rare] = 6,
            [Rarity.Unique] = 1,
            [Rarity.Special] = 1
        };

        internal static List<BookDrop> AllDrops = new List<BookDrop>();

        internal static Dictionary<LorId, List<BookDrop>> BookDrops = new Dictionary<LorId, List<BookDrop>>();

        internal static int BoosterPacksOpened = 0;
        internal static Dictionary<Rarity, List<BookDrop>> BoosterPackRarityDrops = new Dictionary<Rarity, List<BookDrop>>()
        {
            [Rarity.Common] = new List<BookDrop>(),
            [Rarity.Uncommon] = new List<BookDrop>(),
            [Rarity.Rare] = new List<BookDrop>(),
            [Rarity.Unique] = new List<BookDrop>(),
        };
        internal static Dictionary<Rarity, float> BoosterPackRarityWeights = new Dictionary<Rarity, float>() 
        {
            //[Rarity.Common] = 16f / 30,
            [Rarity.Uncommon] = 15f / 30,
            [Rarity.Rare] = 7f / 30,
            [Rarity.Unique] = 2f / 30,
        };
 
        internal static void Init()
        {
            Debug.Log("[LORAP] Initializing Book Content Manager");

            AllDrops.Clear();

            // Parse every drop table to get every combat page and key ppage that can be acquired from books
            List<LorId> collectiblePages = new List<LorId>();

            foreach (var info in DropBookXmlList.Instance._list)
            {
                foreach (var page in info.DropItemList)
                {
                    if (!collectiblePages.Contains(page.id))
                        collectiblePages.Add(page.id);
                }
            }

            // Add combat pages that are acquired after receptions & game end
            collectiblePages.AddRange(new List<LorId>()
            {
                new LorId(704008), new LorId(704003), new LorId(704018), new LorId(704005), new LorId(704016), new LorId(704006), new LorId(704015), new LorId(704007), new LorId(705032), new LorId(705033),
                new LorId(704004), new LorId(704011), new LorId(704012), new LorId(704013), new LorId(704014), new LorId(704001), new LorId(704009), new LorId(704010), new LorId(705002), new LorId(705003),
                new LorId(705004), new LorId(705010), new LorId(705011), new LorId(705013), new LorId(705014), new LorId(705015), new LorId(705016), new LorId(705017), new LorId(705018), new LorId(705019),
                new LorId(705020), new LorId(705021), new LorId(705031),
            });

            // Parse every combat page and save it as a drop
            foreach (var card in ItemXmlDataList.instance._cardInfoList)
            {
                AllDrops.Add(new BookDrop
                {
                    id = card.id,
                    chapter = card.Chapter,
                    type = DropItemType.Card,
                    collectible = collectiblePages.Contains(card.id),
                });
            }

            // Parse every key page and save it as a drop
            foreach (var book in BookXmlList.Instance._list)
            {
                AllDrops.Add(new BookDrop
                {
                    id = book.id,
                    chapter = book.Chapter,
                    type = DropItemType.Equip,
                    collectible = collectiblePages.Contains(book.id),
                });
            }

            // Figure out drops & weights for booster packs
            foreach (var drop in AllDrops.Where(d => d.collectible))
            {
                Rarity dropRarity = drop.type == DropItemType.Card ? ItemXmlDataList.instance.GetCardItem(drop.id).Rarity : BookXmlList.Instance.GetData(drop.id).Rarity;

                BoosterPackRarityDrops[dropRarity].Add(drop);
            }

            
            // Figure out drops for vanilla books
            // Decide of what chapter each book will be
            Dictionary<LorId, int> bookChapters = new Dictionary<LorId, int>();

            // Reception Requirements 
            if (SlotDataManager.ReceptionsRequireBooks)
            {
                foreach (var pair in SlotDataManager.ReceptionBookRequirements)
                {
                    StageClassInfo info = StageClassInfoList.Instance.GetData(pair.Key);

                    foreach (var id in pair.Value)
                    {
                        if (SlotDataManager.BookContentsRandomization == BookContentsRandomization.BookChapter)
                            bookChapters[new LorId(id)] = DropBookXmlList.Instance.GetData(id).chapter;
                        else
                            bookChapters[new LorId(id)] = info.chapter;
                    }
                }
            }

            // Floor Requirements
            if (SlotDataManager.FloorsRequireBooks)
            {
                foreach (var item in SlotDataManager.AbnoBookRequirements)
                {
                    var seph = item.Key;
                    var stageOrder = SlotDataManager.AbnoFightOrder[seph];

                    for (int i = 0; i < item.Value.Count; i++)
                    {
                        int stageId = stageOrder[i];
                        int logicalChapter = 1;

                        if (SlotDataManager.AbnoStageChapters != null &&
                            SlotDataManager.AbnoStageChapters.TryGetValue(stageId, out int parsedChapter))
                        {
                            logicalChapter = parsedChapter;
                        }

                        foreach (var book in item.Value[i])
                        {
                            if (SlotDataManager.BookContentsRandomization == BookContentsRandomization.BookChapter)
                                bookChapters[new LorId(book)] = DropBookXmlList.Instance.GetData(book).chapter;
                            else
                                bookChapters[new LorId(book)] = logicalChapter;
                        }
                    }
                }
            }

            var Random = new System.Random(SlotDataManager.Seed);
            List<BookDrop> allowedDrops = AllDrops.Where(d => d.collectible).ToList();

            if (SlotDataManager.BookContentsRandomization == BookContentsRandomization.Chaotic)
            {
                List<LorId> books = bookChapters.Select(p => p.Key).ToList();
                int dropsPerBook = allowedDrops.Count / books.Count;
                int diff = allowedDrops.Count - (dropsPerBook * books.Count);

                foreach (LorId book in books)
                {
                    List<BookDrop> drops = new List<BookDrop>();

                    // Get drops for the book. Also adds diff to the last book
                    for (var i = 0; i < dropsPerBook + (allowedDrops.Count < dropsPerBook ? diff : 0); i++)
                    {
                        // Failsafe?
                        if (allowedDrops.Count == 0)
                            break;

                        BookDrop drop = allowedDrops.PopRandom(Random);
                        drops.Add(drop);
                    }

                    BookDrops[book] = drops;
                }
            }
            else
            {
                // Assign drops from that chapter
                foreach (int chapter in bookChapters.Values.Distinct())
                {
                    List<BookDrop> chapterDrops = allowedDrops.Where(d => d.chapter == chapter).ToList();
                    List<LorId> chapterBooks = bookChapters.Where(p => p.Value == chapter).Select(p => p.Key).ToList();
                    int dropsPerBook = chapterDrops.Count / chapterBooks.Count;
                    int diff = chapterDrops.Count - (dropsPerBook * chapterBooks.Count);

                    foreach (LorId book in chapterBooks)
                    {
                        List<BookDrop> drops = new List<BookDrop>();

                        // Get drops for the book. Also adds diff to the last book
                        for (var i = 0; i < dropsPerBook + (chapterDrops.Count < dropsPerBook ? diff : 0); i++)
                        {
                            // Failsafe?
                            if (chapterDrops.Count == 0)
                                break;

                            BookDrop drop = chapterDrops.PopRandom(Random);
                            allowedDrops.Remove(drop);
                            drops.Add(drop);
                        }

                        BookDrops[book] = drops;
                    }
                }
            }
        }

        internal static List<BookDropResult> GenerateDrops(LorId bookID)
        {
            List<BookDropResult> dropResults = new List<BookDropResult>();

            // Generate Drops
            if (bookID == new LorId("lorap", 123456)) // TODO: Make it. Also bias utility combat pages
            {

            }
            else if (bookID == new LorId("lorap", 123457))
            {
                for (int i = 0; i < 8; i++)
                {
                    var Random = new System.Random(SlotDataManager.Seed + BoosterPacksOpened * 8 + i);

                    float rng = (float)Random.NextDouble();
                    Rarity dropRarity = Rarity.Common;
                    foreach (var pair in BoosterPackRarityWeights)
                    {
                        if (rng <= pair.Value)
                        {
                            dropRarity = pair.Key;
                        }
                    }

                    List<BookDrop> drops = BoosterPackRarityDrops[dropRarity].Where(
                                                                                    d => d.type == DropItemType.Card
                                                                                    ? InventoryModel.Instance.GetCardCount(d.id) < 150
                                                                                    : BookInventoryModel.Instance.GetBookCount(d.id) < KeyPageRarityLimits[dropRarity]
                                                                                   ).ToList();

                    BookDrop selectedDrop = drops[Random.Next(0, drops.Count)];

                    BookDropResult dropResult = new BookDropResult()
                    {
                        id = selectedDrop.id,
                        itemType = selectedDrop.type,
                        number = 1,
                    };

                    if (selectedDrop.type == DropItemType.Card)
                    {
                        InventoryModel.Instance.AddCard(selectedDrop.id, 1);
                    }
                    else
                    {
                        dropResult.bookInstanceId = BookInventoryModel.Instance.CreateBook(selectedDrop.id).instanceId;
                    }

                    dropResults.Add(dropResult);
                }

                BoosterPacksOpened++;

                DropBookInventoryModel.Instance.RemoveBook(bookID);
            }
            else
            {
                // Vanilla books
                List<BookDrop> pool = BookDrops[bookID];
                foreach (BookDrop drop in pool)
                {
                    BookDropResult dropResult = new BookDropResult()
                    {
                        id = drop.id,
                        itemType = drop.type,
                        number = drop.type == DropItemType.Card ? 3 : 1,
                    };

                    if (drop.type == DropItemType.Card && InventoryModel.Instance.GetCardCount(drop.id) < 150)
                    {
                        InventoryModel.Instance.AddCard(drop.id, 3);
                    }
                    else  //if (drop.type == DropItemType.Equip)
                    {
                        // If we can have more of those create a keypage and store the instanceid to show in the result screen
                        if (BookInventoryModel.Instance.GetBookCount(drop.id) < KeyPageRarityLimits[BookXmlList.Instance.GetData(drop.id).Rarity])
                            dropResult.bookInstanceId = BookInventoryModel.Instance.CreateBook(drop.id).instanceId;
                        else
                            continue; // Can't give, skip
                    }
                    //else // Don't add result into the table if we didn't give player anything // Why was this check even here in the first place?
                    //{
                    //    continue;
                    //}

                    dropResults.Add(dropResult);
                }
            }

            return dropResults;
        }

        // Saving/loading game state
        internal static SaveData GetSaveData()
        {
            SaveData saveData = new SaveData();

            // Save how many booster packs opened
            saveData.AddData("boosterPacksOpened", new SaveData(BoosterPacksOpened));

            return saveData;
        }

        internal static void LoadFromSaveData(SaveData saveData)
        {
            BoosterPacksOpened = saveData.GetInt("boosterPacksOpened");
        }
    }
}

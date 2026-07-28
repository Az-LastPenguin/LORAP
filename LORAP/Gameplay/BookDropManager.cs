using GameSave;
using LORAP.Archipelago;
using LORAP.Playthru;
using LORAP.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LOR_DiceSystem;
using System;

namespace LORAP.Gameplay
{
    internal static class BookDropManager
    {
        internal struct BookDrop
        {
            public LorId id;
            public int chapter;
            public DropItemType type;

            public override string ToString()
            {
                // Using string interpolation for a clean, readable output
                return $"[{chapter}] {id} ({type})";
            }
        }

        internal static readonly Dictionary<Rarity, int> KeyPageRarityLimits = new Dictionary<Rarity, int>()
        {
            [Rarity.Common] = 15,
            [Rarity.Uncommon] = 12,
            [Rarity.Rare] = 6,
            [Rarity.Unique] = 1,
            [Rarity.Special] = 1
        };

        internal const int CombatPageMaxCopies = 150;

        //internal static List<BookDrop> AllDrops = new List<BookDrop>();

        // internal static Dictionary<LorId, List<BookDrop>> BookDrops = new Dictionary<LorId, List<BookDrop>>();
        internal static List<List<BookDrop>> ChapterDrops = new List<List<BookDrop>>();
        internal static List<List<BookDrop>> Bundles = new List<List<BookDrop>>();

        internal static int BooksOfEverythingOpened = 0;
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
 
        internal static void PrepareDrops()
        {
            Debug.Log("[LORAP] Initializing Book Content Manager");

            // BookDrops.Clear();
            Bundles.Clear();
            foreach (var rarityDrops in BoosterPackRarityDrops.Values)
                rarityDrops.Clear();

            // Get all possible drops
            List<BookDrop> allDrops = GatherPages();

            // Prepare Booster Pack Drops
            foreach (BookDrop drop in allDrops)
            {
                Rarity dropRarity = drop.type == DropItemType.Card ? ItemXmlDataList.instance.GetCardItem(drop.id).Rarity : BookXmlList.Instance.GetData(drop.id).Rarity;

                BoosterPackRarityDrops[dropRarity].Add(drop);
            }

            // Prepare Book of Everything Drops
            // 0. Check if the emount of bundles was received correctly
            int bundleCount = SlotDataManager.BoEBundlesPerSphere?.Sum() ?? 0;
            if (bundleCount <= 0)
                throw new Exception("Book of Everything Bundle count was received incorrectly! Possible mod and .apworld version mismatch?");

            // 1. Get all separate chapter page lists & sort them by their ID (their appearance in vanilla pretty much)
            List<List<BookDrop>> dropsByChapter = allDrops.GroupBy(d => d.chapter).Select(g => g.OrderBy(d => d.id).ToList()).ToList();

            // Pages are now sorted by id in ascending order, meaning pages with biggest id appear last in chapter
            // and pages with smallest id appear first in chapter, so...
            // 2. Mix random amount of early and late pages between adjacent chapters for a change
            System.Random rng = GameUtils.CreateRandom("bundle_chapter_mixing");
            for (int i = 0; i < dropsByChapter.Count; i++)
            {
                if (i + 1 >= dropsByChapter.Count)
                    break;

                List<BookDrop> curChapter = dropsByChapter[i];
                List<BookDrop> nextChapter = dropsByChapter[i + 1];

                // We will mix 0-25% of the chapter with least pages between the chapters
                int mixCount = rng.Next(0, Math.Min(curChapter.Count, nextChapter.Count) / 4);

                // And now, we mix
                List<BookDrop> curPart = curChapter.GetRange(curChapter.Count - mixCount, mixCount);
                curChapter.RemoveRange(curChapter.Count - mixCount, mixCount);

                List<BookDrop> nextPart = nextChapter.GetRange(0, mixCount);
                nextChapter.RemoveRange(0, mixCount);

                curChapter.AddRange(nextPart);
                nextChapter.InsertRange(0, curPart);
            }

            // 3. Split every chapter in two, shuffle the halves and combine them
            foreach (List<BookDrop> chapter in dropsByChapter)
            {
                int halfCount = chapter.Count / 2;
            
                List<BookDrop> secondHalf = chapter.GetRange(chapter.Count - halfCount, halfCount);
                chapter.RemoveRange(chapter.Count - halfCount, halfCount);
            
                chapter.Shuffle(rng);
                secondHalf.Shuffle(rng);
                
                chapter.AddRange(secondHalf);
            }

            ChapterDrops = dropsByChapter;

            // 4. Split each chapter's drops into equal parts the amount of bundles in a chapter
            for (int i = 0; i < SlotDataManager.BoEBundlesPerSphere.Count; i++)
            {
                int bundlesNum = SlotDataManager.BoEBundlesPerSphere[i];
                List<BookDrop> chapter = dropsByChapter[i];
            
                int dropsPerBundle = chapter.Count / bundlesNum;
            
                for (int j = 0; j < bundlesNum; j++)
                {
                    Bundles.Add(chapter.Take(dropsPerBundle).ToList());
                    chapter.RemoveRange(0, Math.Min(dropsPerBundle, chapter.Count));
                }

                // If there is still some drops left, just assign them to the last bundle of the chapter
                if (chapter.Count > 0)
                    Bundles.Last().AddRange(chapter);
            }

            // TODO: TO BE REMOVED
            /*
            // Figure out drops for vanilla books
            // Decide of what chapter each book will be
            Dictionary<LorId, int> bookChapters = new Dictionary<LorId, int>();
            
            // Reception Requirements 
            foreach (var pair in SlotDataManager.ReceptionBookRequirements)
            {
                StageClassInfo info = StageClassInfoList.Instance.GetData(pair.Key);
            
                foreach (var id in pair.Value)
                {
                    if (SettingsManager.BookContentsRandomization == BookContentsRandomization.BookChapter)
                        bookChapters[new LorId(id)] = DropBookXmlList.Instance.GetData(id).chapter;
                    else
                        bookChapters[new LorId(id)] = info.chapter;
                }
            }
            
            // Floor Requirements
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
                        if (SettingsManager.BookContentsRandomization == BookContentsRandomization.BookChapter)
                            bookChapters[new LorId(book)] = DropBookXmlList.Instance.GetData(book).chapter;
                        else
                            bookChapters[new LorId(book)] = logicalChapter;
                    }
                }
            }

            var Random = GameUtils.CreateRandom("book_drops");
            List<BookDrop> allowedDrops = AllDrops.Where(d => d.collectible).ToList();

            if (bookChapters.Count == 0)
            {
                Debug.LogWarning("[LORAP] No book requirements found; skipping vanilla book drop randomization.");
                return;
            }

            if (SettingsManager.BookContentsRandomization == BookContentsRandomization.Chaotic)
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

                        BookDrop drop = allowedDrops.TakeRandom(Random);
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

                            BookDrop drop = chapterDrops.TakeRandom(Random);
                            allowedDrops.Remove(drop);
                            drops.Add(drop);
                        }

                        BookDrops[book] = drops;
                    }
                }
            }*/
        }

        private static List<BookDrop> GatherPages()
        {
            // Parse every drop table to get every combat page and key page that can be acquired from books
            HashSet<LorId> collectibleCombatPages = new HashSet<LorId>();
            HashSet<LorId> collectibleKeyPages = new HashSet<LorId>();

            foreach (var info in DropBookXmlList.Instance._list)
            {
                if (info.id.packageId != "")
                    continue;

                foreach (var page in info.DropItemList)
                {
                    if (page.itemType == DropItemType.Card)
                        collectibleCombatPages.Add(page.id);
                    else if (page.itemType == DropItemType.Equip)
                        collectibleKeyPages.Add(page.id);
                }
            }

            // Add combat pages that are acquired after receptions & game end
            collectibleCombatPages.UnionWith(new List<LorId>()
            {
                new LorId(704008), new LorId(704003), new LorId(704018), new LorId(704005), new LorId(704016), new LorId(704006), new LorId(704015), new LorId(704007), new LorId(705032), new LorId(705033),
                new LorId(704004), new LorId(704011), new LorId(704012), new LorId(704013), new LorId(704014), new LorId(704001), new LorId(704009), new LorId(704010), new LorId(705002), new LorId(705003),
                new LorId(705004), new LorId(705010), new LorId(705011), new LorId(705013), new LorId(705014), new LorId(705015), new LorId(705016), new LorId(705017), new LorId(705018), new LorId(705019),
                new LorId(705020), new LorId(705021), new LorId(705031),
            });

            collectibleKeyPages.UnionWith(new List<LorId>()
            {
                new LorId(260005), new LorId(260006), new LorId(260007), new LorId(260008), new LorId(260009), new LorId(260010), new LorId(260011), new LorId(260012), new LorId(260013), new LorId(260014),
            });

            // Get info about every key/combat page that can be a drop
            List<BookDrop> allDrops = new List<BookDrop>();

            // Combat Pages
            foreach (LorId pageId in collectibleCombatPages)
            {
                DiceCardXmlInfo page = ItemXmlDataList.instance._cardInfoTable[pageId];

                allDrops.Add(new BookDrop
                {
                    id = page.id,
                    chapter = page.Chapter,
                    type = DropItemType.Card,
                });
            }

            // Key Pages
            foreach (LorId pageId in collectibleKeyPages)
            {
                BookXmlInfo page = BookXmlList.Instance._dictionary[pageId];

                allDrops.Add(new BookDrop
                {
                    id = page.id,
                    chapter = page.Chapter,
                    type = DropItemType.Equip,
                });
            }

            return allDrops;
        }

        internal static List<BookDropResult> GenerateDrops(LorId bookID)
        {
            List<BookDropResult> dropResults = new List<BookDropResult>();

            // Generate Drops
            if (bookID == new LorId("lorap", 123456))
            {
                if (!PlaythruManager.CanOpenBookOfEverything())
                {
                    return dropResults;
                }

                foreach (BookDrop drop in Bundles[BooksOfEverythingOpened])
                    dropResults.AddRange(GrantDropToMaxStack(drop));

                BooksOfEverythingOpened++;

                DropBookInventoryModel.Instance.RemoveBook(bookID);
            }
            else if (bookID == new LorId("lorap", 123457))
            {
                for (int i = 0; i < 8; i++)
                {
                    var Random = GameUtils.CreateRandom("booster_packs", BoosterPacksOpened * 8 + i);

                    float rng = (float)Random.NextDouble();
                    Rarity dropRarity = Rarity.Common;
                    foreach (var pair in BoosterPackRarityWeights)
                    {
                        if (rng <= pair.Value)
                        {
                            dropRarity = pair.Key;
                        }
                    }

                    List<BookDrop> drops = BoosterPackRarityDrops[dropRarity].Where(CanReceiveDrop).ToList();

                    BookDrop selectedDrop = drops[Random.Next(0, drops.Count)];

                    dropResults.AddRange(GrantDrop(selectedDrop, 1));
                }

                BoosterPacksOpened++;

                DropBookInventoryModel.Instance.RemoveBook(bookID);
            }

            // TODO: TO BE REMOVED
            /*
            else
            {
                
               
                // Vanilla books
                if (!BookDrops.TryGetValue(bookID, out List<BookDrop> pool))
                    return dropResults;

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
            */

            return dropResults;
        }

        internal static List<BookDropResult> GrantDropToMaxStack(BookDrop drop)
        {
            int missingCopies = GetMaxCopies(drop) - GetOwnedCopies(drop);
            return GrantDrop(drop, missingCopies);
        }

        private static List<BookDropResult> GrantDrop(BookDrop drop, int copies)
        {
            List<BookDropResult> dropResults = new List<BookDropResult>();
            int copiesToGrant = Math.Min(copies, GetMaxCopies(drop) - GetOwnedCopies(drop));

            if (copiesToGrant <= 0)
                return dropResults;

            if (drop.type == DropItemType.Card)
            {
                InventoryModel.Instance.AddCard(drop.id, copiesToGrant);
                dropResults.Add(new BookDropResult()
                {
                    id = drop.id,
                    itemType = drop.type,
                    number = copiesToGrant,
                });
            }
            else
            {
                for (int i = 0; i < copiesToGrant; i++)
                {
                    BookDropResult dropResult = new BookDropResult()
                    {
                        id = drop.id,
                        itemType = drop.type,
                        number = 1,
                        bookInstanceId = BookInventoryModel.Instance.CreateBook(drop.id).instanceId,
                    };

                    dropResults.Add(dropResult);
                }
            }

            return dropResults;
        }

        private static bool CanReceiveDrop(BookDrop drop)
        {
            return GetOwnedCopies(drop) < GetMaxCopies(drop);
        }

        private static int GetOwnedCopies(BookDrop drop)
        {
            return drop.type == DropItemType.Card
                ? InventoryModel.Instance.GetCardCount(drop.id)
                : BookInventoryModel.Instance.GetBookCount(drop.id);
        }

        private static int GetMaxCopies(BookDrop drop)
        {
            return drop.type == DropItemType.Card
                ? CombatPageMaxCopies
                : KeyPageRarityLimits[BookXmlList.Instance.GetData(drop.id).Rarity];
        }



        // Saving/loading game state
        internal static SaveData GetSaveData()
        {
            SaveData saveData = new SaveData();

            // Save how many booster packs opened
            saveData.AddData("booksOfEverythingOpened", new SaveData(BooksOfEverythingOpened));
            saveData.AddData("boosterPacksOpened", new SaveData(BoosterPacksOpened));

            return saveData;
        }

        internal static void LoadFromSaveData(SaveData saveData)
        {
            BooksOfEverythingOpened = saveData.GetInt("booksOfEverythingOpened");
            BoosterPacksOpened = saveData.GetInt("boosterPacksOpened");
        }
    }
}

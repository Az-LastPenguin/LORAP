using GameSave;
using LORAP.CustomUI;
using LORAP.Archipelago;
using LORAP.Playthru;
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

        internal const int CombatPageMaxCopies = 150;

        internal static List<BookDrop> AllDrops = new List<BookDrop>();

        internal static Dictionary<LorId, List<BookDrop>> BookDrops = new Dictionary<LorId, List<BookDrop>>();
        internal static List<List<BookDrop>> BookOfEverythingBundles = new List<List<BookDrop>>();

        internal static int BookOfEverythingOpened = 0;
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
            BookDrops.Clear();
            BookOfEverythingBundles.Clear();
            foreach (var rarityDrops in BoosterPackRarityDrops.Values)
                rarityDrops.Clear();

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
                new LorId(705020), new LorId(705021), new LorId(705031), new LorId(260005), new LorId(260006), new LorId(260007), new LorId(260008), new LorId(260009), new LorId(260010), new LorId(260011),
                new LorId(260012), new LorId(260013), new LorId(260014),
            });

            // Parse every combat page and save it as a drop
            foreach (var card in ItemXmlDataList.instance._cardInfoList)
            {
                AllDrops.Add(new BookDrop
                {
                    id = card.id,
                    chapter = card.Chapter,
                    type = DropItemType.Card,
                    collectible = collectibleCombatPages.Contains(card.id),
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
                    collectible = collectibleKeyPages.Contains(book.id),
                });
            }

            // Figure out drops & weights for booster packs
            foreach (var drop in AllDrops.Where(d => d.collectible))
            {
                Rarity dropRarity = drop.type == DropItemType.Card ? ItemXmlDataList.instance.GetCardItem(drop.id).Rarity : BookXmlList.Instance.GetData(drop.id).Rarity;

                BoosterPackRarityDrops[dropRarity].Add(drop);
            }

            if (SlotDataManager.BoESpheresEnabled)
            {
                BuildBookOfEverythingBundles();
                return;
            }

            
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
            }
        }

        internal static List<BookDropResult> GenerateDrops(LorId bookID)
        {
            List<BookDropResult> dropResults = new List<BookDropResult>();

            // Generate Drops
            if (bookID == new LorId("lorap", 123456)) // TODO: Make it. Also bias utility combat pages (?)
            {
                if (!PlaythruManager.CanOpenBookOfEverything())
                {
                    MessagePopup.ShowMessage("No Book of Everything bundle is available yet.");
                    return dropResults;
                }

                dropResults.AddRange(GrantBookOfEverythingBundle());

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

            return dropResults;
        }

        internal static void BuildBookOfEverythingBundles()
        {
            int bundleCount = SlotDataManager.BoEBundlesPerSphere?.Sum() ?? 0;
            if (bundleCount <= 0)
                return;

            var Random = GameUtils.CreateRandom("book_of_everything_bundles");
            List<BookDrop> combatPages = AllDrops
                .Where(d => d.collectible && d.type == DropItemType.Card)
                .GroupBy(d => d.id)
                .Select(g => g.First())
                .ToList();
            List<BookDrop> keyPages = AllDrops
                .Where(d => d.collectible && d.type == DropItemType.Equip)
                .GroupBy(d => d.id)
                .Select(g => g.First())
                .ToList();

            for (int i = 0; i < bundleCount; i++)
                BookOfEverythingBundles.Add(new List<BookDrop>());

            AddDropsToBundles(combatPages, Random);
            AddDropsToBundles(keyPages, Random, bundleCount / 2);
        }

        private static void AddDropsToBundles(List<BookDrop> drops, System.Random random, int startIndex = 0)
        {
            int bundleCount = BookOfEverythingBundles.Count;
            int bundleIndex = startIndex;

            while (drops.Count > 0)
            {
                BookOfEverythingBundles[bundleIndex].Add(drops.TakeRandom(random));
                bundleIndex = (bundleIndex + 1) % bundleCount;
            }
        }

        internal static List<BookDropResult> GrantBookOfEverythingBundle()
        {
            List<BookDropResult> dropResults = new List<BookDropResult>();

            if (BookOfEverythingOpened >= BookOfEverythingBundles.Count)
                return dropResults;

            foreach (BookDrop drop in BookOfEverythingBundles[BookOfEverythingOpened])
                dropResults.AddRange(GrantDropToMaxStack(drop));

            BookOfEverythingOpened++;
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
            int copiesToGrant = System.Math.Min(copies, GetMaxCopies(drop) - GetOwnedCopies(drop));

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
                    };

                    dropResult.bookInstanceId = BookInventoryModel.Instance.CreateBook(drop.id).instanceId;
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
            saveData.AddData("bookOfEverythingOpened", new SaveData(BookOfEverythingOpened));
            saveData.AddData("boosterPacksOpened", new SaveData(BoosterPacksOpened));

            return saveData;
        }

        internal static void LoadFromSaveData(SaveData saveData)
        {
            BookOfEverythingOpened = saveData.GetInt("bookOfEverythingOpened");
            BoosterPacksOpened = saveData.GetInt("boosterPacksOpened");
        }
    }
}

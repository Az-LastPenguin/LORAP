using System;
using System.Collections.Generic;
using System.Linq;
using BTAI;
using LORAP.Archipelago;
using LORAP.Utils;
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

        internal static List<BookDrop> allDrops = new List<BookDrop>();

        internal static Dictionary<LorId, List<BookDrop>> bookDrops = new Dictionary<LorId, List<BookDrop>>();

        internal static void Init()
        {
            Debug.Log("[LORAP] Initializing Book Content Manager");

            allDrops.Clear();

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
                allDrops.Add(new BookDrop
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
                allDrops.Add(new BookDrop
                {
                    id = book.id,
                    chapter = book.Chapter,
                    type = DropItemType.Equip,
                    collectible = collectiblePages.Contains(book.id),
                });
            }


            // Balance book drops if needed
            if (!SlotDataManager.BalanceBookContents)
                return;

            // Decide of what chapter each book will be
            Dictionary<LorId, int> bookChapters = new Dictionary<LorId, int>();

            // Reception Requirements
            if (SlotDataManager.ReceptionsProgression == ReceptionsProgression.Books || SlotDataManager.ReceptionsProgression == ReceptionsProgression.ProgressiveBooks)
            {
                foreach (var pair in SlotDataManager.ReceptionBookRequirements)
                {
                    StageClassInfo info = StageClassInfoList.Instance.GetData(pair.Key);

                    foreach (var id in pair.Value)
                    {
                        bookChapters[new LorId(id)] = info.chapter;
                    }
                }
            }

            // Floor Requirements
            Dictionary<SephirahType, List<int>> abnoChapters = new Dictionary<SephirahType, List<int>>() 
            {
                [SephirahType.Malkuth] = new List<int>() { 1, 3, 4, 5, 5 },
                [SephirahType.Yesod] = new List<int>() { 2, 3, 4, 5, 5 },
                [SephirahType.Hod] = new List<int>() { 2, 3, 4, 5, 5 },
                [SephirahType.Netzach] = new List<int>() { 3, 3, 4, 5, 5 },
                [SephirahType.Tiphereth] = new List<int>() { 4, 4, 5, 6, 6 },
                [SephirahType.Gebura] = new List<int>() { 5, 5, 6, 6, 6 },
                [SephirahType.Chesed] = new List<int>() { 5, 5, 6 ,6 ,6 },
                [SephirahType.Binah] = new List<int>() { 6, 6, 7, 7 },
                [SephirahType.Hokma] = new List<int>() { 6, 6, 7, 7 },
                [SephirahType.Keter] = new List<int>() { 1, 3, 5, 6, 7 },
            };

            if (SlotDataManager.FloorsRequireBooks)
            {
                foreach (var item in SlotDataManager.AbnoBookRequirements)
                {
                    var seph = item.Key;

                    for (int i = 0; i < item.Value.Count; i++)
                    {
                        foreach (var book in item.Value[i])
                        {
                            bookChapters[new LorId(book)] = abnoChapters[seph][i];
                        }
                    }
                }
            }

            // If there are leftover books, assign them their base chapters
            foreach (var book in DropBookXmlList.Instance._list.Where(b => b.DropItemList.Count > 0 && !bookChapters.Keys.Contains(b.id)))
            {
                bookChapters[book.id] = book.chapter;
            }

            // Assign drops from that chapter
            var Random = new System.Random(SlotDataManager.Seed);
            List<BookDrop> allowedDrops = allDrops.Where(d => d.collectible).ToList();

            foreach (int chapter in bookChapters.Values.Distinct())
            {
                List<BookDrop> chapterDrops = allowedDrops.Where(d => d.chapter == chapter).ToList();
                List<LorId> chapterBooks = bookChapters.Where(p => p.Value == chapter).Select(p => p.Key).ToList();
                int dropsPerBook = chapterDrops.Count() / chapterBooks.Count();
                int diff = chapterDrops.Count() - (dropsPerBook * chapterBooks.Count());

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

                    bookDrops[book] = drops;
                }
            }
        }

        internal static List<BookDropResult> GenerateDrops(LorId bookID) // TODO: Maybe add check for package id?
        {
            //DropBookInventoryModel.Instance.RemoveBook(bookID); // Remove only for filler books

            if (bookID.id == 123456) // TODO: Make it. // TODO: Bias utility combat pages
            {
                return new List<BookDropResult>();
            }
            if (bookID.id == 123457) // TODO: Make this too.
            {
                return new List<BookDropResult>();
            }

            if (!bookDrops.ContainsKey(bookID))
                return new List<BookDropResult>();

            List<BookDrop> pool = bookDrops[bookID];
            List<BookDropResult> dropResults = new List<BookDropResult>();

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
                else if (drop.type == DropItemType.Equip)
                {
                    int keyLimit = BookXmlList.Instance.GetData(drop.id).Rarity switch
                    {
                        Rarity.Common => 15,
                        Rarity.Uncommon => 12,
                        Rarity.Rare => 6,
                        Rarity.Unique => 1,
                        Rarity.Special => 1,
                        _ => 1,
                    };

                    // If we can have more of those create a keypage and store the instanceid to show in the result screen
                    if (BookInventoryModel.Instance.GetBookCount(drop.id) < keyLimit)
                        dropResult.bookInstanceId = BookInventoryModel.Instance.CreateBook(drop.id).instanceId;
                }
                else // Don't add result into the table if we didn't give player anything
                {
                    continue;
                }

                dropResults.Add(dropResult);
            }

            return dropResults;
        }
    }
}

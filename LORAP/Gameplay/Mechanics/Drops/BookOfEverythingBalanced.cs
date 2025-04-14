using System;
using System.Collections.Generic;
using System.Linq;
using GameSave;
using LORAP.Playthru;
using UnityEngine;

namespace LORAP.Gameplay.Systems.Drops
{
    internal class BookOfEverythingBalanced : BaseDropSystem
    {
        private int BookId = 123456;

        private int BooksBurned = 0;
        private int TotalBooks = 0;

        private Dictionary<BookDropItemInfo, float> CombatPageScoreTable = new Dictionary<BookDropItemInfo, float>();
        private float CombatMin;
        private float CombatMax;
        private Dictionary<BookDropItemInfo, float> KeyPageScoreTable = new Dictionary<BookDropItemInfo, float>();
        private float KeyMin;
        private float KeyMax;

        internal override void Setup(SlotDataStruct SlotData, int Seed)
        {
            CombatPageScoreTable.Clear();
            KeyPageScoreTable.Clear();

            TotalBooks = SlotData.TotalBooksOfEverything;

            var KeyPages = ContentManager.CustomBooks[BookId].DropItemList.Where(d => d.itemType == DropItemType.Equip).ToList();
            var CombatPages = ContentManager.CustomBooks[BookId].DropItemList.Where(d => d.itemType == DropItemType.Card).ToList();
            var Random = new System.Random(Seed);

            // Combat Pages
            foreach (var PageDrop in CombatPages)
            {
                var Page = ItemXmlDataList.instance.GetCardItem(PageDrop.id);

                var Chapter = Page.Chapter;
                
                // Box-Muller
                var u1 = Math.Max(1 - Random.NextDouble(), 0.01);
                var u2 = 1 - Random.NextDouble();

                var R = Math.Sqrt(-2 * Math.Log(u1, Math.E));

                var r1 = R * Math.Cos(2 * Math.PI * u2);
                var r2 = R * Math.Sin(2 * Math.PI * u2);

                float mean = 0;
                float dev = 1; // Selected by hand
                switch (Chapter)
                {
                    case 1:
                        mean = 12.6f;
                        dev = 4;
                        break;
                    case 2:
                        mean = 22;
                        dev = 5.3f;
                        break;
                    case 3:
                        mean = 37;
                        dev = 7f;
                        break;
                    case 4:
                        mean = 54f;
                        dev = 10;
                        break;
                    case 5:
                        mean = 67f;
                        dev = 14f;
                        break;
                    case 6:
                        mean = 84f;
                        dev = 15.5f;
                        break;
                    case 7:
                        mean = 95;
                        dev = 15f;
                        break;
                }

                // 8/100 of the scale from min to max per book

                float Score = (float)(mean + dev * r1);
                CombatPageScoreTable.Add(PageDrop, Score);
            }

            CombatMin = CombatPageScoreTable.Values.Min();
            CombatMax = CombatPageScoreTable.Values.Max();

            /*Debug.Log($"Combat Pages Scores: ");
            for (int i = 1; i < 8; i++)
            {
                foreach (var list in CombatPageScoreTable.Where(p => ItemXmlDataList.instance.GetCardItem(p.Key.id).Chapter == i))
                {
                    Debug.Log($"{list.Value}; {i}");
                }
            }*/

            // Key Pages
            foreach (var PageDrop in KeyPages)
            {
                var Page = BookXmlList.Instance.GetData(PageDrop.id);

                var Chapter = Page.Chapter;

                // Box-Muller
                var u1 = Math.Max(1 - Random.NextDouble(), 0.01);
                var u2 = 1 - Random.NextDouble();

                var R = Math.Sqrt(-2 * Math.Log(u1, Math.E));

                var r1 = R * Math.Cos(2 * Math.PI * u2);
                var r2 = R * Math.Sin(2 * Math.PI * u2);

                float mean = 0;
                float dev = 1; // Selected by hand
                switch (Chapter)
                {
                    case 1:
                        mean = 10.3f;
                        dev = 3.2f;
                        break;
                    case 2:
                        mean = 15.4f;
                        dev = 3f;
                        break;
                    case 3:
                        mean = 20;
                        dev = 3.2f;
                        break;
                    case 4:
                        mean = 31f;
                        dev = 4.9f;
                        break;
                    case 5:
                        mean = 41f;
                        dev = 6f;
                        break;
                    case 6:
                        mean = 50f;
                        dev = 7.5f;
                        break;
                    case 7:
                        mean = 65;
                        dev = 6f;
                        break;
                }

                // 5/100 of the scale from min to max per book

                float Score = (float)(mean + dev * r1);
                KeyPageScoreTable.Add(PageDrop, Score);
            }

            KeyMin = KeyPageScoreTable.Values.Min();
            KeyMax = KeyPageScoreTable.Values.Max();

            /*Debug.Log($"Key Pages Scores: ");
            for (int i = 1; i < 8; i++)
            {
                foreach (var list in KeyPageScoreTable.Where(p => BookXmlList.Instance.GetData(p.Key.id).Chapter == i))
                {
                    Debug.Log($"{list.Value}; {i}");
                }
            }*/




            // Unfinished Balancing thing (might use later idk)
            /*Dictionary<int, float> CombatExceptionMults = new Dictionary<int, float>()
            {
                //[701004] = 0.7f, // Divinatory Impact (Too much score for 5 dices)
                //[501001] = 3f, // Boundary of Death (Too low due to one dice)
                //[303001] = 0.8f, // Scratch That! (Too much score due to not accounting for 2 power loss for every dice)
                //[303002] = 0.8f, // Brawl (Too much score due to 4 dices that actually disappear in most cases)
                //[402005] = 0.8f, // Not Another Step (IMHO just should be a bit lower)
                //[701012] = 1.25f // Brace Up (Util page from 7th chapter, gotta be a bit higher)

            };

            // Calculate score of Combat Pages
            foreach (var PageDrop in CombatPages)
            {
                var Page = ItemXmlDataList.instance.GetCardItem(PageDrop.id);

                // Force mult for exceptions
                float ExMult = CombatExceptionMults.ContainsKey(PageDrop.id.id) ? CombatExceptionMults[PageDrop.id.id] : 1f;

                // Base score for the chapter (Move some later chapters' utility pages down the list)
                float ChapterScore = Page.Chapter;

                // Score is multiplied by the chapter
                float ChapterMult = 1f; // (float)Math.Pow(1.15, Page.Chapter - 1); //2f - (float)Math.Pow(0.65, Page.Chapter); //Page.Chapter * (float)Math.Pow(1.15, Page.Chapter);
                switch (Page.Chapter) {
                    case 2: ChapterMult = 2f; break;
                    case 3: ChapterMult = 3f; break;
                    case 4: ChapterMult = 4f; break;
                    case 5: ChapterMult = 5f; break;
                    case 6: ChapterMult = 6f; break;
                    case 7: ChapterMult = 7f; break;
                }

                // Mult for being something other than melee
                float RangeMult = 1f;
                switch (Page.Spec.Ranged)
                {
                    case (CardRange.Far): // Ranged
                        RangeMult = 1.2f;
                        break;
                    case (CardRange.FarArea): // Mass-Summation
                    case (CardRange.FarAreaEach): // Mass-Individual
                        RangeMult = 1.5f;
                        break;
                    case (CardRange.Instance): // Instant (It's literally a range type in the game's code)
                        RangeMult = 2f;
                        break;
                }

                // Score for light cost (<= 4 - more Score, > 4 - less Score)
                float CostScore = 4 - Page.Spec.Cost;

                // Score for having an effect
                float EffScore = Page.Script != "" ? 2.5f : 0f;

                // Calculate score of the dices
                float DiceScore = 0;
                int i = 0;
                foreach (var Dice in Page.DiceBehaviourList)
                {
                    // Take average roll of the dice
                    var AvgRoll = (Dice.Min + Dice.Dice) / 2f;

                    // Mult for having an effect
                    var DiceEffMult = Dice.Script != "" ? 1.05f : 1f;

                    // More dices = more score
                    DiceScore += AvgRoll * DiceEffMult * (2f - (float)Math.Pow(0.5, i));
                    i++;
                }

                // Final Score
                float PageScore = (ChapterScore + DiceScore + EffScore + CostScore + ((float)Random.NextDouble() - 0.5f)) * ChapterMult * RangeMult * ExMult;
                CombatPageScoreTable.Add(PageDrop, PageScore);
            }*/


            /*Dictionary<int, float> KeyExceptionMults = new Dictionary<int, float>()
            {
                [240001] = 0.2f, // Yujin
                [240002] = 0.2f, // Valentin
                [240003] = 0.2f, // Tenma
            };

            // Calculate score of Key Pages
            foreach (var PageDrop in KeyPages)
            {
                var Page = BookXmlList.Instance.GetData(PageDrop.id);

                // Force mult for exceptions
                float ExMult = KeyExceptionMults.ContainsKey(PageDrop.id.id) ? KeyExceptionMults[PageDrop.id.id] : 1f;

                // Base score for HP > 40
                float HPScore = Math.Max(Page.EquipEffect.Hp - 40, 0);

                // Bonus score for Stagger > 20
                float StaggerScore = Math.Max(Page.EquipEffect.Break - 20, 0);

                // Mult for the chapter of the page
                float ChapterMult = Page.Chapter;

                // Mult for being something other than melee
                float RangeMult = 1f;
                switch (Page.RangeType)
                {
                    case (EquipRangeType.Range): // Ranged
                        RangeMult = 1.1f;
                        break;
                    case (EquipRangeType.Hybrid): // Hybrid
                        RangeMult = 1.2f;
                        break;
                }

                // Mult for having more resistances
                float ResMult = 0.8f + ((int)Page.EquipEffect.SResist + (int)Page.EquipEffect.PResist + (int)Page.EquipEffect.HResist + (int)Page.EquipEffect.SBResist + (int)Page.EquipEffect.PBResist + (int)Page.EquipEffect.HBResist) / 6 * 0.1f;

                // Bonus score for passives (their cost)
                float PassiveScore = 0;
                foreach (var pid in Page.EquipEffect.PassiveList)
                {
                    var Passive = PassiveXmlList.Instance.GetData(pid);
                    PassiveScore += Passive.cost;
                }

                float FinalScore = (HPScore + StaggerScore + PassiveScore + ((float)Random.NextDouble() - 0.5f)) * ChapterMult * RangeMult * ResMult * ExMult;
                KeyPageScoreTable.Add(PageDrop, FinalScore);
            }*/
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
            List<BookDropResult> Drops = new List<BookDropResult>();

            var Random = new System.Random(PlaythruManager.Seed / 2 + BooksBurned); // Did what first came to mind lol

            Debug.Log($"TOTAL: {TotalBooks}; BURNED: {BooksBurned}");
            Debug.Log($"Key: {KeyMin}, {KeyMax}");
            Debug.Log($"Combat: {CombatMin}, {CombatMax}");

            float CombatDev = (CombatMax - CombatMin) / TotalBooks * 8;
            float CombatPos = CombatMin + (CombatMax - CombatMin) / TotalBooks * BooksBurned;

            float KeyDev = (KeyMax - KeyMin) / TotalBooks * 5;
            float KeyPos = KeyMin + (KeyMax - KeyMin) / TotalBooks * BooksBurned;

            Debug.Log($"Key: {KeyPos - KeyDev} < {KeyPos} < {KeyPos + KeyDev}");
            Debug.Log($"Combat: {CombatPos - CombatDev} < {CombatPos} < {CombatPos + CombatDev}");
            var KeyPages = KeyPageScoreTable.Where(p => p.Value >= KeyPos - KeyDev && p.Value <= KeyPos + KeyDev);
            var CombatPages = CombatPageScoreTable.Where(p => p.Value >= CombatPos - CombatDev && p.Value <= CombatPos + CombatDev);
            Debug.Log($"Key: {KeyPages.Count()}; Combat: {CombatPages.Count()}");
            for (int i = 0; i < 16; i++)
            {
                var KeyPage = Random.Next(0, 100) >= 80;
                var Pool = KeyPage ? KeyPages.Where(d => BookInventoryModel.Instance.GetBookCount(d.Key.id) < (BookXmlList.Instance.GetData(d.Key.id).Rarity == Rarity.Unique ? 1 : 5 - (int)BookXmlList.Instance.GetData(d.Key.id).Rarity)) : CombatPages;

                if (KeyPage && Pool.Count() == 0)
                {
                    Pool = CombatPages;
                    KeyPage = false;
                }

                var Selected = Pool.ElementAt(Random.Next(Pool.Count()));
                BookDropResult Drop = new BookDropResult();
                Drop.id = Selected.Key.id;
                Drop.itemType = Selected.Key.itemType;
                Drop.number = Selected.Key.itemType == DropItemType.Card ? Random.Next(1,3) : 1;

                if (KeyPage)
                    Drop.bookInstanceId = BookInventoryModel.Instance.CreateBook(Selected.Key.id).instanceId;
                else
                    InventoryModel.Instance.AddCard(Selected.Key.id);

                Drops.Add(Drop);
            }

            BooksBurned++;

            return Drops;
        }
    }
}

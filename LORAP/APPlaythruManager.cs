using CustomInvitation;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine.UIElements.UIR;

namespace LORAP
{
    internal static class APPlaythruManager
    {
        internal static List<int> OpenedReceptions = new List<int>();

        internal static Dictionary<SephirahType, int> EmotionCardAmounts = new Dictionary<SephirahType, int>()
        {
            [SephirahType.Keter] = 0,
            [SephirahType.Malkuth] = 0,
            [SephirahType.Yesod] = 0,
            [SephirahType.Hod] = 0,
            [SephirahType.Netzach] = 0,
            [SephirahType.Tiphereth] = 0,
            [SephirahType.Gebura] = 0,
            [SephirahType.Chesed] = 0,
            [SephirahType.Binah] = 0,
            [SephirahType.Hokma] = 0,
        };

        internal static Dictionary<SephirahType, int> EGOAmounts = new Dictionary<SephirahType, int>()
        {
            [SephirahType.Keter] = 0,
            [SephirahType.Malkuth] = 0,
            [SephirahType.Yesod] = 0,
            [SephirahType.Hod] = 0,
            [SephirahType.Netzach] = 0,
            [SephirahType.Tiphereth] = 0,
            [SephirahType.Gebura] = 0,
            [SephirahType.Chesed] = 0,
            [SephirahType.Binah] = 0,
            [SephirahType.Hokma] = 0,
        };

        internal static Dictionary<SephirahType, int> AbnoProgress = new Dictionary<SephirahType, int>()
        {
            [SephirahType.Keter] = 1,
            [SephirahType.Malkuth] = 1,
            [SephirahType.Yesod] = 1,
            [SephirahType.Hod] = 1,
            [SephirahType.Netzach] = 1,
            [SephirahType.Tiphereth] = 1,
            [SephirahType.Gebura] = 1,
            [SephirahType.Chesed] = 1,
            [SephirahType.Binah] = 1,
            [SephirahType.Hokma] = 1,
        };

        internal static bool BinahUnlocked = false;

        internal static bool BlackSilenceUnlocked = false;

        internal static int MaxPassiveCost = 8;

        private static List<int> BookIds = new List<int>() { 200001, 200002, 200003, 200004, 200005, 200006, 200007, 200008, 200009, 200010, 200011, 200012, 200013, 200014, 200015, 200016,
        210001, 210002, 210003, 210004, 210008, 210009,
        220001, 220002, 220003, 220004, 220005, 220006, 220007, 220008, 220009, 220010, 220011, 220012, 220013, 220014, 220015, 220016, 220017, 220018, 220019, 220020, 220021,
        230001, 230002, 230003, 230004, 230005, 230006, 230007, 230008, 230009, 230010, 230011, 230012, 230013, 230014, 230015, 230016, 230017, 230018, 230019, 230020, 230021, 230022, 230023,
        230024, 230025, 230026, 230027, 230028, 230029, 230030,
        240001, 240002, 240003, 240004, 240005, 240006, 240008, 240009, 240010, 240011, 240012, 240013, 240014, 240015, 240016, 240017, 240018, 240019, 240020, 240021, 240022, 240023, 243001,
        243002, 243003, 243004,
        250001, 250002, 250003, 250004, 250005, 250006, 250007, 250008, 250009, 250010, 250011, 250012, 250013, 250014, 250015, 250016, 250017, 250018, 250019, 250020, 250021, 250022, 250023,
        250024, 250025, 250026, 250027, 250028, 250029, 250030, 250031, 250032, 250033, 250034, 250035, 250036, 250037, 243005, 252001, 252002, 253001, 254001, 254002, 255001, 255002, 256002,
        260001, 260002, 260003, 260004};

        internal static List<DropBookXmlInfo> CustomBooks = new List<DropBookXmlInfo>();

        private static DropBookXmlInfo CreateCustomBook(int id, DropItemState state, int dropNum, List<BookDropItemInfo> dropList)
        {
            var Book = new DropBookXmlInfo();
            Book._id = id;
            Book.workshopName = "";
            Book.workshopID = "Archipelago";
            Book.itemDropState = state;
            Book.DropNum = dropNum;
            Book.DropItemList = dropList;
            //Book.InitializeDropItemList(id.ToString());
            Traverse.Create(Singleton<DropBookXmlList>.Instance).Field<List<DropBookXmlInfo>>("_list").Value.Add(Book);
            Traverse.Create(Singleton<DropBookXmlList>.Instance).Field<Dictionary<LorId, DropBookXmlInfo>>("_dict").Value.Add(Book.id, Book);

            return Book;
        }

        internal static void SetupGame()
        {
            // Custom books for drops
            List<BookDropItemInfo> CommonKeyPages = new List<BookDropItemInfo>();
            List<BookDropItemInfo> UncommonKeyPages = new List<BookDropItemInfo>();
            List<BookDropItemInfo> RareKeyPages = new List<BookDropItemInfo>();
            List<BookDropItemInfo> UniqueKeyPages = new List<BookDropItemInfo>();

            List<BookDropItemInfo> CombatPages = new List<BookDropItemInfo>();

            List<int> foundIds = new List<int>();

            foreach (int bid in BookIds)
            {
                var book = Singleton<DropBookXmlList>.Instance.GetData(bid);

                foreach (var p in book.DropItemList)
                {
                    if (foundIds.Contains(p.id.id) || (p.itemType == DropItemType.Card && ItemXmlDataList.instance.GetCardItem(p.id).isError))
                        continue;

                    if (p.itemType == DropItemType.Equip)
                    {
                        var info = Singleton<BookXmlList>.Instance.GetData(p.id);

                        switch (info.Rarity)
                        {
                            case Rarity.Common:
                                CommonKeyPages.Add(p);
                                break;
                            case Rarity.Uncommon:
                                UncommonKeyPages.Add(p);
                                break;
                            case Rarity.Rare:
                                RareKeyPages.Add(p);
                                break;
                            case Rarity.Unique:
                                UniqueKeyPages.Add(p);
                                break;
                        }
                    }
                    else if (p.itemType == DropItemType.Card)
                    {
                        CombatPages.Add(p);
                    }

                    foundIds.Add(p.id.id);
                }
            }

            CustomBooks.Add(CreateCustomBook(123456, DropItemState.Equip, 1, CommonKeyPages));
            CustomBooks.Add(CreateCustomBook(123457, DropItemState.Equip, 1, UncommonKeyPages));
            CustomBooks.Add(CreateCustomBook(123458, DropItemState.Equip, 1, RareKeyPages));
            CustomBooks.Add(CreateCustomBook(123459, DropItemState.Equip, 1, UniqueKeyPages));

            CustomBooks.Add(CreateCustomBook(123460, DropItemState.Card, 9, CombatPages));
            CustomBooks.Add(CreateCustomBook(123461, DropItemState.Card, 18, CombatPages));
        }

        internal static void OpenReception(int id)
        {
            OpenedReceptions.Add(id);

            (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel.SetStoryLine();
        }

        internal static bool GetReceptionOpened(int id)
        {
            return OpenedReceptions.Contains(id);
        }

        private static void FloorUpgradePopup(string text, SephirahType seph = SephirahType.None)
        {
            if (UI.UIController.Instance.CurrentUIPhase == UIPhase.Sephirah && seph != SephirahType.None)
            {
                GameSceneManager.Instance.ActivateUIController();
                UI.UIController.Instance.SetCurrentSephirah(seph);
                UI.UIController.Instance.CallUIPhase(UIPhase.Sephirah);
            }
            UIAlarmPopup.instance.SetAlarmText(text);
        }

        private static string FloorName(SephirahType seph)
        {
            string floor = "";
            switch (seph)
            {
                case SephirahType.None:
                    floor = "";
                    break;
                case SephirahType.Keter:
                    floor = "Floor of General Works";
                    break;
                case SephirahType.Malkuth:
                    floor = "Floor of History";
                    break;
                case SephirahType.Yesod:
                    floor = "Floor of Technological Sciences";
                    break;
                case SephirahType.Hod:
                    floor = "Floor of Literature";
                    break;
                case SephirahType.Netzach:
                    floor = "Floor of Art";
                    break;
                case SephirahType.Tiphereth:
                    floor = "Floor of Natural Sciences";
                    break;
                case SephirahType.Chesed:
                    floor = "Floor of Language";
                    break;
                case SephirahType.Gebura:
                    floor = "Floor of Social Sciences";
                    break;
                case SephirahType.Hokma:
                    floor = "Floor of Philosophy";
                    break;
                case SephirahType.Binah:
                    floor = "Floor of Religion";
                    break;
            }
            return floor;
        }

        internal static void OpenFloor(SephirahType seph)
        {
            if (LibraryModel.Instance.IsOpenedSephirah(seph)) return;

            LibraryModel.Instance.OpenSephirah(seph);

            if (seph == SephirahType.Binah)
            {
                var floor = Traverse.Create(LibraryModel.Instance).Field("_floorList").GetValue<List<LibraryFloorModel>>().Find(f => f.Sephirah == seph);
                floor.SetOpenedUnitCount(Math.Min(5, floor.GetOpendUnitCount() + 1));
            }


            FloorUpgradePopup($"{FloorName(seph)} was Opened!", seph);
        }

        internal static void LevelUpFloor(SephirahType seph)
        {
            if (EGOAmounts[seph] > 4) return;

            EmotionCardAmounts[seph]++;

            //if (UI.UIController.Instance.CurrentUIPhase == UIPhase.Sephirah)
            //    UIGetAbnormalityPanel.instance.SetData(LibraryModel.Instance.GetFloor(seph));
            //else
            FloorUpgradePopup($"{FloorName(seph)} upgrade! +3 Emotion Cards!", seph);
        }

        internal static void AddLibrarian(SephirahType seph)
        {
            if (EGOAmounts[seph] > 4) return;

            var floor = Traverse.Create(LibraryModel.Instance).Field("_floorList").GetValue<List<LibraryFloorModel>>().Find(f => f.Sephirah == seph);
            floor.SetOpenedUnitCount(Math.Min(5, floor.GetOpendUnitCount() + 1));
            FloorUpgradePopup($"{FloorName(seph)} upgrade! +1 Librarian!", seph);
        }

        internal static void AddEGO(SephirahType seph)
        {
            if (EGOAmounts[seph] > 4) return; 

            EGOAmounts[seph]++;
            FloorUpgradePopup($"{FloorName(seph)} upgrade! +1 EGO card!", seph);
        }

        internal static void AddAbnoProgress(SephirahType seph)
        {
            if (EGOAmounts[seph] > 5) return;

            AbnoProgress[seph]++;
        }

        internal static void GiveCustomBook(int i, int num = 1)
        {
            int max = 1;
            switch (i)
            {
                case 0:
                    max = 5;
                    break;
                case 1:
                    max = 4;
                    break;
                case 2:
                    max = 3;
                    break;
            }

            if (CustomBooks[i].DropItemList.Where(p => Singleton<BookInventoryModel>.Instance.GetBookCount(p.id) < max).Count() > 0)
                Singleton<DropBookInventoryModel>.Instance.AddBook(CustomBooks[i].id, num);
        }

        internal static BookDropResult GetDropEquip(Rarity rarity)
        {
            var pool = CustomBooks[(int)rarity];

            BookDropResult result = new BookDropResult();

            int max = 1;
            switch (rarity)
            {
                case Rarity.Common:
                    max = 5;
                    break;
                case Rarity.Uncommon:
                    max = 4;
                    break;
                case Rarity.Rare:
                    max = 3;
                    break;
            }

            var rng = RandomUtil.SelectOne(pool.DropItemList.Where(p => Singleton<BookInventoryModel>.Instance.GetBookCount(p.id) < max).ToList());
            result.id = rng.id;
            result.bookInstanceId = Singleton<BookInventoryModel>.Instance.CreateBook(rng.id).instanceId;
            result.itemType = DropItemType.Equip;
            result.number = 1;

            return result;
        }

        internal static List<BookDropResult> GetDropCards(int amount)
        {
            var pool = CustomBooks[4];
            List<BookDropResult> Drops = new List<BookDropResult>();

            for (int i = 0; i < amount; i++)
            {
                BookDropResult result = new BookDropResult();

                var rng = RandomUtil.SelectOne(pool.DropItemList);
                LORAP.Instance.LogInfo($"{rng.id} {ItemXmlDataList.instance.GetCardItem(rng.id).id} {ItemXmlDataList.instance.GetCardItem(rng.id).isError}");
                Singleton<InventoryModel>.Instance.AddCard(rng.id);
                result.id = rng.id;
                result.itemType = DropItemType.Card;
                result.number = 1;

                Drops.Add(result);
            }

            return Drops;
        }

        internal static void UpMaxPassiveCost()
        {
            MaxPassiveCost++;
            FloorUpgradePopup($"Library upgrade! +1 Max Passive Point!");
        }

        internal static void UnlockBinah()
        {
            if (BinahUnlocked) return;

            BinahUnlocked = true;
            FloorUpgradePopup($"Library upgrade! Binah unlocked!", SephirahType.Binah);
        }

        internal static void UnlockBlackSilence()
        {
            if (BlackSilenceUnlocked) return;

            BlackSilenceUnlocked = true;
            LibraryModel.Instance.GetFloor(SephirahType.Keter).GetUnitDataList().Find((UnitDataModel x) => x.isSephirah)?.ResetForBlackSilence();
            FloorUpgradePopup($"Library upgrade! Black Silence unlocked!", SephirahType.Keter);
        }
    }

    internal static class LORClassExtensions
    {
        internal static int GetEGOAmount(this LibraryFloorModel floor)
        {
            return APPlaythruManager.EGOAmounts[floor.Sephirah];
        }

        internal static int GetEmotionCardAmount(this LibraryFloorModel floor)
        {
            return APPlaythruManager.EmotionCardAmounts[floor.Sephirah];
        }
    }
}
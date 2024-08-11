using HarmonyLib;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using UI;

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

        internal static DropBookXmlInfo ObjetdartBook;

        internal static void SetupGame()
        {
            // Custom books for drops
            List<BookDropItemXmlInfo> dropList = new List<BookDropItemXmlInfo>();

            foreach (int bid in BookIds)
            {
                var book = Singleton<DropBookXmlList>.Instance.GetData(bid);

                foreach (var p in book._DropItemList)
                {
                    if (p.itemType != DropItemType.Equip) continue;

                    var info = Singleton<BookXmlList>.Instance.GetData(p.id);

                    //Logger.LogInfo($"{info.Name} {info.Rarity}");

                    if (info.Rarity == Rarity.Unique)
                        dropList.Add(p);
                }
            }

            ObjetdartBook = new DropBookXmlInfo();
            ObjetdartBook._id = 123456;
            ObjetdartBook.workshopName = "Objet d art Page";
            ObjetdartBook.workshopID = "Archipelago";
            ObjetdartBook._targetText = "dropbook_9fixer";
            ObjetdartBook.itemDropState = DropItemState.Equip;
            ObjetdartBook.DropNum = 1;
            ObjetdartBook._DropItemList = dropList;
            ObjetdartBook.InitializeDropItemList("123456");
            Traverse.Create(Singleton<DropBookXmlList>.Instance).Field<List<DropBookXmlInfo>>("_list").Value.Add(ObjetdartBook);
            Traverse.Create(Singleton<DropBookXmlList>.Instance).Field<Dictionary<LorId, DropBookXmlInfo>>("_dict").Value.Add(ObjetdartBook.id, ObjetdartBook);

            /*foreach (DropBoxCount item in Singleton<DropBoxListModel>.Instance.GetEquipDropBoxCountInfoTable(ObjetdartBook.id))
            {
                Logger.LogInfo($"{item.itemInfo.id}");
            }*/
        }

        public static void OpenReception(int id)
        {
            OpenedReceptions.Add(id);

            (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel.SetStoryLine();
        } 

        public static bool GetReceptionOpened(int id)
        {
            return OpenedReceptions.Contains(id);
        }

        private static void FloorUpgradePopup(SephirahType seph, string text)
        {
            if (UI.UIController.Instance.CurrentUIPhase == UIPhase.Sephirah)
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

        public static void OpenFloor(SephirahType seph)
        {
            LibraryModel.Instance.OpenSephirah(seph);

            FloorUpgradePopup(seph, $"{FloorName(seph)} was Opened!");
        }

        public static void LevelUpFloor(SephirahType seph)
        {
            EmotionCardAmounts[seph]++;

            //if (UI.UIController.Instance.CurrentUIPhase == UIPhase.Sephirah)
            //    UIGetAbnormalityPanel.instance.SetData(LibraryModel.Instance.GetFloor(seph));
            //else
            FloorUpgradePopup(seph, $"{FloorName(seph)} upgrade! +3 Emotion Cards!");
        }

        public static void AddLibrarian(SephirahType seph)
        {
            var floor = Traverse.Create(LibraryModel.Instance).Field("_floorList").GetValue<List<LibraryFloorModel>>().Find(f => f.Sephirah == seph);
            floor.SetOpenedUnitCount(Math.Min(5, floor.GetOpendUnitCount() + 1));
            FloorUpgradePopup(seph, $"{FloorName(seph)} upgrade! +1 Librarian!");
        }

        public static void AddEGO(SephirahType seph)
        {
            EGOAmounts[seph]++;
            FloorUpgradePopup(seph, $"{FloorName(seph)} upgrade! +1 EGO card!");
        }

        public static void AddAbnoProgress(SephirahType seph)
        {
            AbnoProgress[seph]++;
        }

        public static void GiveObjetPageBook()
        {
            Singleton<DropBookInventoryModel>.Instance.AddBook(123456);
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
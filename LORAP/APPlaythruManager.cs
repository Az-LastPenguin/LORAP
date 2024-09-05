using GameSave;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UI;

namespace LORAP
{
    internal class RarityDropInfo
    {
        public int totalPacks = 0;
        public int packsUsed = 0;
        public List<int> notFoundItems = new List<int>();
    }

    internal static class APPlaythruManager
    {
        internal static int ItemsReceived = 0;

        internal static int Seed = 143;

        internal static int Fillers = 10;

        internal static int Traps = 10;


        internal static List<int> OpenedReceptions = new List<int>();

        internal static List<int> FoundBooks = new List<int>();

        internal static Dictionary<SephirahType, int> AbnoPageAmounts = new Dictionary<SephirahType, int>()
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

        internal static Dictionary<Rarity, RarityDropInfo> RarityDrops = new Dictionary<Rarity, RarityDropInfo>()
        {
            [Rarity.Common] = new RarityDropInfo(),
            [Rarity.Uncommon] = new RarityDropInfo(),
            [Rarity.Rare] = new RarityDropInfo(),
            [Rarity.Unique] = new RarityDropInfo(),
        };


        internal static void LoadFromSaveData(SaveData saveData)
        {
            LORAP.Instance.LogInfo("Loading the game...");

            ItemsReceived = saveData.GetData("itemsReceived").GetIntSelf();
            foreach (SaveData dat in saveData.GetData("openedReceptions"))
            {
                OpenedReceptions.Add(dat.GetIntSelf());
            }

            foreach (SaveData dat in saveData.GetData("foundBooks"))
            {
                FoundBooks.Add(dat.GetIntSelf());
            }

            int num = 0;
            foreach (var d in saveData.GetData("abnoPageAmounts"))
            {
                AbnoPageAmounts[AbnoPageAmounts.Keys.ElementAt(num)] = d.GetIntSelf();
                num++;
            }

            num = 0;
            foreach (var d in saveData.GetData("egoAmounts"))
            {
                EGOAmounts[EGOAmounts.Keys.ElementAt(num)] = d.GetIntSelf();
                num++;
            }

            num = 0;
            foreach (var d in saveData.GetData("abnoProgress"))
            {
                AbnoProgress[AbnoProgress.Keys.ElementAt(num)] = d.GetIntSelf();
                num++;
            }

            BinahUnlocked = saveData.GetData("binahUnlocked").GetIntSelf() == 1;
            BlackSilenceUnlocked = saveData.GetData("blackSilenceUnlocked").GetIntSelf() == 1;
            MaxPassiveCost = saveData.GetData("maxPassiveCost").GetIntSelf();

            num = 0;
            foreach (var d in saveData.GetData("rarityDrops"))
            {
                RarityDrops[RarityDrops.Keys.ElementAt(num)].packsUsed = d.GetInt("packsUsed");
                foreach (var dt in d.GetData("notFoundItems"))
                {
                    RarityDrops[RarityDrops.Keys.ElementAt(num)].notFoundItems.Add(dt.GetIntSelf());
                }

                num++;
            }
        }

        internal static SaveData GetSaveData()
        {
            LORAP.Instance.LogInfo("Saving the game...");

            SaveData saveData = new SaveData();

            saveData.AddData("itemsReceived", new SaveData(ItemsReceived));
            saveData.AddData("openedReceptions", new SaveData(OpenedReceptions));
            saveData.AddData("foundBooks", new SaveData(FoundBooks));

            SaveData _saveData = new SaveData();
            foreach (var d in AbnoPageAmounts)
            {
                _saveData.AddToList(new SaveData(d.Value));
            }
            saveData.AddData("abnoPageAmounts", _saveData);

            _saveData = new SaveData();
            foreach (var d in EGOAmounts)
            {
                _saveData.AddToList(new SaveData(d.Value));
            }
            saveData.AddData("egoAmounts", _saveData);

            _saveData = new SaveData();
            foreach (var d in AbnoProgress)
            {
                _saveData.AddToList(new SaveData(d.Value));
            }
            saveData.AddData("abnoProgress", _saveData);

            saveData.AddData("binahUnlocked", new SaveData(BinahUnlocked ? 1 : 0));
            saveData.AddData("blackSilenceUnlocked", new SaveData(BlackSilenceUnlocked ? 1 : 0));
            saveData.AddData("maxPassiveCost", new SaveData(MaxPassiveCost));

            _saveData = new SaveData();
            foreach (var d in RarityDrops)
            {
                SaveData _saveData2 = new SaveData();

                _saveData2.AddData("packsUsed", new SaveData(d.Value.packsUsed));
                SaveData _saveData3 = new SaveData();
                foreach (var id in d.Value.notFoundItems)
                {
                    _saveData3.AddToList(new SaveData(id));
                }
                _saveData2.AddData("notFoundItems", _saveData3);

                _saveData.AddToList(_saveData2);
            }
            saveData.AddData("rarityDrops", _saveData);

            return saveData;
        }


        internal static bool IsReceptionOpened(int id)
        {
            return OpenedReceptions.Contains(id);
        }

        private static void FloorUpgradePopup(string text, SephirahType seph = SephirahType.None)
        {
            if (UI.UIController.Instance.CurrentUIPhase == UIPhase.Sephirah && seph != SephirahType.None && LibraryModel.Instance.IsOpenedSephirah(seph))
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
                    floor = "Floor of Social Sciences";
                    break;
                case SephirahType.Gebura:
                    floor = "Floor of Language";
                    break;
                case SephirahType.Hokma:
                    floor = "Floor of Religion";
                    break;
                case SephirahType.Binah:
                    floor = "Floor of Philosophy";
                    break;
            }
            return floor;
        }


        internal static void OpenReception(int id)
        {
            OpenedReceptions.Add(id);

            (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel.SetStoryLine();
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

        internal static void AddAnboPages(SephirahType seph)
        {
            if (AbnoPageAmounts[seph] > 4) return;

            AbnoPageAmounts[seph]++;

            //if (UI.UIController.Instance.CurrentUIPhase == UIPhase.Sephirah)
            //    UIGetAbnormalityPanel.instance.SetData(LibraryModel.Instance.GetFloor(seph));
            //else
            FloorUpgradePopup($"{FloorName(seph)} upgrade! +3 Abnormality Pages!", seph);
        }

        internal static void AddLibrarian(SephirahType seph)
        {
            var floor = Traverse.Create(LibraryModel.Instance).Field("_floorList").GetValue<List<LibraryFloorModel>>().Find(f => f.Sephirah == seph);

            if (floor.GetOpendUnitCount() > 4) return;

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
            if (AbnoProgress[seph] > 5) return;

            AbnoProgress[seph]++;
        }

        internal static void GiveCustomBook(int i, int num = 1)
        {
            Singleton<DropBookInventoryModel>.Instance.AddBook(CustomContentManager.CustomBooks[i].id, num);
        }

        internal static void GiveBoosterPack(Rarity rarity, int num = 1)
        {
            Singleton<DropBookInventoryModel>.Instance.AddBook(CustomContentManager.BoosterPacks[rarity].id, num);
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


        internal static List<BookDropResult> GenerateBoosterDrops(Rarity rarity)
        {
            var amount = 16;
            switch (rarity)
            {
                case Rarity.Uncommon:
                    amount = 12;
                    break;
                case Rarity.Rare:
                    amount = 8;
                    break;
                case Rarity.Unique:
                    amount = 4;
                    break;
            }

            RarityDropInfo info = RarityDrops[rarity];

            info.packsUsed++;

            var pack = CustomContentManager.CustomBooks[(int)rarity];
            List<BookDropResult> Drops = new List<BookDropResult>();

            var rng = new Random();

            for (int i = 0; i < amount; i++)
            {
                BookDropResult result = new BookDropResult();

                BookDropItemInfo selected;

                // Guarantee a copy of each page
                int amountToDrop = Math.Max(0, (info.totalPacks - info.packsUsed) * amount);
                if (amountToDrop < info.notFoundItems.Count)
                {
                    int guarantee = info.notFoundItems[0];
                    info.notFoundItems.RemoveAt(0);
                    LORAP.Instance.LogInfo($"Guaranteeing drop of {guarantee}");

                    var drop = pack.DropItemList.Find(i => i.id.id == guarantee);
                    if (drop.itemType == DropItemType.Equip)
                    {
                        result.id = drop.id;
                        result.bookInstanceId = Singleton<BookInventoryModel>.Instance.CreateBook(drop.id).instanceId;
                        result.itemType = DropItemType.Equip;
                        result.number = 1;
                    }
                    else
                    {
                        Singleton<InventoryModel>.Instance.AddCard(drop.id);
                        result.id = drop.id;
                        result.itemType = DropItemType.Card;
                        result.number = 1;
                    }

                    Drops.Add(result);

                    continue;
                }

                int max = 1; // Max of Key Page by rarity
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

                var Pool = pack.DropItemList.Where(i => i.itemType == DropItemType.Card).ToList();
                Pool.AddRange(pack.DropItemList.Where(i => i.itemType == DropItemType.Equip && Singleton<BookInventoryModel>.Instance.GetBookCount(i.id) < max).ToList());

                selected = RandomUtil.SelectOne(Pool);
                info.notFoundItems.Remove(selected.id.id);
                result.id = selected.id;
                result.itemType = selected.itemType;
                result.number = 1;

                if (selected.itemType == DropItemType.Equip)
                    result.bookInstanceId = Singleton<BookInventoryModel>.Instance.CreateBook(selected.id).instanceId;
                else
                    Singleton<InventoryModel>.Instance.AddCard(selected.id);

                Drops.Add(result);
            }

            LORAP.Instance.LogInfo($"{rarity} Pack Opening Progress: {info.packsUsed}/{info.totalPacks}");
            LORAP.Instance.LogInfo($"{rarity} Drops Progress: {pack.DropItemList.Count - info.notFoundItems.Count}/{pack.DropItemList.Count}");

            return Drops;
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
            return APPlaythruManager.AbnoPageAmounts[floor.Sephirah];
        }
    }
}
using HarmonyLib;
using LORAP.Playthru;
using System.Collections.Generic;
using UnityEngine;

namespace LORAP.Archipelago
{
    internal static class CheckManager
    {
        internal enum ItemType
        {
            Floor,
            AbnoPages,
            Librarian,
            EGO,
            PassivePoint,
            Reception,
            Binah,
            BlackSilence,
            BookOfEverything
        }

        internal class APItem
        {
            public ItemType category;
        }

        private class FloorUpgrade : APItem
        {
            public SephirahType seph;
        }

        private class Reception : APItem
        {
            public Reception()
            {
                category = ItemType.Reception;
            }

            public List<int> ids;
        }


        public static int BaseOffset = 143000;
        internal static Dictionary<long, APItem> ItemMap = new Dictionary<long, APItem>()
        {
            [BaseOffset + 0] = new FloorUpgrade() { category = ItemType.Floor, seph = SephirahType.Keter },
            [BaseOffset + 1] = new FloorUpgrade() { category = ItemType.Floor, seph = SephirahType.Malkuth },
            [BaseOffset + 2] = new FloorUpgrade() { category = ItemType.Floor, seph = SephirahType.Yesod },
            [BaseOffset + 3] = new FloorUpgrade() { category = ItemType.Floor, seph = SephirahType.Hod },
            [BaseOffset + 4] = new FloorUpgrade() { category = ItemType.Floor, seph = SephirahType.Netzach },
            [BaseOffset + 5] = new FloorUpgrade() { category = ItemType.Floor, seph = SephirahType.Tiphereth },
            [BaseOffset + 6] = new FloorUpgrade() { category = ItemType.Floor, seph = SephirahType.Gebura },
            [BaseOffset + 7] = new FloorUpgrade() { category = ItemType.Floor, seph = SephirahType.Chesed },
            [BaseOffset + 8] = new FloorUpgrade() { category = ItemType.Floor, seph = SephirahType.Binah },
            [BaseOffset + 9] = new FloorUpgrade() { category = ItemType.Floor, seph = SephirahType.Hokma },

            [BaseOffset + 10] = new FloorUpgrade() { category = ItemType.AbnoPages, seph = SephirahType.Keter },
            [BaseOffset + 11] = new FloorUpgrade() { category = ItemType.AbnoPages, seph = SephirahType.Malkuth },
            [BaseOffset + 12] = new FloorUpgrade() { category = ItemType.AbnoPages, seph = SephirahType.Yesod },
            [BaseOffset + 13] = new FloorUpgrade() { category = ItemType.AbnoPages, seph = SephirahType.Hod },
            [BaseOffset + 14] = new FloorUpgrade() { category = ItemType.AbnoPages, seph = SephirahType.Netzach },
            [BaseOffset + 15] = new FloorUpgrade() { category = ItemType.AbnoPages, seph = SephirahType.Tiphereth },
            [BaseOffset + 16] = new FloorUpgrade() { category = ItemType.AbnoPages, seph = SephirahType.Gebura },
            [BaseOffset + 17] = new FloorUpgrade() { category = ItemType.AbnoPages, seph = SephirahType.Chesed },
            [BaseOffset + 18] = new FloorUpgrade() { category = ItemType.AbnoPages, seph = SephirahType.Binah },
            [BaseOffset + 19] = new FloorUpgrade() { category = ItemType.AbnoPages, seph = SephirahType.Hokma },

            [BaseOffset + 20] = new FloorUpgrade() { category = ItemType.Librarian, seph = SephirahType.Keter },
            [BaseOffset + 21] = new FloorUpgrade() { category = ItemType.Librarian, seph = SephirahType.Malkuth },
            [BaseOffset + 22] = new FloorUpgrade() { category = ItemType.Librarian, seph = SephirahType.Yesod },
            [BaseOffset + 23] = new FloorUpgrade() { category = ItemType.Librarian, seph = SephirahType.Hod },
            [BaseOffset + 24] = new FloorUpgrade() { category = ItemType.Librarian, seph = SephirahType.Netzach },
            [BaseOffset + 25] = new FloorUpgrade() { category = ItemType.Librarian, seph = SephirahType.Tiphereth },
            [BaseOffset + 26] = new FloorUpgrade() { category = ItemType.Librarian, seph = SephirahType.Gebura },
            [BaseOffset + 27] = new FloorUpgrade() { category = ItemType.Librarian, seph = SephirahType.Chesed },
            [BaseOffset + 28] = new FloorUpgrade() { category = ItemType.Librarian, seph = SephirahType.Binah },
            [BaseOffset + 29] = new FloorUpgrade() { category = ItemType.Librarian, seph = SephirahType.Hokma },

            [BaseOffset + 30] = new FloorUpgrade() { category = ItemType.EGO, seph = SephirahType.Keter },
            [BaseOffset + 31] = new FloorUpgrade() { category = ItemType.EGO, seph = SephirahType.Malkuth },
            [BaseOffset + 32] = new FloorUpgrade() { category = ItemType.EGO, seph = SephirahType.Yesod },
            [BaseOffset + 33] = new FloorUpgrade() { category = ItemType.EGO, seph = SephirahType.Hod },
            [BaseOffset + 34] = new FloorUpgrade() { category = ItemType.EGO, seph = SephirahType.Netzach },
            [BaseOffset + 35] = new FloorUpgrade() { category = ItemType.EGO, seph = SephirahType.Tiphereth },
            [BaseOffset + 36] = new FloorUpgrade() { category = ItemType.EGO, seph = SephirahType.Gebura },
            [BaseOffset + 37] = new FloorUpgrade() { category = ItemType.EGO, seph = SephirahType.Chesed },
            [BaseOffset + 38] = new FloorUpgrade() { category = ItemType.EGO, seph = SephirahType.Binah },
            [BaseOffset + 39] = new FloorUpgrade() { category = ItemType.EGO, seph = SephirahType.Hokma },

            [BaseOffset + 40] = new Reception() { ids = new List<int>() { 20001, 20002, 20003 } },
            [BaseOffset + 41] = new Reception() { ids = new List<int>() { 20004 } },
            [BaseOffset + 42] = new Reception() { ids = new List<int>() { 20005 } },
 
            [BaseOffset + 43] = new Reception() { ids = new List<int>() { 30001 } },
            [BaseOffset + 44] = new Reception() { ids = new List<int>() { 30006 } },
            [BaseOffset + 45] = new Reception() { ids = new List<int>() { 30002 } },
            [BaseOffset + 46] = new Reception() { ids = new List<int>() { 30007 } },
            [BaseOffset + 47] = new Reception() { ids = new List<int>() { 30003 } },
            [BaseOffset + 48] = new Reception() { ids = new List<int>() { 30008 } },
            [BaseOffset + 49] = new Reception() { ids = new List<int>() { 30004 } },
            [BaseOffset + 50] = new Reception() { ids = new List<int>() { 30005 } },

            [BaseOffset + 51] = new Reception() { ids = new List<int>() { 40004 } },
            [BaseOffset + 52] = new Reception() { ids = new List<int>() { 40005 } },
            [BaseOffset + 53] = new Reception() { ids = new List<int>() { 40001 } },
            [BaseOffset + 54] = new Reception() { ids = new List<int>() { 40007 } },
            [BaseOffset + 55] = new Reception() { ids = new List<int>() { 40003 } },
            [BaseOffset + 56] = new Reception() { ids = new List<int>() { 40008 } },
            [BaseOffset + 57] = new Reception() { ids = new List<int>() { 40002 } },
            [BaseOffset + 58] = new Reception() { ids = new List<int>() { 40006 } },

            [BaseOffset + 59] = new Reception() { ids = new List<int>() { 50003, 50004 } },
            [BaseOffset + 60] = new Reception() { ids = new List<int>() { 50007 } },
            [BaseOffset + 61] = new Reception() { ids = new List<int>() { 50014 } },
            [BaseOffset + 62] = new Reception() { ids = new List<int>() { 50006 } },
            [BaseOffset + 63] = new Reception() { ids = new List<int>() { 50009 } },
            [BaseOffset + 64] = new Reception() { ids = new List<int>() { 50012 } },
            [BaseOffset + 65] = new Reception() { ids = new List<int>() { 50001, 50002 } },
            [BaseOffset + 66] = new Reception() { ids = new List<int>() { 50008 } },
            [BaseOffset + 67] = new Reception() { ids = new List<int>() { 50013 } },
            [BaseOffset + 68] = new Reception() { ids = new List<int>() { 50005 } },
            [BaseOffset + 69] = new Reception() { ids = new List<int>() { 50010 } },
            [BaseOffset + 70] = new Reception() { ids = new List<int>() { 50011 } },

            [BaseOffset + 71] = new Reception() { ids = new List<int>() { 60001, 60002 } },

            [BaseOffset + 72] = new Reception() { ids = new List<int>() { 100001 } },
            [BaseOffset + 73] = new Reception() { ids = new List<int>() { 100002 } },
            [BaseOffset + 74] = new Reception() { ids = new List<int>() { 100003 } },

            [BaseOffset + 75] = new Reception() { ids = new List<int>() { 100004 } },
            [BaseOffset + 76] = new Reception() { ids = new List<int>() { 100005 } },
            [BaseOffset + 77] = new Reception() { ids = new List<int>() { 100006 } },
            [BaseOffset + 78] = new Reception() { ids = new List<int>() { 100007 } },
            [BaseOffset + 79] = new Reception() { ids = new List<int>() { 100008 } },
 
            [BaseOffset + 80] = new Reception() { ids = new List<int>() { 100009 } },
            [BaseOffset + 81] = new Reception() { ids = new List<int>() { 100010 } },
            [BaseOffset + 82] = new Reception() { ids = new List<int>() { 100014 } },

            [BaseOffset + 83] = new Reception() { ids = new List<int>() { 100011 } },
            [BaseOffset + 84] = new Reception() { ids = new List<int>() { 100012 } },

            [BaseOffset + 85] = new Reception() { ids = new List<int>() { 100013 } },
            [BaseOffset + 86] = new Reception() { ids = new List<int>() { 100015 } },
            [BaseOffset + 87] = new Reception() { ids = new List<int>() { 100016 } },
            [BaseOffset + 88] = new Reception() { ids = new List<int>() { 100017 } },
            [BaseOffset + 89] = new Reception() { ids = new List<int>() { 100018 } },
            [BaseOffset + 90] = new Reception() { ids = new List<int>()  { 100019 } },

            [BaseOffset + 91] = new APItem() { category = ItemType.PassivePoint },

            [BaseOffset + 92] = new APItem() { category = ItemType.BookOfEverything },

            [BaseOffset + 93] = new APItem() { category = ItemType.Binah },
            [BaseOffset + 94] = new APItem() { category = ItemType.BlackSilence },
        };


        private static int LocationBookOffset = 143000;
        internal static List<int> BookIds = new List<int>()
        {
            200001,
            200002,
            200004,
            200005,
            200006,
            200007,
            200008,
            200009,
            200010,
            200011,
            200012,
            200013,
            200014,
            200015,
            200016,

            210001,
            210002,
            210003,
            210004,
            210005,
            210006,
            210008,
            210009,

            220003,
            220002,
            220005,
            220006,
            220007,
            220012,
            220013,
            220014,
            220015,
            220008,
            220009,
            220010,
            220011,
            220016,
            210007,
            220017,
            220018,
            220019,
            220020,
            220021,

            230001,
            230002,
            230003,
            230004,
            230005,
            230007,
            230008,
            230009,
            230010,
            230011,
            230012,
            230014,
            230015,
            230016,
            230017,
            230018,
            230019,
            230020,
            230021,
            230022,
            230023,
            230024,
            230025,
            230013,
            230026,
            230027,
            230028,
            230030,
            230029,

            240010,
            240011,
            240012,
            240013,
            240001,
            240002,
            240003,
            240004,
            240008,
            240009,
            240005,
            240006,
            240014,
            240022,
            240019,
            240020,
            240021,
            240023,
            240018,
            240015,
            240016,
            240017,
            243002,
            243001,
            243004,
            243003,

            250006,
            250010,
            250008,
            250009,
            250007,
            250015,
            250001,
            250002,
            250003,
            250004,
            250005,
            250011,
            250014,
            250013,
            250012,
            250016,
            250017,
            250019,
            250018,
            250020,
            250021,
            250022,
            250025,
            250024,
            250023,
            250029,
            250028,
            250027,
            250026,
            250037,
            250036,
            250035,
            250034,
            250033,
            250032,
            250030,
            250031,
            243005,
            252002,
            252001,
            253001,
            254001,
            254002,
            255002,
            255001,
            256002,
            256001,

            260001,
            260003,
            260002,
            260004,
        };

        private static int ClearOffset = 143600;
        internal static Dictionary<int, List<long>> ClearMap = new Dictionary<int, List<long>>()
        {
            // Ensemble
            [70001] = new List<long>() { ClearOffset + 1, ClearOffset + 2, ClearOffset + 3 },
            [70002] = new List<long>() { ClearOffset + 4, ClearOffset + 5, ClearOffset + 6 },
            [70003] = new List<long>() { ClearOffset + 7, ClearOffset + 8, ClearOffset + 9 },
            [70004] = new List<long>() { ClearOffset + 10, ClearOffset + 11, ClearOffset + 12 },
            [70005] = new List<long>() { ClearOffset + 13, ClearOffset + 14, ClearOffset + 15 },
            [70006] = new List<long>() { ClearOffset + 16, ClearOffset + 17, ClearOffset + 18 },
            [70007] = new List<long>() { ClearOffset + 19, ClearOffset + 20, ClearOffset + 21 },
            [70008] = new List<long>() { ClearOffset + 22, ClearOffset + 23, ClearOffset + 24 },
            [70009] = new List<long>() { ClearOffset + 25, ClearOffset + 26, ClearOffset + 27 },
            [70010] = new List<long>() { ClearOffset + 28, ClearOffset + 29, ClearOffset + 30 },

            // Black Silence
            [60003] = new List<long>() { ClearOffset + 31, ClearOffset + 32, ClearOffset + 33, ClearOffset + 34, ClearOffset + 35, ClearOffset + 36 },

            // Distorted Ensemble
            [60004] = new List<long>() { ClearOffset + 37, ClearOffset + 38, ClearOffset + 39, ClearOffset + 40, ClearOffset + 41, ClearOffset + 42 },

            // Keter Realization
            [210009] = new List<long>() { ClearOffset + 43, ClearOffset + 44, ClearOffset + 45, ClearOffset + 46, ClearOffset + 47, ClearOffset + 48, ClearOffset + 49, ClearOffset + 50 },
        };


        private static int LocationAbnoOffset = 143300;
        private static int AbnoRewardNum = 3;
        private static int RealizationRewardNum = 5;

        internal static void AbnoChecks(SephirahType seph)
        {
            int progress = seph.FloorModel().GetCurrentAbnoStage();
            int num = seph == SephirahType.Binah || seph == SephirahType.Hokma ? progress < 4 ? AbnoRewardNum : AbnoRewardNum * 2 + RealizationRewardNum : progress < 5 ? AbnoRewardNum : AbnoRewardNum + RealizationRewardNum;

            int baseid = LocationAbnoOffset + (AbnoRewardNum * 5 + RealizationRewardNum) * ((int)seph - 1) + AbnoRewardNum * (progress - 2);

            List<long> ids = new List<long>();
            for (int i = 0; i < num; i++)
            {
                ids.Add(baseid + i);
            }

            ids.ForEach(ConnectionManager.Check);
        }

        internal static void BookCheck(int id)
        {
            if (!BookIds.Contains(id)) return;
            ConnectionManager.Check(LocationBookOffset + BookIds.IndexOf(id));
        }

        internal static void ClearCheck(LorId id)
        {
            ClearCheck(id.id);
        }

        internal static void ClearCheck(int id)
        {
            if (!ClearMap.ContainsKey(id)) return;

            ClearMap[id].ForEach(ConnectionManager.Check);
        }

        internal static void ReceiveItem(long id)
        {
            if (!ItemMap.ContainsKey(id))
            {
                Debug.Log($"Cannot receive {id}!!");
                return;
            }

            APItem item = ItemMap[id];

            Debug.Log($"Receiving {id} - {item.category}");

            switch (item.category)
            {
                case ItemType.Floor:
                    PlaythruManager.OpenFloor(((FloorUpgrade)item).seph);
                    break;
                case ItemType.AbnoPages:
                    PlaythruManager.AddAnboPages(((FloorUpgrade)item).seph);
                    break;
                case ItemType.Librarian:
                    PlaythruManager.AddLibrarian(((FloorUpgrade)item).seph);
                    break;
                case ItemType.EGO:
                    PlaythruManager.AddEGO(((FloorUpgrade)item).seph);
                    break;
                case ItemType.PassivePoint:
                    PlaythruManager.UpMaxPassiveCost();
                    break;
                case ItemType.Reception:
                    Reception reception = (Reception)item;

                    foreach (var rid in reception.ids)
                    {
                        PlaythruManager.OpenReception(rid);
                    }

                    break;
                case ItemType.Binah:
                    PlaythruManager.UnlockBinah();
                    break;
                case ItemType.BlackSilence:
                    PlaythruManager.UnlockBlackSilence();
                    break;
                case ItemType.BookOfEverything:
                    PlaythruManager.GiveBook(123456);
                    break;
                default:
                    Debug.Log($"That item has no type??? wtf???");
                    return;
            }
        }
    }
}

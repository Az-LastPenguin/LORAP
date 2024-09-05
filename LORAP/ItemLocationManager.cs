using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace LORAP
{
    internal static class ItemLocationManager
    {
        internal enum ItemType
        {
            AbnoPages,
            Librarian,
            EGO,
            PassivePoint,
            BoosterPack,
            Reception,
            Binah,
            BlackSilence
        }

        internal class APItem
        {
            public ItemType category;
        }

        private class FloorUpgrade : APItem
        {
            public SephirahType seph;
        }

        private class BoosterPack : APItem
        {
            public BoosterPack()
            {
                category = ItemType.BoosterPack;
            }

            public Rarity rarity;
        }

        private class Reception : APItem
        {
            public Reception()
            {
                category = ItemType.Reception;
            }

            public int id;
        }

        private static int LocationBookOffset = 143000;
        private static int LocationAbnoOffset = 143300;

        public static int BaseOffset = 143000;

        internal static Dictionary<long, APItem> ItemMap = new Dictionary<long, APItem>()
        {
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

            [BaseOffset + 40] = new APItem() { category = ItemType.PassivePoint },

            [BaseOffset + 41] = new BoosterPack() { rarity = Rarity.Common },
            [BaseOffset + 42] = new BoosterPack() { rarity = Rarity.Uncommon },
            [BaseOffset + 43] = new BoosterPack() { rarity = Rarity.Rare },
            [BaseOffset + 44] = new BoosterPack() { rarity = Rarity.Unique },

            [BaseOffset + 46] = new Reception() { id = 20001 },
            [BaseOffset + 47] = new Reception() { id = 20004 },
            [BaseOffset + 48] = new Reception() { id = 20005 },

            [BaseOffset + 49] = new Reception() { id = 30001 },
            [BaseOffset + 50] = new Reception() { id = 30006 },
            [BaseOffset + 51] = new Reception() { id = 30002 },
            [BaseOffset + 52] = new Reception() { id = 30007 },
            [BaseOffset + 53] = new Reception() { id = 30003 },
            [BaseOffset + 54] = new Reception() { id = 30008 },
            [BaseOffset + 55] = new Reception() { id = 30004 },
            [BaseOffset + 56] = new Reception() { id = 30005 },

            [BaseOffset + 57] = new Reception() { id = 40004 },
            [BaseOffset + 58] = new Reception() { id = 40005 },
            [BaseOffset + 59] = new Reception() { id = 40001 },
            [BaseOffset + 60] = new Reception() { id = 40007 },
            [BaseOffset + 61] = new Reception() { id = 40003 },
            [BaseOffset + 62] = new Reception() { id = 40008 },
            [BaseOffset + 63] = new Reception() { id = 40002 },
            [BaseOffset + 64] = new Reception() { id = 40006 },

            [BaseOffset + 65] = new Reception() { id = 50003 },
            [BaseOffset + 66] = new Reception() { id = 50007 },
            [BaseOffset + 67] = new Reception() { id = 50014 },
            [BaseOffset + 68] = new Reception() { id = 50006 },
            [BaseOffset + 69] = new Reception() { id = 50009 },
            [BaseOffset + 70] = new Reception() { id = 50012 },
            [BaseOffset + 71] = new Reception() { id = 50001 },
            [BaseOffset + 72] = new Reception() { id = 50008 },
            [BaseOffset + 73] = new Reception() { id = 50013 },
            [BaseOffset + 74] = new Reception() { id = 50005 },
            [BaseOffset + 75] = new Reception() { id = 50010 },
            [BaseOffset + 76] = new Reception() { id = 50011 },

            [BaseOffset + 77] = new Reception() { id = 60001 },

            [BaseOffset + 78] = new Reception() { id = 100001 },
            [BaseOffset + 79] = new Reception() { id = 100002 },
            [BaseOffset + 80] = new Reception() { id = 100003 },

            [BaseOffset + 81] = new Reception() { id = 100004 },
            [BaseOffset + 82] = new Reception() { id = 100005 },
            [BaseOffset + 83] = new Reception() { id = 100006 },
            [BaseOffset + 84] = new Reception() { id = 100007 },
            [BaseOffset + 85] = new Reception() { id = 100008 },

            [BaseOffset + 86] = new Reception() { id = 100009 },
            [BaseOffset + 87] = new Reception() { id = 100010 },
            [BaseOffset + 88] = new Reception() { id = 100014 },

            [BaseOffset + 89] = new Reception() { id = 100011 },
            [BaseOffset + 90] = new Reception() { id = 100012 },

            [BaseOffset + 91] = new Reception() { id = 100013 },
            [BaseOffset + 92] = new Reception() { id = 100015 },
            [BaseOffset + 93] = new Reception() { id = 100016 },
            [BaseOffset + 94] = new Reception() { id = 100017 },
            [BaseOffset + 95] = new Reception() { id = 100018 },
            [BaseOffset + 96] = new Reception() { id = 100019 },

            [BaseOffset + 100] = new APItem() { category = ItemType.Binah },
            [BaseOffset + 101] = new APItem() { category = ItemType.BlackSilence },
        };

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

        private static Dictionary<SephirahType, int> AbnoIds = new Dictionary<SephirahType, int>()
        {
            [SephirahType.Keter] = LocationAbnoOffset,
            [SephirahType.Malkuth] = LocationAbnoOffset + 24,
            [SephirahType.Yesod] = LocationAbnoOffset + 48,
            [SephirahType.Hod] = LocationAbnoOffset + 72,
            [SephirahType.Netzach] = LocationAbnoOffset + 96,
            [SephirahType.Tiphereth] = LocationAbnoOffset + 120,
            [SephirahType.Gebura] = LocationAbnoOffset + 144,
            [SephirahType.Chesed] = LocationAbnoOffset + 168,
            [SephirahType.Binah] = LocationAbnoOffset + 192,
            [SephirahType.Hokma] = LocationAbnoOffset + 216,
        };

        internal static int SephAbnoToLocation(SephirahType seph)
        {
            return AbnoIds[seph];
        }

        internal static void SendBookCheck(int id)
        {
            LORAP.Instance.SendCheck(LocationBookOffset + BookIds.IndexOf(id));
        }

        internal static void SendAbnoChecks(SephirahType seph)
        {
            int id = SephAbnoToLocation(seph);
            id = id + (APPlaythruManager.AbnoProgress[seph] - 1) * 4;

            int num = 0;
            if (seph == SephirahType.Binah || seph == SephirahType.Hokma && APPlaythruManager.AbnoProgress[seph] < 5)
            {
                if (APPlaythruManager.AbnoProgress[seph] < 4)
                {
                    num = 4;
                } 
                else
                {
                    num = 11;
                }
            }
            else if (APPlaythruManager.AbnoProgress[seph] < 6)
            {
                if (APPlaythruManager.AbnoProgress[seph] < 5)
                {
                    num = 4;
                } 
                else
                {
                    num = 8;
                }
            }

            for(int i = 0; i < num; i++)
            {
                LORAP.Instance.SendCheck(id + i);
            }
        }

        internal static void ReceiveItem(long id)
        {
            if (!ItemMap.ContainsKey(id))
            {
                LORAP.Instance.LogInfo($"Cannot receive {id}!!");
                return;
            }

            APItem item = ItemMap[id];

            LORAP.Instance.LogInfo($"Receiving {id} - {item.category}");

            switch (item.category)
            {
                case ItemType.AbnoPages:
                    APPlaythruManager.AddAnboPages(((FloorUpgrade)item).seph);
                    break;
                case ItemType.Librarian:
                    APPlaythruManager.AddLibrarian(((FloorUpgrade)item).seph);
                    break;
                case ItemType.EGO:
                    APPlaythruManager.AddEGO(((FloorUpgrade)item).seph);
                    break;
                case ItemType.PassivePoint:
                    APPlaythruManager.UpMaxPassiveCost();
                    break;
                case ItemType.BoosterPack:
                    APPlaythruManager.GiveBoosterPack(((BoosterPack)item).rarity);
                    break;
                case ItemType.Reception:
                    Reception reception = (Reception)item;
                    switch (reception.id)
                    {
                        case 20001:
                            APPlaythruManager.OpenReception(reception.id);
                            APPlaythruManager.OpenReception(reception.id + 1);
                            APPlaythruManager.OpenReception(reception.id + 2);
                            break;
                        case 50003:
                            APPlaythruManager.OpenReception(reception.id);
                            APPlaythruManager.OpenReception(reception.id + 1);
                            break;
                        case 50001:
                            APPlaythruManager.OpenReception(reception.id);
                            APPlaythruManager.OpenReception(reception.id + 1);
                            break;
                        case 60001:
                            APPlaythruManager.OpenReception(reception.id);
                            APPlaythruManager.OpenReception(reception.id + 1);
                            break;
                        default:
                            APPlaythruManager.OpenReception(reception.id);
                            break;
                    }
                    break;
                case ItemType.Binah:
                    APPlaythruManager.UnlockBinah();
                    break;
                case ItemType.BlackSilence:
                    APPlaythruManager.UnlockBlackSilence();
                    break;
                default:
                    LORAP.Instance.LogInfo($"That item has no type???");
                    return;
            }
        }
    }
}

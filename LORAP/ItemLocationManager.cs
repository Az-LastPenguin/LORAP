using HarmonyLib;
using System.Collections.Generic;

namespace LORAP
{
    internal static class ItemLocationManager
    {
        private static int LocationBookOffset = 143000;
        private static int LocationAbnoOffset = 143300;

        public static int LibraryUpgradesOffset = 143000;
        public static int LibrarianUpgradesOffset = 143000 + 40;
        public static int ReceptionsOffset = 143000 + 46;
        public static int BinahOffset = LibraryUpgradesOffset + 100;
        public static int BlackSilenceOffset = LibraryUpgradesOffset + 101;

        private static List<int> BookIds = new List<int>()
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
            200017,

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

        private static List<int> ReceptionsIds = new List<int>()
        {
            20001,
            20004,
            20005,

            30001,
            30006,
            30002,
            30007,
            30003,
            30008,
            30004,
            30005,

            40004,
            40005,
            40001,
            40007,
            40003,
            40008,
            40002,
            40006,

            50003,
            50007,
            50014,
            50006,
            50009,
            50012,
            50001,
            50008,
            500013,
            50005,
            50010,
            50011,

            60001,


            100001,
            100002,
            100003,

            100004,
            100005,
            100006,
            100007,
            100008,

            100009,
            100010,
            100014,

            100011,
            100012,

            100013,
            100015,
            100016,
            100017,
            100018,
            100019,
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

        internal static int BookToLocation(int id)
        {
            return LocationBookOffset + BookIds.IndexOf(id);
        }

        internal static int SephAbnoToLocation(SephirahType seph)
        {
            return AbnoIds[seph];
        }

        internal static int ItemIdToReception(long id)
        {
            return ReceptionsIds[(int)(id - ReceptionsOffset)];
        }

        internal static void SendBookCheck(int id)
        {
            LORAP.Instance.SendCheck(BookToLocation(id));
        }

        internal static void SendAbnoChecks(SephirahType seph)
        {
            int id = SephAbnoToLocation(seph);
            id = id + (APPlaythruManager.AbnoProgress[seph] - 1) * 4;

            long[] ids = new long[] { }; 
            if (seph == SephirahType.Binah || seph == SephirahType.Hokma && APPlaythruManager.AbnoProgress[seph] < 5)
            {
                if (APPlaythruManager.AbnoProgress[seph] < 4)
                {
                    ids.AddItem(id + 0);
                    ids.AddItem(id + 1);
                    ids.AddItem(id + 2);
                    ids.AddItem(id + 3);
                } 
                else
                {
                    ids.AddItem(id + 0);
                    ids.AddItem(id + 1);
                    ids.AddItem(id + 2);

                    ids.AddItem(id + 3);
                    ids.AddItem(id + 4);
                    ids.AddItem(id + 5);

                    ids.AddItem(id + 6);
                    ids.AddItem(id + 7);
                    ids.AddItem(id + 8);
                    ids.AddItem(id + 9);
                    ids.AddItem(id + 10);
                }
            }
            else if (APPlaythruManager.AbnoProgress[seph] < 6)
            {
                if (APPlaythruManager.AbnoProgress[seph] < 5)
                {
                    ids.AddItem(id + 0);
                    ids.AddItem(id + 1);
                    ids.AddItem(id + 2);
                    ids.AddItem(id + 3);
                } 
                else
                {
                    ids.AddItem(id + 0);
                    ids.AddItem(id + 1);
                    ids.AddItem(id + 2);

                    ids.AddItem(id + 3);
                    ids.AddItem(id + 4);
                    ids.AddItem(id + 5);
                    ids.AddItem(id + 6);
                    ids.AddItem(id + 7);
                }
            }

            LORAP.Instance.SendCheck(ids);
        }

        internal static void ReceiveItem(long id)
        {
            LORAP.Instance.LogInfo($"Receiving {id}");

            SephirahType NumToSeph(long num)
            {
                switch (num)
                {
                    case 0:
                        return SephirahType.Keter;
                    case 1:
                        return SephirahType.Malkuth;
                    case 2:
                        return SephirahType.Yesod;
                    case 3:
                        return SephirahType.Hod;
                    case 4:
                        return SephirahType.Netzach;
                    case 5:
                        return SephirahType.Tiphereth;
                    case 6:
                        return SephirahType.Gebura;
                    case 7:
                        return SephirahType.Chesed;
                    case 8:
                        return SephirahType.Binah;
                    case 9:
                        return SephirahType.Hokma;
                }

                return SephirahType.None;
            }

            if (id == BlackSilenceOffset)
            {
                APPlaythruManager.UnlockBlackSilence();
            }
            else if (id == BinahOffset)
            {
                APPlaythruManager.UnlockBinah();
            }
            else if (id >= ReceptionsOffset) // Open receptions
            {
                LORAP.Instance.LogInfo("Reception");

                int receptionId = ItemIdToReception(id);
                switch (receptionId)
                {
                    case 20001:
                        APPlaythruManager.OpenReception(receptionId);
                        APPlaythruManager.OpenReception(receptionId + 1);
                        APPlaythruManager.OpenReception(receptionId + 2);
                        break;
                    case 50003:
                        APPlaythruManager.OpenReception(receptionId);
                        APPlaythruManager.OpenReception(receptionId + 1);
                        break;
                    case 50001:
                        APPlaythruManager.OpenReception(receptionId);
                        APPlaythruManager.OpenReception(receptionId + 1);
                        break;
                    case 60001:
                        APPlaythruManager.OpenReception(receptionId);
                        APPlaythruManager.OpenReception(receptionId + 1);
                        break;
                    default:
                        APPlaythruManager.OpenReception(receptionId);
                        break;
                }
            }
            else if (id > LibrarianUpgradesOffset) // Give booster packs
            {
                LORAP.Instance.LogInfo("Booster Pack");
                APPlaythruManager.GiveCustomBook((int)(id - LibrarianUpgradesOffset - 1));
            }
            else if (id == LibrarianUpgradesOffset) // Upgrade max passive cost
            {
                LORAP.Instance.LogInfo("Up Passive Cost");
                APPlaythruManager.UpMaxPassiveCost();
            }
            else if (id >= LibraryUpgradesOffset + 30) // Give EGO
            {
                LORAP.Instance.LogInfo("EGO Page");
                APPlaythruManager.AddEGO(NumToSeph(id - LibraryUpgradesOffset - 30));
            }
            else if (id >= LibraryUpgradesOffset + 20) // Unlock Librarians
            {
                LORAP.Instance.LogInfo("Librarian");
                APPlaythruManager.AddLibrarian(NumToSeph(id - LibraryUpgradesOffset - 20));
            }
            else if (id >= LibraryUpgradesOffset + 10) // Give Abno Pages
            {
                LORAP.Instance.LogInfo("Abno Pages");
                APPlaythruManager.AddAnboPages(NumToSeph(id - LibraryUpgradesOffset - 10));
            }
            else if (id >= LibraryUpgradesOffset) // Open Floors
            {
                LORAP.Instance.LogInfo("Open Floor");
                APPlaythruManager.OpenFloor(NumToSeph(id - LibraryUpgradesOffset));
            }
        }
    }
}

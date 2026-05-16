using LORAP.Playthru;
using System;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LORAP.Utils
{
    internal static class ClassExtensions
    {
        internal static bool IsOpen(this SephirahType seph)
        {
            return PlaythruManager.Floors[seph].Open;
        }

        internal static int GetCurrentAbnoStage(this SephirahType seph)
        {
            return PlaythruManager.Floors[seph].AbnoStage;
        }

        internal static int GetCurrentAbnoStage(this LibraryFloorModel floor)
        {
            return PlaythruManager.Floors[floor.Sephirah].AbnoStage;
        }

        internal static int GetEGOAmount(this LibraryFloorModel floor)
        {
            return PlaythruManager.Floors[floor.Sephirah].EGO;
        }

        internal static int GetAbnoPageAmount(this LibraryFloorModel floor)
        {
            return PlaythruManager.Floors[floor.Sephirah].AbnoPages;
        }

        internal static string FloorName(this SephirahType seph)
        {
            return seph switch
            {
                SephirahType.Malkuth => "Floor of History",
                SephirahType.Yesod => "Floor of Technological Sciences",
                SephirahType.Hod => "Floor of Literature",
                SephirahType.Netzach => "Floor of Art",
                SephirahType.Tiphereth => "Floor of Natural Sciences",
                SephirahType.Gebura => "Floor of Language",
                SephirahType.Chesed => "Floor of Social Sciences",
                SephirahType.Binah => "Floor of Philosophy",
                SephirahType.Hokma => "Floor of Religion",
                SephirahType.Keter => "Floor of General Works",
                _ => "",
            };
        }

        internal static string FloorTextId(this SephirahType seph)
        {
            return seph switch
            {
                SephirahType.Malkuth => "ui_malkuthfloor",
                SephirahType.Yesod => "ui_yesodfloor",
                SephirahType.Hod => "ui_hodfloor",
                SephirahType.Netzach => "ui_netzachfloor",
                SephirahType.Tiphereth => "ui_tipherethfloor",
                SephirahType.Gebura => "ui_chesedfloor",
                SephirahType.Chesed => "ui_geburafloor",
                SephirahType.Binah => "ui_hokmafloor",
                SephirahType.Hokma => "ui_binahfloor",
                SephirahType.Keter => "ui_keterfloor",
                _ => "",
            };
        }

        internal static LibraryFloorModel FloorModel(this SephirahType seph)
        {
            return LibraryModel.Instance._floorList.Find(f => f.Sephirah == seph);
        }


        // Convert integer to a roman number (0 to 100)
        private static string[][] romanNumerals = new string[][] {
            new string[] { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" }, // Ones
            new string[] { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" }, // Tens
        };

        internal static string ToRoman(this int number)
        {
            if (number == 0)
                return "0";

            char[] chars = number.ToString().ToCharArray();

            if (chars.Count() > 2)
                return "C";

            string result = "";

            for (int i = 0; i < chars.Count(); i++)
            {
                result += romanNumerals[i][Int32.Parse(chars[i].ToString())];
            }

            return result;
        }

        internal static void AddCallback(this EventTrigger evt, EventTriggerType type, UnityAction<BaseEventData> action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry()
            {
                eventID = type
            };
            entry.callback.AddListener(action);
            evt.triggers.Add(entry);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LORAP.Playthru;

namespace LORAP.Utils
{
    internal static class ClassExtensions
    {
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

        private static List<string> FloorNames = new List<string>() 
        {
            "", // None
            "Floor of History",
            "Floor of Technological Sciences",
            "Floor of Literature",
            "Floor of Art",
            "Floor of Natural Sciences",
            "Floor of Language",
            "Floor of Social Sciences",
            "Floor of Philosophy",
            "Floor of Religion",
            "Floor of General Works",
            "", // ETC
        };

        internal static string FloorName(this SephirahType seph)
        {
            return FloorNames[(int)seph];
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

        public static string ToRoman(this int number)
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
    }
}

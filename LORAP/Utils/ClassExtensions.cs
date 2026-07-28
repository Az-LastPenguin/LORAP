using LORAP.Playthru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


        // Convert integer to a roman number (0 to 100) (Stolen)
        internal static string ToRoman(this int num)
        {
            if (num <= 0)
                return "0";

            return ToRomanRecursive(num);
        }

        private static string ToRomanRecursive(int num)
        {
            return num switch
            {
                int n when n >= 100 => "C+",
                int n when n >= 90 => "XC" + ToRoman(num - 90),
                int n when n >= 50 => "L" + ToRoman(num - 50),
                int n when n >= 40 => "XL" + ToRoman(num - 40),
                int n when n >= 10 => "X" + ToRoman(num - 10),
                int n when n >= 9 => "IX" + ToRoman(num - 9),
                int n when n >= 5 => "V" + ToRoman(num - 5),
                int n when n >= 4 => "IV" + ToRoman(num - 4),
                int n when n >= 1 => "I" + ToRoman(num - 1),
                _ => "",
            };
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

        /// <summary>
        /// Take out a random element from this list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        internal static T TakeRandom<T>(this IList<T> list, System.Random random)
        {
            int rng = random.Next(list.Count);
            T element = list.ElementAt(rng);
            list.RemoveAt(rng);
            return element;
        }

        /// <summary>
        /// Take an element at index 0 from this list (Pop from the bottom of the stack).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        internal static T Pop<T>(this IList<T> list)
        {
            T element = list.ElementAt(0);
            list.RemoveAt(0);
            return element;
        }

        // Stolen from some unity forum post (and modified a bit)
        /// <summary>
        /// In-place shuffle of the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        internal static void Shuffle<T>(this IList<T> ts, System.Random random)
        {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = random.Next(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
        }
    }
}

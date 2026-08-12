using GameSave;
using LORAP.Playthru;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LORAP.Archipelago
{
    // Option Enums
    internal enum Endgoal
    {
        ReverberationEnsemble,
        BlackSilence,
        KeterRealization,
        DistortedEnsemble
    }

    internal enum AbnoPageShuffle
    {
        None,
        InFloor,
        Sets,
        Pages
    }

    internal enum AbnoPageRandomization
    {
        None,
        Guarantee,
        Unbound
    }

    internal enum BookContentsRandomization
    {
        BookChapter,
        StageChapter,
        Chaotic
    }

    internal enum BattleNodeKind
    {
        Reception,
        Stage
    }

    internal enum DeathlinkAction
    {
        UnitDeath,
        FloorWipe,
        StageLoss
    }

    internal enum ProgressionMode
    {
        BookRequirements,
        BoELayers
    }

    // Setting class
    internal interface ISetting
    {
        string Name { get; set; }

        string SlotDataID { get; set; }

        bool Overridable { get; set; }

        object GetValueAsObj();

        void SetValue(object value, bool overrideValue = false);
    }

    internal class Setting<T> : ISetting
    {
        public string Name { get; set; } // Will be displayed in Settings menu

        public string SlotDataID { get; set; } // If it has SlotDataID then it's AP option and mod will try to pull it from SlotData

        public T Value;

        public bool Overridable { get; set; } // If an AP option, this decides if client can override that option (for example disable deathlink)

        public T Override; // If Overridable, the setting will always return Override instead of Value, Value is used to restore the original setting value

        public Setting(string name, string slotdataid = "", bool overridable = false, T defaultValue = default)
        {
            Name = name;
            SlotDataID = slotdataid;
            Value = defaultValue;
            Overridable = overridable;
            Override = Value;

            SettingsManager.AllSettings[name] = this;
        }

        public object GetValueAsObj()
        {
            return Overridable ? Override : Value;
        }

        public T GetValue()
        {
            return Overridable ? Override : Value;
        }

        public void SetValue(object value, bool overrideValue = false)
        {
            SetValue((T)value);
        }

        public void SetValue(T value, bool overrideValue = false) // When not overriding, make both value and override same; When overriding only change override. Only time when game is not overriding is when parsing the slotdata
        {
            if (!overrideValue)
                Value = value;

            Override = value;
        }

        public void ResetValue()
        {
            Override = Value;
        }


        // Overloads for comparing and other stuff (I ❤︎⁠ POLYMORPHISM)
        public static implicit operator bool(Setting<T> sett)
        {
            if (sett.GetValue() is bool val)
                return val;

            return false;
        }

        public static bool operator ==(Setting<T> sett, T val)
        {
            return sett.GetValue().Equals(val);
        }

        public static bool operator !=(Setting<T> sett, T val)
        {
            return !(sett == val);
        }

        public static bool operator >(Setting<T> sett, int other)
        {
            if (sett.GetValue() is int val)
                return val > other;

            return false;
        }

        public static bool operator <(Setting<T> sett, int other)
        {
            if (sett.GetValue() is int val)
                return val < other;

            return false;
        }

        public override bool Equals(object obj)
        {
            return GetValue().Equals(obj);
        }

        public override int GetHashCode()
        {
            return GetValue().GetHashCode();
        }
    }

    // NOTE: This does handle SlotData BUT it only handles OPTIONS from the SlotData. Everything else is handled by SlotDataManager
    internal static class SettingsManager
    {
        internal static readonly Dictionary<string, ISetting> AllSettings = new Dictionary<string, ISetting>(); // Used for automating saving and loading

        /** CLIENT OPTIONS **/
        internal static Setting<bool> SendHintsOnReceptionScout = new Setting<bool>("Auto Reception Hints", defaultValue: true); // TODO: Change to false (or an Enum) after making AP Client

        /** AP OPTIONS **/
        /* Start and Goals */
        internal static Setting<List<Endgoal>> Endgoals = new Setting<List<Endgoal>>("Endgoals", "endgoals");

        internal static Setting<List<Endgoal>> PersistentGoals = new Setting<List<Endgoal>>("Persistent Goals", "persistent_goals");

        internal static Setting<int> EnsembleBattles = new Setting<int>("Ensemble Endgoal Battles", "ensemble_battles");

        internal static Setting<bool> EndgoalsAlwaysUnlocked = new Setting<bool>("Endgoals Always Unlocked", "endgoals_always_unlocked", true);

        /* Battle Graph and Progression */
        internal static Setting<ProgressionMode> RunProgressionMode = new Setting<ProgressionMode>("Progression Mode", "progression_mode");

        internal static Setting<int> SphereClearPercentage = new Setting<int>("Sphere Clear Percentage", "sphere_clear_percentage", defaultValue: 50);

        internal static Setting<bool> EnemiesTurnIntoChecks = new Setting<bool>("Enemies Turn Into Checks", "enemies_turn_into_checks", true);

        /* Randomization */
        internal static Setting<bool> ShuffleEnsembleFloors = new Setting<bool>("Shuffle Ensemble Floors", "shuffle_ensemble_floor", true);

        internal static Setting<AbnoPageShuffle> AbnoPageShuffle = new Setting<AbnoPageShuffle>("Abnormality Page Shuffle", "abno_page_shuffle");

        internal static Setting<AbnoPageRandomization> AbnoPageRandomization = new Setting<AbnoPageRandomization>("Abnormality Page Randomization", "abno_page_randomization");

        internal static Setting<bool> ExodiaGuarantee = new Setting<bool>("Guarantee Exodia Abnormality Sets", "exodia_guarantee");

        internal static Setting<bool> EgoPageShuffle = new Setting<bool>("Shuffle EGO Pages", "ego_page_shuffle");

        // Page randomization here (someday)

        /* Book Contents and Filler */
        internal static Setting<BookContentsRandomization> BookContentsRandomization = new Setting<BookContentsRandomization>("Randomize Book Contents", "book_contents_randomization");

        // Filler items here

        // Filler pages here

        // Remove exclusiveness here

        /* Deathlink */
        internal static Setting<bool> Deathlink = new Setting<bool>("Deathlink", "deathlink", true);

        internal static Setting<DeathlinkAction> IncomingDeathlink = new Setting<DeathlinkAction>("Incoming Deathlink", "incoming_deathlink", true);

        internal static Setting<DeathlinkAction> OutgoingDeathlink = new Setting<DeathlinkAction>("Outgoing Deathlink", "outgoing_deathlink", true);

        // Main Code
        internal static void ParseSlotData(Dictionary<string, object> slotData)
        {
            // Try to get every AP Option listed above from the slotData
            foreach (ISetting setting in AllSettings.Values)
            {
                if (setting.SlotDataID == "")
                    continue;

                if (!slotData.ContainsKey(setting.SlotDataID))
                    throw new Exception($"Option {setting.SlotDataID} ({setting.Name}) is missing from SlotData!\n Possible mod and .apworld version mismatch?");

                // OptionSets are the odd ones here, AP sends them as JSON arrays, not numbers.
                if (setting == Endgoals || setting == PersistentGoals)
                {
                    List<Endgoal> goals = (slotData[setting.SlotDataID] as JArray)
                        .Select(e => (Endgoal)Enum.Parse(typeof(Endgoal), e.Value<string>().Replace(" ", "")))
                        .ToList();

                    setting.SetValue(goals);

                    continue;
                }

                // Everything else is (currently) an int. Will have to change this when we get another string option but oh well
                int val = (int)(long)slotData[setting.SlotDataID];
                switch (setting)
                {
                    case Setting<bool> s:
                        s.SetValue(val == 1);
                        break;
                    case Setting<int> s:
                        s.SetValue(val);
                        break;
                    case var _ when setting.GetType().GetGenericArguments()[0].IsEnum:
                        setting.SetValue(Enum.ToObject(setting.GetType().GetGenericArguments()[0], val));
                        break;
                }
            }
        }

        // Saving/Loading overriden values // TODO: Complete it after making the client
        internal static SaveData GetSaveData()
        {
            SaveData saveData = new SaveData();

            foreach (ISetting setting in AllSettings.Values)
            {
                if (!setting.Overridable && setting.SlotDataID != "") // Only save overridable AP options and client settings
                    continue;

                switch (setting)
                {
                    case Setting<bool> s:
                        saveData.AddData(s.Name, new SaveData(s.Override ? 1 : 0));
                        break;
                    case Setting<int> s:
                        saveData.AddData(s.Name, new SaveData(s.Override));
                        break;
                    case var _ when setting.GetType().GetGenericArguments()[0].IsEnum:
                        saveData.AddData(setting.Name, new SaveData(Convert.ToInt32(setting.GetValueAsObj())));
                        break;
                }
            }

            return saveData;
        }

        internal static void LoadFromSaveData(SaveData saveData)
        {
            
        }
    }
}

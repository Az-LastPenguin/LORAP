using System;
using System.Collections.Generic;
using System.Linq;
using LORAP.Gameplay.Systems.Goals;
using LORAP.Playthru;
using UnityEngine;

namespace LORAP.Gameplay.Mechanics.Goals
{
    internal static class GoalsManager
    {
        internal static List<BaseGoal> Goals = new List<BaseGoal>()
        {
            new ClearGoal()
            {
                Name = "Reverberation Ensemble",
                ClearsRequired = 10,
                Stages = new List<int>() {70001, 70002, 70003, 70004, 70005, 70006, 70007, 70008, 70009, 70010},
            },
            new ClearGoal()
            {
                Name = "Black Silence",
                ClearsRequired = 1,
                Stages = new List<int>() {60003},
            },
            new ClearGoal()
            {
                Name = "Distorted Ensemble",
                ClearsRequired = 1,
                Stages = new List<int>() {60004},
            },
            new ClearGoal()
            {
                Name = "Keter Realization",
                ClearsRequired = 1,
                Stages = new List<int>() {210009},
            },
        };

        internal static void Setup(SlotDataStruct SlotData)
        {
            Goals.ForEach(g => g.Active = false);

            SlotData.EndGoals.ForEach(g => Goals.Where(gg => gg.Name == g).ToList().ForEach(gg => gg.Active = true));

            Goals.OfType<ClearGoal>().ElementAt(0).ClearsRequired = SlotData.EnsembleBattles;
        }

        internal static bool GoalsAchieved()
        {
            return Goals.Where(g => g.Active).All(g => g.Completed);
        }
    }
}

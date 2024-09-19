using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace LORAP.Patches
{
    [HarmonyPatch(typeof(StageController))]
    internal class EndGoalPatch
    {
        // Complete goals
        [HarmonyPatch("EndBattlePhase_invitation")]
        [HarmonyPrefix]
        static bool EndGoalCheck(StageController __instance)
        {
            StageModel stageModel = __instance.GetStageModel();

            if (stageModel.GetFrontAvailableWave() != null)
                return true;

            if (APPlaythruManager.EndGameBattlesBeaten.Contains(stageModel.ClassInfo.id.id))
                return true;

            var ensembleBattles = new List<int>() { 70001, 70002, 70003, 70004, 70005, 70006, 70007, 70008, 70009, 70010 };
            if (ensembleBattles.Contains(stageModel.ClassInfo.id.id))
            {
                APPlaythruManager.EndGameBattlesBeaten.Add(stageModel.ClassInfo.id.id);
                if (APPlaythruManager.EndGameBattlesBeaten.Count(id => ensembleBattles.Contains(id)) >= APPlaythruManager.EnsembleBattles && APPlaythruManager.Goals.Contains(APPlaythruManager.GoalType.ReverbEnsemble))
                {
                    foreach (var id in APPlaythruManager.EndGameBattlesBeaten.Where(id => !ensembleBattles.Contains(stageModel.ClassInfo.id.id)))
                    {
                        APPlaythruManager.EndGameBattlesBeaten.Add(id);
                    }
                }
            }

            if (stageModel.ClassInfo.id.id == 60003 && APPlaythruManager.Goals.Contains(APPlaythruManager.GoalType.BlackSilence))
            {
                APPlaythruManager.EndGameBattlesBeaten.Add(stageModel.ClassInfo.id.id);
                LORAP.Log("Black Silence beaten"); // Send End Goal here
            }

            if (stageModel.ClassInfo.id.id == 60004 && APPlaythruManager.Goals.Contains(APPlaythruManager.GoalType.DistortedEnsemble))
            {
                APPlaythruManager.EndGameBattlesBeaten.Add(stageModel.ClassInfo.id.id);
                LORAP.Log("Distorted Ensemble beaten"); // Send End Goal here
            }

            APPlaythruManager.CheckEndConditions();

            return false;
        }
    }
}

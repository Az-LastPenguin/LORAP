using HarmonyLib;
using LORAP.Archipelago;
using LORAP.Playthru;
using UI;

namespace LORAP.Patches
{
    [HarmonyPatch(typeof(UIBattleResultPanel))]
    internal class EndGoalPatch
    {
        // Complete goals
        [HarmonyPatch(nameof(UIBattleResultPanel.SetData))]
        [HarmonyPostfix]
        static void EndGoalCheck(UIBattleResultPanel __instance, TestBattleResultData resultdata)
        {
            var stageModel = resultdata.stagemodelInBattle;
            var won = resultdata.iswin;

            if (!won || PlaythruManager.ReceptionsCompleted.Contains(stageModel.ClassInfo._id))
                return;

            PlaythruManager.ReceptionsCompleted.Add(stageModel.ClassInfo._id);

            CheckManager.ClearCheck(stageModel.ClassInfo._id);

            PlaythruManager.CheckEndConditions();
        }
    }
}

using HarmonyLib;
using UI;
using GameSave;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using LORAP.Playthru;
using LORAP.Archipelago;
using LORAP.Gameplay.Mechanics.Goals;
using System;

namespace LORAP.Patches
{
    [HarmonyPatch(typeof(StageController))] 
    internal class AbnoChecks
    {
        // On Abno Suppression or Floor Realization end, send the checks
        [HarmonyPatch("EndBattlePhase_creature")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> AbnoChecksAndProgress(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Change Keter Realization reward code
            var instr = instructions.ToList();
            var pos = instr.FindIndex(i => i.opcode == OpCodes.Ldloc_2) + 4;

            instr.RemoveRange(pos, 20); // remove floor level check and floor level up, UiMainPanel.LevelUpFloor and saveplaydata

            instr.Insert(pos, new CodeInstruction(OpCodes.Ldloc_2)); // floor to stack from local
            instr.Insert(pos + 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(StageLibraryFloorModel), nameof(StageLibraryFloorModel.Sephirah)))); // get seph
            instr.Insert(pos + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlaythruManager), nameof(PlaythruManager.ProgressAbno)))); // +1 current abno for the floor

            instr.Insert(pos + 3, new CodeInstruction(OpCodes.Ldloc_0)); // stageModel to stack from local
            instr.Insert(pos + 4, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(StageModel), nameof(StageModel.ClassInfo)))); // get stageclassinfo from stagemodel
            instr.Insert(pos + 5, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(StageClassInfo), nameof(StageClassInfo.id)))); // get id from stageclassinfo
            instr.Insert(pos + 6, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CheckManager), nameof(CheckManager.ClearCheck), new List<Type>(){ typeof(LorId) }.ToArray()))); // give checks from the realization

            instr.Insert(pos + 7, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlaythruManager), nameof(PlaythruManager.CheckEndConditions)))); // check end conditions call

            instr.Insert(pos + 8, new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(SaveManager), nameof(SaveManager.Instance)))); // get savemanager
            instr.Insert(pos + 9, new CodeInstruction(OpCodes.Ldc_I4_1)); // 1 to stack (save file) (useless)
            instr.Insert(pos + 10, new CodeInstruction(OpCodes.Ldc_I4_0)); // 0 to stack (false) (idk)
            instr.Insert(pos + 11, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SaveManager), nameof(SaveManager.SavePlayData)))); // savemanager.SavePlayData
            instr.Insert(pos + 12, new CodeInstruction(OpCodes.Pop)); // pop boolean that is returned from the stack


            // Change other abnos reward code
            pos = instr.FindIndex(i => i.opcode == OpCodes.Ldc_I4_8) - 8;

            instr.RemoveRange(pos, 22); // remove floor level up stuff

            instr.Insert(pos, new CodeInstruction(OpCodes.Ldloc_2)); // floor to stack from local
            instr.Insert(pos + 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(StageLibraryFloorModel), nameof(StageLibraryFloorModel.Sephirah)))); // get seph
            instr.Insert(pos + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CheckManager), nameof(CheckManager.AbnoChecks)))); // send abno checks

            instr.Insert(pos + 3, new CodeInstruction(OpCodes.Ldloc_2)); // floor to stack from local
            instr.Insert(pos + 4, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(StageLibraryFloorModel), nameof(StageLibraryFloorModel.Sephirah)))); // get seph
            instr.Insert(pos + 5, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlaythruManager), nameof(PlaythruManager.ProgressAbno)))); // +1 current abno for the floor

            return instr;
        }
    }

    [HarmonyPatch(typeof(UIMainPanel))]
    internal class AbnoSuppression2
    {
        [HarmonyPatch(nameof(UIMainPanel.OnClickLevelUp))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> OnClickLevelUpPrefix(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Change floor level to current abno progress
            var instr = instructions.ToList();
            var pos = instr.FindIndex(i => i.opcode == OpCodes.Ldloc_2) + 1;

            instr.RemoveRange(pos, 1); //remove librarymodel.level getter

            instr.Insert(pos, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LORClassExtensions), nameof(LORClassExtensions.GetCurrentAbno)))); // get current abno number

            return instr;
        }
    }

    [HarmonyPatch(typeof(LibraryModel))]
    internal class AbnoSuppression3
    {
        [HarmonyPatch(typeof(LibraryModel))]
        internal class AbnoSuppression4
        {
            [HarmonyPatch(nameof(LibraryModel.CheckCreatureBossBattle))]
            [HarmonyPrefix]
            static bool CheckCreatureBossBattlePrefix(LibraryModel __instance, LibraryFloorModel floor, ref bool __result)
            {
                __result = (floor.Sephirah == SephirahType.Binah || floor.Sephirah == SephirahType.Hokma) ? (PlaythruManager.Floors[floor.Sephirah].CurrentAbno == 4) : (PlaythruManager.Floors[floor.Sephirah].CurrentAbno == 5);

                return false;
            }
        }

        [HarmonyPatch(nameof(LibraryModel.CanLevelUpSephirah))]
        [HarmonyPrefix]
        static bool CanLevelUpSephirahPrefix(LibraryModel __instance, SephirahType sep, ref bool __result)
        {
            __result = false;

            if (!Traverse.Create(__instance).Field<HashSet<SephirahType>>("_openedSephirah").Value.Contains(sep)) return false;

            __result = (sep == SephirahType.Binah || sep == SephirahType.Hokma) ? (PlaythruManager.Floors[sep].CurrentAbno < 5)
                : (sep == SephirahType.Keter && !GoalsManager.Goals.Find(g => g.Name == "Keter Realization").Active)
                ? (PlaythruManager.Floors[sep].CurrentAbno < 5) : (PlaythruManager.Floors[sep].CurrentAbno < 6);

            return false;
        }
    }

    [HarmonyPatch(typeof(UI.UIController))]
    internal class AbnoSuppression1
    {
        // Force current Abno Suppressions and Realizations.
        [HarmonyPatch(nameof(UI.UIController.OnClickStartCreatureStage))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> OnClickStartCreatureStagePrefix(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Change floor level to current abno progress
            var instr = instructions.ToList();
            var pos = instr.FindIndex(i => i.opcode == OpCodes.Stloc_0) - 1;

            instr.RemoveRange(pos, 1); //remove librarymodel.level getter

            instr.Insert(pos, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LORClassExtensions), nameof(LORClassExtensions.GetCurrentAbno)))); // get current abno number

            return instr;
        }
    }
}

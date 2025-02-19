using HarmonyLib;
using LORAP.Playthru;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UI;
using UnityEngine;

namespace LORAP.Patches
{
    [HarmonyPatch(typeof(StageLibraryFloorModel))]
    internal class ModifiedSlection
    {
        // Create custom Selection of Abno Pages when Emotion Level rises
        [HarmonyPatch("CreateSelectableList")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> CustomPagesSelectableList(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {   
            // Replace floorLevel in the GetDataList with player's currently owned abno pages number
            var instr = instructions.ToList();
            var pos = instr.FindIndex(i => i.opcode == OpCodes.Stloc_3) + 7;

            instr.RemoveRange(pos, 8);
            instr.Insert(pos, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(LORClassExtensions), nameof(LORClassExtensions.GetAbnoPageAmount)))); // get number of abno from floor
            instr.Insert(pos + 1, new CodeInstruction(OpCodes.Ldc_I4_1)); 
            instr.Insert(pos + 2, new CodeInstruction(OpCodes.Add)); // +1
            instr.Insert(pos + 3, new CodeInstruction(OpCodes.Stloc_3)); // save it

            return instr;
        }

        // Select random Abno Page
        [HarmonyPatch("RandomSelect")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> CustomAbnoPagesRandom(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Replace floorLevel in the GetDataList with player's currently owned abno pages number
            var instr = instructions.ToList();
            var pos = instr.FindIndex(i => i.opcode == OpCodes.Ldc_I4_1) + 8;

            instr.RemoveRange(pos, 11);
            //instr.Insert(pos, new CodeInstruction(OpCodes.Ldc_I4, LibraryModel.Instance.GetFloor(StageController.Instance.CurrentFloor).GetAbnoPageAmount() + 1));
            instr.Insert(pos, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(LORClassExtensions), nameof(LORClassExtensions.GetAbnoPageAmount))));
            instr.Insert(pos + 1, new CodeInstruction(OpCodes.Stloc_1));

            return instr;
        }

        // Select random EGO Page
        [HarmonyPatch("RandomSelectEgo")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> CustomEGOPagesRandom(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Limit the list of ego pages by the player's currently owned ego pages number with GetRange
            var instr = instructions.ToList();
            var pos = instr.FindIndex(i => i.opcode == OpCodes.Callvirt) + 1;

            LocalBuilder floor = generator.DeclareLocal(typeof(LibraryFloorModel));
            LocalBuilder seph = generator.DeclareLocal(typeof(SephirahType));

            instr.Insert(pos, new CodeInstruction(OpCodes.Stloc_S, seph.LocalIndex)); // save seph to local
            instr.Insert(pos + 1, new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(LibraryModel), nameof(LibraryModel.Instance)))); // get libraryModel
            instr.Insert(pos + 2, new CodeInstruction(OpCodes.Ldloc_S, seph.LocalIndex)); // seph to stack from local
            instr.Insert(pos + 3, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(LibraryModel), nameof(LibraryModel.GetFloor)))); // get libraryFloorModel
            instr.Insert(pos + 4, new CodeInstruction(OpCodes.Stloc_S, floor.LocalIndex)); // save floor to the new local
            instr.Insert(pos + 5, new CodeInstruction(OpCodes.Ldloc_S, seph.LocalIndex)); // seph to stack from local
            pos += 7; 
            instr.Insert(pos, new CodeInstruction(OpCodes.Ldc_I4_0)); // 0 to stack
            instr.Insert(pos + 1, new CodeInstruction(OpCodes.Ldloc_S, floor.LocalIndex)); // floor to stack from local
            instr.Insert(pos + 2, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(LORClassExtensions), nameof(LORClassExtensions.GetEGOAmount)))); // get number of ego from floor
            instr.Insert(pos + 3, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<EmotionEgoXmlInfo>), nameof(List<EmotionEgoXmlInfo>.GetRange)))); // egolist.GetRange()

            return instr;
        }
    }

    [HarmonyPatch(typeof(UIAbnormalityPanel))]
    internal class AbnoCardsList
    {
        // Show Abno and EGO Pages player has regardless of the in-game library level. Next three patches are related
        [HarmonyPatch(nameof(UIAbnormalityPanel.SetData))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> FloorAbnoList(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Change max i in the loop to current abno pages number
            var instr = instructions.ToList();
            var pos = instr.FindIndex(i => i.opcode == OpCodes.Sub) + 1;

            Label l = (Label)instr[pos].operand;
            pos -= 3;

            instr.RemoveRange(pos, 4);
            instr.Insert(pos, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(LORClassExtensions), nameof(LORClassExtensions.GetAbnoPageAmount))));
            instr.Insert(pos + 1, new CodeInstruction(OpCodes.Blt_S, l));

            return instr;
        }
    }

    [HarmonyPatch(typeof(UIAbnormalityCategoryPanel))]
    internal class AbnoCardsNamePatch
    {
        // Hide name of the abno because abno pages are from multiple abnos most of the time
        [HarmonyPatch(nameof(UIAbnormalityPanel.SetData))]
        [HarmonyPostfix]
        static void AbnoCardsName(UIAbnormalityCategoryPanel __instance, List<EmotionCardXmlInfo> data, int index, UIAbnormalityPanel panel)
        {
            __instance.txt_Title.text = "";
        }
    }

    [HarmonyPatch(typeof(UIEgoCardPanel))]
    internal class EGOCardsList
    {
        [HarmonyPatch(nameof(UIEgoCardPanel.SetData))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> FloorEGOList(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Change max i in the loop to current abno pages number
            var instr = instructions.ToList();
            var pos = instr.FindIndex(i => i.opcode == OpCodes.Stloc_0) + 1;

            instr.RemoveRange(pos, 21); // Remove if (floor.level < 6) {...}

            pos = instr.FindIndex(i => i.opcode == OpCodes.Add) + 3;

            instr.RemoveRange(pos, 2); // remove egoCardList.Count
            instr.Insert(pos, new CodeInstruction(OpCodes.Ldarg_1)); // floor to stack from args
            instr.Insert(pos + 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(LORClassExtensions), nameof(LORClassExtensions.GetEGOAmount)))); // get number of ego from floor

            return instr;
        }
    }

    [HarmonyPatch(typeof(UIFloorPanel))]
    internal class AbnoEGOButtons
    {
        [HarmonyPatch(nameof(UIFloorPanel.OnUpdatePhase))]
        [HarmonyPostfix]
        static void FloorAbnoEGOButtons(UIFloorPanel __instance)
        {
            __instance.abnormalityEgoTap.SetActive(true);
            __instance.onlyAbnormalityTap.SetActive(false);
        }
    }
}

using HarmonyLib;
using LORAP.Gameplay.Mechanics.Drops;
using LORAP.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;   
using UI;
using UnityEngine;

namespace LORAP.Patches
{
    [HarmonyPatch(typeof(UIBookPanel))]
    internal class DropsGeneration
    {
        // Generate Gacha Drops and Send AP Checks on Book Burn
        [HarmonyPatch("FeedBookTargetSephirah")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> GenerateDropsSendChecks(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Get drops from custom books
            CIWriter Writer = new CIWriter(instructions, generator);

            // Remove _ = floor.Exp;
            Writer.ToPattern(OpCodes.Ldloc_0, OpCodes.Callvirt, OpCodes.Pop);
            Writer.Remove(3);

            // Change FeedBook to GenerateDrops
            Writer.ToPattern(OpCodes.Ldloc_1, OpCodes.Ldloc_0, OpCodes.Call, OpCodes.Ldloc_3);
            Writer.Remove(7);
            Writer.Add(new CodeInstruction(OpCodes.Ldloc_1));
            Writer.Add(new CodeInstruction(OpCodes.Ldloc_3));
            Writer.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DropsManager), nameof(DropsManager.GenerateDrops))));

            // Remove _ = floor.Exp; _ = floor.Level; floor.GetMaxExp();
            Writer.ToPattern(OpCodes.Ldloc_0, OpCodes.Callvirt, OpCodes.Pop, OpCodes.Ldloc_0, OpCodes.Callvirt);
            Writer.Nop(); // Save loop exit label
            Writer.Remove(8);

            return Writer.Instructions;
        }
    }

    [HarmonyPatch(typeof(UIShowUsingBookInfoPanel))]
    internal class FakeBookDropsPatch
    {
        // Hide every book's drops and make "Burn and see;)" text visible
        [HarmonyPatch(nameof(UIShowUsingBookInfoPanel.ShowBookInfoData))]
        [HarmonyPrefix]
        static bool BurnAndSee(UIShowUsingBookInfoPanel __instance, DropBookXmlInfo dropBookInfo)
        {
            // Just the copied code with changes because i'm lazy :)
            __instance.gameObject.SetActive(value: true);
            __instance.xmlinfo = dropBookInfo;
            __instance.SetActivePanel(show: true);

            var xmlinfo = __instance.xmlinfo;
            if (xmlinfo == null)
                return false;

            __instance.currentDropBookSlot.SetData_DropBook(xmlinfo.id);
            __instance.txt_bookName.text = xmlinfo.Name;

            __instance.rewardItemList.SetItemsData(new List<UIRewardBookData>(), new List<UIRewardCardData>());
            __instance.SetColor(UIColorManager.Manager.GetUIColor(UIColor.Default));
            __instance.img_BookIcon.color = Color.white;
            __instance.img_BookIcon.sprite = xmlinfo.bookIcon;
            __instance.img_BookIconGlow.sprite = xmlinfo.bookIconGlow;

            // Make "Burn and see;)" text visible
            __instance.gameObject.transform.Find("BurnAndSee").gameObject.SetActive(true);
            
            return false;
        }

        // Hide "Burn and see;)" text
        [HarmonyPatch(nameof(UIShowUsingBookInfoPanel.SetEmptyInfo))]
        [HarmonyPrefix]
        static bool HideBurnAndSee(UIShowUsingBookInfoPanel __instance)
        {
            __instance.gameObject.transform.Find("BurnAndSee").gameObject.SetActive(false);

            return true;
        }
    }
}

using HarmonyLib;
using LORAP.Gameplay.Mechanics.Drops;
using System.Collections.Generic;
using System.Reflection.Emit;   
using UI;
using UnityEngine;

using static HarmonyLib.Code;

namespace LORAP.Patches
{
    internal class GachaPatches
    {
        // UIBattleResultPanel patch. Generate Gacha Drops and Send AP Checks when burning a book. //
        [HarmonyPatch(typeof(UIBookPanel), nameof(UIBookPanel.FeedBookTargetSephirah))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> GenerateDropsSendChecks(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeMatcher = new CodeMatcher(instructions, generator);

            codeMatcher.MatchStartForward(Ldloc_0, Callvirt, Pop)
                .RemoveInstructions(3)
                .MatchStartForward(Ldloc_1, Ldloc_0, Call, Ldloc_3)
                .RemoveInstructions(7)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DropsManager), nameof(DropsManager.GenerateDrops))))
                .MatchStartForward(Ldloc_0, Callvirt, Pop, Ldloc_0, Callvirt)
                .SetAndAdvance(Nop.opcode, null)
                .RemoveInstructions(8);

            return codeMatcher.Instructions();
        }



        // UIShowUsingBookInfoPanel patches. Add "Burn and see ;)" text to book burning menu. //
        // Hide every book's drops and make "Burn and see ;)" text visible
        [HarmonyPatch(typeof(UIShowUsingBookInfoPanel), nameof(UIShowUsingBookInfoPanel.ShowBookInfoData))]
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
        [HarmonyPatch(typeof(UIShowUsingBookInfoPanel), nameof(UIShowUsingBookInfoPanel.SetEmptyInfo))]
        [HarmonyPrefix]
        static bool HideBurnAndSee(UIShowUsingBookInfoPanel __instance)
        {
            __instance.gameObject.transform.Find("BurnAndSee").gameObject.SetActive(false);

            return true;
        }
    }
}

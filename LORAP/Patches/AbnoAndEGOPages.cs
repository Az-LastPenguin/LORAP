using HarmonyLib;
using LORAP.CustomUI;
using LORAP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LORAP.Patches
{
    internal class AbnoAndEGOPages
    {
        // Stop abno and ego page display window from appearing vanilla way (it's shown when getting abno and ego page items)
        [HarmonyPatch(typeof(UIGetAbnormalityPanel), nameof(UIGetAbnormalityPanel.SetData))]
        [HarmonyPrefix]
        static bool NoGetAbnoPanel() => false;



        // Instead of closing it vanilla way it's closed by my mod with additional stuff done
        [HarmonyPatch(typeof(UIGetAbnormalityPanel), nameof(UIGetAbnormalityPanel.PointerClickButton))]
        [HarmonyPrefix]
        static bool RedirectPanelClosure(UIGetAbnormalityPanel __instance)
        {
            // Try to show next message. If the queue is empty, it will just close
            AbnoEgoPagePopup.NextMessage();

            return false;
        }



        // Change the way game makes a list of abno pages when emotion levels up
        [HarmonyPatch(typeof(StageLibraryFloorModel), nameof(StageLibraryFloorModel.CreateSelectableList))]
        [HarmonyPrefix]
        static bool CustomRandomizeAbnoPages(StageLibraryFloorModel __instance, int emotionLevel, ref List<EmotionCardXmlInfo> __result)
        {
            int positiveTotal = __instance._unitList.Where(u => u.IsAddedBattle).Sum(u => u.emotionDetail.totalPositiveCoins.Count);
            int negativeTotal = __instance._unitList.Where(u => u.IsAddedBattle).Sum(u => u.emotionDetail.totalNegativeCoins.Count);

            LibraryFloorModel floor = LibraryModel.Instance.GetFloor(StageController.Instance.CurrentFloor);

            if (floor == null)
                return false;

            int floorLevel = floor.GetAbnoPageAmount() + 1;

            int num3 = 1;
            num3 = (emotionLevel <= 2) ? 1 : ((emotionLevel > 4) ? 3 : 2);

            // Copied and expanded EmotionCardXmlList.Instance.GetDataList()
            List<EmotionCardXmlInfo> data = EmotionCardXmlList.Instance._list.Where(c => c.Sephirah == StageController.Instance.CurrentFloor && c.Level <= floorLevel && c.EmotionLevel <= emotionLevel && !c.Locked).ToList();
            foreach (EmotionCardXmlInfo selected in __instance._selectedList)
            {
                data.Remove(selected);
            }

            // The simplified code
            int center = 0;
            float diff = ((positiveTotal + negativeTotal) > 0 ? (float)(positiveTotal - negativeTotal) / (float)(positiveTotal + negativeTotal) : 0.5f) / ((Math.Max(11f - emotionLevel, 1)) / 10f);
            if (Mathf.Abs(diff) < 0.1f)
            {
                center = 0;
            }
            else if (Mathf.Abs(diff) < 0.3f)
            {
                center = diff > 0 ? 1 : -1;
            }
            else
            {
                center = diff > 0 ? 2 : -2;
            }

            data.Sort((EmotionCardXmlInfo x, EmotionCardXmlInfo y) => Mathf.Abs(x.EmotionRate - center) - Mathf.Abs(y.EmotionRate - center));

            List<EmotionCardXmlInfo> list = new List<EmotionCardXmlInfo>();
            while (data.Count > 0 && list.Count < 3)
            {
                int ER = Mathf.Abs(data[0].EmotionRate - center);
                List<EmotionCardXmlInfo> list2 = data.FindAll((EmotionCardXmlInfo x) => Mathf.Abs(x.EmotionRate - center) == ER);

                if (list2.Count + list.Count <= 3)
                {
                    list.AddRange(list2);
                    foreach (EmotionCardXmlInfo item2 in list2)
                    {
                        data.Remove(item2);
                    }

                    continue;
                }
                int left = 3 - list.Count;
                for (int i = 0; i < left; i++)
                {
                    if (list2.Count == 0)
                    {
                        break;
                    }

                    EmotionCardXmlInfo item = RandomUtil.SelectOne(list2);
                    list2.Remove(item);
                    data.Remove(item);
                    list.Add(item);
                }
            }

            __result = list;

            return false;
        }

        // Custom ego page selection according to amount of currently owned pages
        [HarmonyPatch(typeof(StageLibraryFloorModel), nameof(StageLibraryFloorModel.RandomSelectEgo))]
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
            instr.Insert(pos + 2, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ClassExtensions), nameof(ClassExtensions.GetEGOAmount)))); // get number of ego from floor
            instr.Insert(pos + 3, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<EmotionEgoXmlInfo>), nameof(List<EmotionEgoXmlInfo>.GetRange)))); // egolist.GetRange()

            return instr;
        }



        // Hide name of the abno in the list of abno pages
        [HarmonyPatch(typeof(UIAbnormalityCategoryPanel), nameof(UIAbnormalityCategoryPanel.SetData))]
        [HarmonyPostfix]
        static void AbnoCardsName(UIAbnormalityCategoryPanel __instance) => __instance.txt_Title.text = "";



        // Display currently owned abno pages
        [HarmonyPatch(typeof(UIAbnormalityPanel), nameof(UIAbnormalityPanel.SetData))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> FloorAbnoList(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Change max i in the loop to current abno pages number
            var instr = instructions.ToList();
            var pos = instr.FindIndex(i => i.opcode == OpCodes.Sub) + 1;

            Label l = (Label)instr[pos].operand;
            pos -= 3;

            instr.RemoveRange(pos, 4);
            instr.Insert(pos, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ClassExtensions), nameof(ClassExtensions.GetAbnoPageAmount))));
            instr.Insert(pos + 1, new CodeInstruction(OpCodes.Blt_S, l));

            return instr;
        }



        // Display currently owned ego pages
        [HarmonyPatch(typeof(UIEgoCardPanel), nameof(UIEgoCardPanel.SetData))]
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
            instr.Insert(pos + 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ClassExtensions), nameof(ClassExtensions.GetEGOAmount)))); // get number of ego from floor

            return instr;
        }



        // Make game always show both abno and ego page panels
        [HarmonyPatch(typeof(UIFloorPanel), nameof(UIFloorPanel.OnUpdatePhase))]
        [HarmonyPostfix]
        static void FloorAbnoEGOButtons(UIFloorPanel __instance)
        {
            __instance.abnormalityEgoTap.SetActive(true);
            __instance.onlyAbnormalityTap.SetActive(false);
        }



        // Instead of closing it vanilla way it's closed by my mod with additional stuff done
        [HarmonyPatch(typeof(LevelUpUI), nameof(LevelUpUI.InitBase))]
        [HarmonyPrefix]
        static bool FixPickUnbound(LevelUpUI __instance, ref int selectedCount)
        {
            if (selectedCount > 4)
                selectedCount = 4;

            return true;
        }



        // When hovering over abno page in unit info, make it higher in render (sorting) order
        [HarmonyPatch(typeof(EmotionPassiveCardUI), nameof(EmotionPassiveCardUI.OnPointerEnter))]
        [HarmonyPostfix]
        static void AbnoPageChangeOrderEnter(EmotionPassiveCardUI __instance, PointerEventData eventData)
        {
            Canvas canvas = __instance.gameObject.GetComponent<Canvas>();

            if (canvas == null)
                return;

            canvas.overrideSorting = true;
            canvas.sortingOrder = 1400;
        }

        // When hovering exiting over abno page in unit info, make it lower in render (sorting) order
        [HarmonyPatch(typeof(EmotionPassiveCardUI), nameof(EmotionPassiveCardUI.OnPointerExit))]
        [HarmonyPostfix]
        static void AbnoPageChangeOrderExit(EmotionPassiveCardUI __instance, PointerEventData eventData)
        {
            Canvas canvas = __instance.gameObject.GetComponent<Canvas>();

            if (canvas == null)
                return;

            canvas.overrideSorting = false;
            //canvas.sortingOrder = 1301;
        }
    }
}

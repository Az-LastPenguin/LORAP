using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using LORAP.CustomUI;
using LORAP.Playthru;
using UI;
using UnityEngine;

using static HarmonyLib.Code;

namespace LORAP.Patches
{
    internal class AbnoAndEGOPages
    {
        // UIGetAbnormalityPanel patches. For modified abno and ego page display. //
        // Stop abno and ego page display window from appearing vanilla way (it's shown when getting abno and ego page items)
        [HarmonyPatch(typeof(UIGetAbnormalityPanel), nameof(UIGetAbnormalityPanel.SetData))]
        [HarmonyPrefix]
        static bool NoGetAbnoPanel() => false;

        // Instead of closing it vanilla way it's closed by my mod with additional stuff done
        [HarmonyPatch(typeof(UIGetAbnormalityPanel), nameof(UIGetAbnormalityPanel.PointerClickButton))]
        [HarmonyPrefix]
        static bool RedirectPanelClosure(UIGetAbnormalityPanel __instance)
        {
            MessagePopup.PagesClose();

            return false;
        }



        // StageLibraryFloorModel patches. For modified abno and ego pages selection. //
        // Due to PM quantum coding, i'm replacing this method with itself with few changes. Mostly to show abno pages of the level you have
        [HarmonyPatch(typeof(StageLibraryFloorModel), nameof(StageLibraryFloorModel.CreateSelectableList))]
        [HarmonyPrefix]
        static bool CustomRandomizeAbnoPages(StageLibraryFloorModel __instance, ref List<EmotionCardXmlInfo> __result, int emotionLevel)
        {
            int posivtiveTotal = 0;
            int negativeTotal = 0;
            foreach (UnitBattleDataModel unit in __instance._unitList)
            {
                if (unit.IsAddedBattle)
                {
                    posivtiveTotal += unit.emotionDetail.totalPositiveCoins.Count;
                    negativeTotal += unit.emotionDetail.totalNegativeCoins.Count;
                }
            }

            int floorLevel = 0;
            LibraryFloorModel floor = LibraryModel.Instance.GetFloor(Singleton<StageController>.Instance.CurrentFloor);
            if (floor != null)
            {
                // The difference
                floorLevel = floor.GetAbnoPageAmount() + 1;
            }

            int num3 = 1;
            num3 = (emotionLevel <= 2) ? 1 : ((emotionLevel > 4) ? 3 : 2);

            List<EmotionCardXmlInfo> dataList = Singleton<EmotionCardXmlList>.Instance.GetDataList(Singleton<StageController>.Instance.CurrentFloor, floorLevel, num3);
            foreach (EmotionCardXmlInfo selected in __instance._selectedList)
            {
                dataList.Remove(selected);
            }

            // The simplified code
            int center = 0;
            float diff = ((posivtiveTotal + negativeTotal) > 0 ? (float)(posivtiveTotal - negativeTotal) / (float)(posivtiveTotal + negativeTotal) : 0.5f) / ((11f - emotionLevel) / 10f);
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

            dataList.Sort((EmotionCardXmlInfo x, EmotionCardXmlInfo y) => Mathf.Abs(x.EmotionRate - center) - Mathf.Abs(y.EmotionRate - center));

            List<EmotionCardXmlInfo> list = new List<EmotionCardXmlInfo>();
            while (dataList.Count > 0 && list.Count < 3)
            {
                int ER = Mathf.Abs(dataList[0].EmotionRate - center);
                List<EmotionCardXmlInfo> list2 = dataList.FindAll((EmotionCardXmlInfo x) => Mathf.Abs(x.EmotionRate - center) == ER);

                if (list2.Count + list.Count <= 3)
                {
                    list.AddRange(list2);
                    foreach (EmotionCardXmlInfo item2 in list2)
                    {
                        dataList.Remove(item2);
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
                    dataList.Remove(item);
                    list.Add(item);
                }
            }

            __result = list;

            return false;
        }

        // Select random Abno Page
        /*[HarmonyPatch("RandomSelect")]
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
        }*/

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
            instr.Insert(pos + 2, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(LORClassExtensions), nameof(LORClassExtensions.GetEGOAmount)))); // get number of ego from floor
            instr.Insert(pos + 3, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<EmotionEgoXmlInfo>), nameof(List<EmotionEgoXmlInfo>.GetRange)))); // egolist.GetRange()

            return instr;
        }



        // UIAbnormalityCategoryPanel patch. Hide name of the abno since abno pages are from different abnos 99.9% of the time. //
        [HarmonyPatch(typeof(UIAbnormalityCategoryPanel), nameof(UIAbnormalityCategoryPanel.SetData))]
        [HarmonyPostfix]
        static void AbnoCardsName(UIAbnormalityCategoryPanel __instance) => __instance.txt_Title.text = "";



        // UIAbnormalityPanel patch. Display currently owned abno pages. //
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
            instr.Insert(pos, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(LORClassExtensions), nameof(LORClassExtensions.GetAbnoPageAmount))));
            instr.Insert(pos + 1, new CodeInstruction(OpCodes.Blt_S, l));

            return instr;
        }



        // UIEgoCardPanel patch. Display currently owned ego pages. //
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
            instr.Insert(pos + 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(LORClassExtensions), nameof(LORClassExtensions.GetEGOAmount)))); // get number of ego from floor

            return instr;
        }



        // UIFloorPanel patch. Make game always show both abno and ego page panels. //
        [HarmonyPatch(typeof(UIFloorPanel), nameof(UIFloorPanel.OnUpdatePhase))]
        [HarmonyPostfix]
        static void FloorAbnoEGOButtons(UIFloorPanel __instance)
        {
            __instance.abnormalityEgoTap.SetActive(true);
            __instance.onlyAbnormalityTap.SetActive(false);
        }
    }
}

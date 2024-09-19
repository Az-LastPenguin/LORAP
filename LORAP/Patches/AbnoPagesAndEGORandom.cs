using HarmonyLib;
using LOR_DiceSystem;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace LORAP.Patches
{

    [HarmonyPatch(typeof(StageLibraryFloorModel))]
    internal class ModifiedSlection
    {
        // To only show Abno And EGO pages that player has. Next two patches are related.
        // Copy of StageLibraryFloorModel.CreateSelectableList, except for few things
        [HarmonyPatch("CreateSelectableList")]
        [HarmonyPrefix]
        static bool CustomPagesSelectableList(StageLibraryFloorModel __instance, int emotionLevel, ref List<EmotionCardXmlInfo> __result)
        {
            int num = 0;
            int num2 = 0;
            foreach (UnitBattleDataModel unit in Traverse.Create(__instance).Field<List<UnitBattleDataModel>>("_unitList").Value)
            {
                if (unit.IsAddedBattle)
                {
                    num += unit.emotionDetail.totalPositiveCoins.Count;
                    num2 += unit.emotionDetail.totalNegativeCoins.Count;
                }
            }

            LibraryFloorModel floor = LibraryModel.Instance.GetFloor(Singleton<StageController>.Instance.CurrentFloor);
            int num3 = 1;
            num3 = ((emotionLevel <= 2) ? 1 : ((emotionLevel > 4) ? 3 : 2));
            List<EmotionCardXmlInfo> dataList = Singleton<EmotionCardXmlList>.Instance.GetDataList(Singleton<StageController>.Instance.CurrentFloor, APPlaythruManager.AbnoPageAmounts[floor.Sephirah] + 1, num3);
            foreach (EmotionCardXmlInfo selected in Traverse.Create(__instance).Field<List<EmotionCardXmlInfo>>("_selectedList").Value)
            {
                dataList.Remove(selected);
            }
            int center = 0;
            int num4 = num + num2;
            float num5 = 0.5f;
            if (num4 > 0)
            {
                num5 = (float)(num - num2) / (float)num4;
            }
            float num6 = num5 / ((11f - (float)emotionLevel) / 10f);
            if ((double)Mathf.Abs(num6) < 0.1)
            {
                center = 0;
            }
            else if ((double)Mathf.Abs(num6) < 0.3)
            {
                if (num6 > 0f)
                {
                    center = 1;
                }
                else
                {
                    center = -1;
                }
            }
            else if (num6 > 0f)
            {
                center = 2;
            }
            else
            {
                center = -2;
            }
            dataList.Sort((EmotionCardXmlInfo x, EmotionCardXmlInfo y) => Mathf.Abs(x.EmotionRate - center) - Mathf.Abs(y.EmotionRate - center));
            List<EmotionCardXmlInfo> list = new List<EmotionCardXmlInfo>();
            new List<EmotionCardXmlInfo>();
            new List<EmotionCardXmlInfo>();
            new List<EmotionCardXmlInfo>();
            while (dataList.Count > 0 && list.Count < 3)
            {
                int er = Mathf.Abs(dataList[0].EmotionRate - center);
                List<EmotionCardXmlInfo> list2 = dataList.FindAll((EmotionCardXmlInfo x) => Mathf.Abs(x.EmotionRate - center) == er);
                if (list2.Count + list.Count <= 3)
                {
                    list.AddRange(list2);
                    foreach (EmotionCardXmlInfo item2 in list2)
                    {
                        dataList.Remove(item2);
                    }
                    continue;
                }
                int num7 = 3 - list.Count;
                for (int i = 0; i < num7; i++)
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

        // Copy of StageLibraryFloorModel.RandomSelect, except for few things
        [HarmonyPatch("RandomSelect")]
        [HarmonyPrefix]
        static bool CustomAbnoPagesRandom(StageLibraryFloorModel __instance, List<EmotionCardXmlInfo> duplicated, ref EmotionCardXmlInfo __result)
        {
            List<EmotionCardXmlInfo> list = new List<EmotionCardXmlInfo>();
            LibraryFloorModel floor = LibraryModel.Instance.GetFloor(Singleton<StageController>.Instance.CurrentFloor);
            list = Singleton<EmotionCardXmlList>.Instance.GetDataList(Singleton<StageController>.Instance.CurrentFloor, APPlaythruManager.AbnoPageAmounts[floor.Sephirah] + 1, 1);
            foreach (EmotionCardXmlInfo item in duplicated)
            {
                list.Remove(item);
            }
            if (list.Count > 0)
            {
                __result = RandomUtil.SelectOne(list);
            }
            __result = null;

            return false;
        }

        // Copy of StageLibraryFloorModel.RandomSelectEgo, except for few things
        [HarmonyPatch("RandomSelectEgo")]
        [HarmonyPrefix]
        static bool CustomEGOPagesRandom(StageLibraryFloorModel __instance, List<EmotionEgoXmlInfo> duplicated, ref EmotionEgoXmlInfo __result)
        {
            List<EmotionEgoXmlInfo> list = new List<EmotionEgoXmlInfo>();
            LibraryFloorModel floor = LibraryModel.Instance.GetFloor(Singleton<StageController>.Instance.CurrentFloor);
            foreach (EmotionEgoXmlInfo data in Singleton<EmotionEgoXmlList>.Instance.GetDataList(Singleton<StageController>.Instance.CurrentFloor).GetRange(0, APPlaythruManager.EGOAmounts[floor.Sephirah]))
            {
                if (!data.isLock)
                {
                    list.Add(data);
                }
            }
            foreach (EmotionEgoXmlInfo item in duplicated)
            {
                list.Remove(item);
            }

            LORAP.Log(list.Count());

            if (list.Count > 0)
            {
                __result = RandomUtil.SelectOne(list);

                return false;
            }
            __result = null;

            return false;
        }
    }

    [HarmonyPatch(typeof(UIAbnormalityPanel))]
    internal class AbnoCardsList
    {
        // Show Abno and EGO Pages player has regardless of the in-game library level. Next two patches are related
        [HarmonyPatch(nameof(UIAbnormalityPanel.SetData))]
        [HarmonyPrefix]
        static bool FloorAbnoList(UIAbnormalityPanel __instance, LibraryFloorModel floor)
        {
            Traverse.Create(__instance).Field<UIEmotionPassiveCardInven>("previewCard").Value.SetActiveDetail(on: false);
            var AbCategoryPanel = Traverse.Create(__instance).Field<List<UIAbnormalityCategoryPanel>>("AbCategoryPanel").Value;

            int i;
            for (i = 0; i < floor.GetEmotionCardAmount(); i++)
            {
                if (AbCategoryPanel.Count <= i)
                {
                    return false;
                }
                List<EmotionCardXmlInfo> dataListByLevel = Singleton<EmotionCardXmlList>.Instance.GetDataListByLevel(floor.Sephirah, i + 2);
                AbCategoryPanel[i].SetData(dataListByLevel, i, __instance);
            }
            for (; i < AbCategoryPanel.Count; i++)
            {
                AbCategoryPanel[i].SetLock();
            }

            Traverse.Create(__instance).Field<ScrollRect>("scroll").Value.normalizedPosition = new Vector2(0f, 1f);
            __instance.OnChangedScrollView();

            return false;
        }
    }

    [HarmonyPatch(typeof(UIEgoCardPanel))]
    internal class EGOCardsList
    {
        [HarmonyPatch(nameof(UIEgoCardPanel.SetData))]
        [HarmonyPrefix]
        static bool FloorEGOList(UIEgoCardPanel __instance, LibraryFloorModel floor)
        {
            List<DiceCardXmlInfo> egoCardList = Singleton<EmotionEgoXmlList>.Instance.GetEgoCardList(floor.Sephirah);

            var slotList = Traverse.Create(__instance).Field<List<UIEgoCardPreviewSlot>>("slotList").Value;

            int i;
            for (i = 0; i < floor.GetEGOAmount(); i++)
            {
                if (slotList.Count > i)
                {
                    slotList[i].Init(new DiceCardItemModel(egoCardList[i]));
                }
            }
            for (; i < slotList.Count; i++)
            {
                slotList[i].Init(null);
            }

            __instance.HideDetailSlotByInventory();

            return false;
        }
    }

    [HarmonyPatch(typeof(UIFloorPanel))]
    internal class AbnoEGOButtons
    {
        [HarmonyPatch(nameof(UIFloorPanel.OnUpdatePhase))]
        [HarmonyPostfix]
        static void FloorAbnoEGOButtons(UIFloorPanel __instance)
        {
            Traverse.Create(__instance).Field<GameObject>("abnormalityEgoTap").Value.SetActive(value: true);
            Traverse.Create(__instance).Field<GameObject>("onlyAbnormalityTap").Value.SetActive(value: false);
        }
    }
}

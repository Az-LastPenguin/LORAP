using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace LORAP.Patches
{
    [HarmonyPatch(typeof(StageClassInfo))]
    internal class ClosedReceptionsPatch
    {
        internal static List<int> shownIds = new List<int>();

        // Block receptions that are not unlocked for the player
        [HarmonyPatch(nameof(StageClassInfo.currentState), MethodType.Getter)]
        [HarmonyPrefix]
        static bool BlockReceptions(StageClassInfo __instance, ref StoryState __result)
        {
            __result = StoryState.Close;

            if ((__instance.chapter == 1 || __instance.chapter == 2) && __instance.storyType != "Chapter2")
                __result = StoryState.Clear;

            if (shownIds.Contains(__instance.id.id))
                __result = StoryState.Clear;

            if (APPlaythruManager.IsReceptionOpened(__instance.id.id))
                __result = StoryState.Clear;

            return false;
        }
    }

    [HarmonyPatch(typeof(UIStoryProgressPanel))]
    internal class HideReceptionsPatch
    {
        [HarmonyPatch("SetStoryLine")]
        [HarmonyPrefix]
        static bool HideReceptions(UIStoryProgressPanel __instance)
        {
            __instance.currentSlot = null;
            StoryTotal.instance.SetData();
            foreach (GameObject chapter in Traverse.Create(__instance).Field<List<GameObject>>("chapterList").Value)
            {
                chapter.SetActive(true);
            }

            foreach (var block in Traverse.Create(__instance).Field<List<UIBlockChapterAlarm>>("blockChapterList").Value)
            {
                block.root.gameObject.SetActive(false);
            }

            foreach (var icon in Traverse.Create(__instance).Field<List<UIStoryProgressIconSlot>>("iconList").Value)
            {
                StoryLineData storyLineData = StoryTotal.instance._lineList.Find((StoryLineData x) => x.currentstory == icon.currentStory);
                List<StageClassInfo> story = storyLineData != null ? storyLineData.stageList : Traverse.Create(icon).Field<List<StageClassInfo>>("storyData").Value;
                icon.SetSlotData(story);
                icon.SetActiveStory(true);

                List<int> ensembleIds = new List<int>() { 70001, 70002, 70003, 70004, 70005, 70006, 70007, 70008, 70009, 70010 };
                if (ensembleIds.Contains(story[0].id.id))
                {
                    typeof(UIStoryProgressIconSlot).GetMethod("SetIcon", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(icon, new object[] { UISpriteDataManager.instance._floorIconSet[story[0].id.id-70000] });
                }

                List<int> hiddenIds = new List<int>() {610000, 60007};
                if (hiddenIds.Contains(story[0].id.id))
                    icon.SetActiveStory(false);
            }

            foreach (UIStoryProgressIconSlot chapterIcon in Traverse.Create(__instance).Field<List<UIStoryProgressIconSlot>>("chapterIconList").Value)
            {
                chapterIcon.SetChapterStoryIcon();
                chapterIcon.SetChapterStoryIconDefault();
            }

            foreach (var c in Traverse.Create(__instance).Field("chapterIconList").GetValue() as List<UIStoryProgressIconSlot>)
            {
                Traverse.Create(c).Field("isDisabled").SetValue(true);
                c.GetComponentInChildren<UICustomSelectable>().interactable = false;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(UIStoryProgressIconSlot))]
    internal class CheckmarkPatch
    {
        // Add a checkmark to completed receptions
        [HarmonyPatch("SetSlotOpen")]
        [HarmonyPrefix]
        static void Checkmark(UIStoryProgressIconSlot __instance, bool open)
        {
            if (__instance.transform.Find("Checkmark") == null)
                return;

            StoryLineData storyLineData = StoryTotal.instance._lineList.Find((StoryLineData x) => x.currentstory == __instance.currentStory);
            List<StageClassInfo> story = storyLineData != null ? storyLineData.stageList : Traverse.Create(__instance).Field<List<StageClassInfo>>("storyData").Value;

            // List of not yet found books
            List<LorId> list = new List<LorId>();
            foreach (StageClassInfo stage in story)
            {
                if (stage.id == 40008)
                {
                    list.Add(new LorId(240023));
                }

                foreach (StageWaveInfo wave in stage.waveList)
                {
                    foreach (LorId enemyUnitId in wave.enemyUnitIdList)
                    {
                        EnemyUnitClassInfo data = Singleton<EnemyUnitClassInfoList>.Instance.GetData(enemyUnitId);
                        foreach (EnemyDropItemTable dropTable in data.dropTableList)
                        {
                            foreach (EnemyDropItem dropItem in dropTable.dropItemList)
                            {
                                LorId item = new LorId(data.workshopID, dropItem.bookId);
                                if (!list.Contains(item) && (!ItemLocationManager.BookIds.Contains(item.id) || !APPlaythruManager.FoundBooks.Contains(item.id)))
                                {
                                    list.Add(item);
                                }
                            }
                        }
                    }
                }
            }


            // Other requirements
            List<int> ensembleIds = new List<int>() { 70001, 70002, 70003, 70004, 70005, 70006, 70007, 70008, 70009, 70010 };
            bool cleared = true;
            if (ClosedReceptionsPatch.shownIds.Contains(__instance._storyData.First().id.id) && !APPlaythruManager.EndGameBattlesBeaten.Contains(__instance._storyData.First().id.id))
                cleared = false;


            if (list.Count == 0 && cleared)
                __instance.transform.Find("Checkmark").gameObject.SetActive(true);
            else
                __instance.transform.Find("Checkmark").gameObject.SetActive(false);
        }

        // Fix opening Black Silence and Distorted Ensemble receptions
        [HarmonyPatch("ClickMainIcon")]
        [HarmonyPrefix]
        static bool EndReceptionsPatch(UIStoryProgressIconSlot __instance)
        {
            if (!new List<int>() { 60003, 60004 }.Contains(__instance._storyData.First().id.id)) return true;

            if (ClosedReceptionsPatch.shownIds.Contains(__instance._storyData.First().id.id)) return true;

            return false;
        }

        // Custom icon highlight for Ensemble
        [HarmonyPatch("SetHighlighted")]
        [HarmonyPrefix]
        static bool EnsembleIcons(UIStoryProgressIconSlot __instance, bool on)
        {
            Dictionary<UIStoryLine, Color> CustomDefaultColors = new Dictionary<UIStoryLine, Color>()
            {
                [(UIStoryLine)151] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)152] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)153] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)154] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)155] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)156] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)157] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)158] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)159] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)160] = new Color(0.8f, 0.8f, 0.8f, 1),
            };

            Dictionary<UIStoryLine, Color> CustomHighlightColors = new Dictionary<UIStoryLine, Color>()
            {
                [(UIStoryLine)151] = new Color(1, 1, 1, 1),
                [(UIStoryLine)152] = new Color(1, 1, 1, 1),
                [(UIStoryLine)153] = new Color(1, 1, 1, 1),
                [(UIStoryLine)154] = new Color(1, 1, 1, 1),
                [(UIStoryLine)155] = new Color(1, 1, 1, 1),
                [(UIStoryLine)156] = new Color(1, 1, 1, 1),
                [(UIStoryLine)157] = new Color(1, 1, 1, 1),
                [(UIStoryLine)158] = new Color(1, 1, 1, 1),
                [(UIStoryLine)159] = new Color(1, 1, 1, 1),
                [(UIStoryLine)160] = new Color(1, 1, 1, 1),
            };

            if (!CustomHighlightColors.ContainsKey(__instance.currentStory)) return true;

            var isChapterIcon = Traverse.Create(__instance).Field<bool>("isChapterIcon").Value;
            var originalcolor = Traverse.Create(__instance).Field<Color>("originalcolor").Value;

            var highlightColor = CustomHighlightColors[__instance.currentStory];
            var defaultColor = CustomDefaultColors[__instance.currentStory];


            Color color = ((!isChapterIcon) ? originalcolor : (on ? highlightColor : defaultColor));
            Color color2 = (on ? highlightColor : UIColorManager.Manager.DefaultGlowColor);

            __instance.transform.Find("[Rect]Close/[Rect]Icon/[Image]Icon_content").gameObject.GetComponent<Image>().color = color;
            __instance.transform.Find("[Rect]Close/[Rect]Icon/[Image]Icon_bg").gameObject.GetComponent<Image>().color = color2;
            __instance.transform.Find("[Rect]Close/[Rect]Icon/[Image]Icon_Frame").gameObject.GetComponent<Image>().color = color2;

            __instance.transform.Find("[Rect]Open/[Rect]OpenIcon/[Image]Icon_content").gameObject.GetComponent<Image>().color = (isChapterIcon ? defaultColor : originalcolor);
            __instance.transform.Find("[Rect]Open/[Rect]OpenIcon/[Image]Icon_bg").gameObject.GetComponent<Image>().color = UIColorManager.Manager.DefaultGlowColor;

            return false;
        }
    }
}

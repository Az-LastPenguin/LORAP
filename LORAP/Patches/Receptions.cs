using HarmonyLib;
using LORAP.Playthru;
using LORAP.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace LORAP.Patches
{
    [HarmonyPatch(typeof(StageClassInfo))]
    internal class ClosedReceptionsPatch
    {
        // Block receptions that are not unlocked for the player
        [HarmonyPatch(nameof(StageClassInfo.currentState), MethodType.Getter)]
        [HarmonyPrefix]
        static bool BlockReceptions(StageClassInfo __instance, ref StoryState __result)
        {
            __result = StoryState.Close;

            if (PlaythruManager.IsReceptionOpened(__instance.id.id))
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
            StoryTotal.instance.SetData(); // What's it for?
            __instance.chapterList.ForEach(c => c.SetActive(true)); // Show all chapters and receptions
            __instance.blockChapterList.ForEach(b => b.root.gameObject.SetActive(false)); // Hide all chapter block things

            List<int> ensembleIds = new List<int>() { 70001, 70002, 70003, 70004, 70005, 70006, 70007, 70008, 70009, 70010 };
            List<int> hideIDs = new List<int>() { 610000, 60007 };
            foreach (var icon in __instance.iconList) // Set all receptions info and icons
            {
                List<StageClassInfo> storyData = StoryTotal.instance._lineList.Find((StoryLineData x) => x.currentstory == icon.currentStory)?.stageList ?? icon.storyData;
                icon.SetSlotData(storyData);

                if (hideIDs.Contains(storyData[0]._id)) // Hide some receptions
                    icon.SetActiveStory(false);
                else
                    icon.SetActiveStory(true);

                if (ensembleIds.Contains(storyData[0]._id)) // Set custom ensemble icons
                {
                    if (storyData[0].currentState == StoryState.Clear)
                        icon.SetIcon(UISpriteDataManager.instance._floorIconSet[storyData[0]._id - 70000]);
                    else
                        icon.SetIcon(UISpriteDataManager.instance._questionicon[1]);
                }
                    

                // Show checkmark for the completed receptions that have every book collected
                if (icon.transform.Find("Checkmark") == null)
                    continue;

                var notFound = storyData.SelectMany(s => s.waveList).SelectMany(w => w.enemyUnitIdList).SelectMany(u => EnemyUnitClassInfoList.Instance.GetData(u).dropTableList).SelectMany(t => t.dropItemList).Where(i => !PlaythruManager.FoundBooks.Contains(i.bookId)).Count();

                if (PlaythruManager.ReceptionsCompleted.Contains(storyData[0]._id) && notFound == 0)
                    icon.transform.Find("Checkmark").gameObject.SetActive(true);
            }

            foreach (UIStoryProgressIconSlot chapterIcon in __instance.chapterIconList) // Make chapter buttons not interactable (and some other default stuff)
            {
                chapterIcon.SetChapterStoryIcon();
                chapterIcon.SetChapterStoryIconDefault();
                chapterIcon.enabled = false;
                chapterIcon.isDisabled = true;
                chapterIcon.transform.Find("[Rect]ChapterTitle/[Rect]Close (1)/[Xbox]SelectableTarget").gameObject.GetComponent<UICustomSelectable>().interactable = false;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(UIStoryProgressIconSlot))]
    internal class CheckmarkPatch
    {
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

            var isChapterIcon = __instance.isChapterIcon;
            var originalcolor = __instance.originalcolor;

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

        // Fix opening Black Silence and Distorted Ensemble receptions
        [HarmonyPatch("ClickMainIcon")]
        [HarmonyPrefix]
        static bool EndReceptionsPatch(UIStoryProgressIconSlot __instance)
        {
            if (!new List<int>() { 60003, 60004 }.Contains(__instance._storyData.First().id.id)) return true;

            if (PlaythruManager.IsReceptionOpened(__instance._storyData.First().id.id)) return true;

            return false;
        }
    }

    [HarmonyPatch(typeof(StageController))]
    internal class BonusRewardRemovePatch
    {
        [HarmonyPatch(nameof(StageController.EndBattlePhase_invitation))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> BonusRewardRemove(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Remove additional page rewards from the receptions because those rewards are now in the book of everything
            CIWriter Writer = new CIWriter(instructions, generator);

            // Remove part with giving additional pages for endgame content stuff
            Writer.ToPattern(OpCodes.Ldloc_0, OpCodes.Callvirt, OpCodes.Ldfld, OpCodes.Callvirt, OpCodes.Stloc_S);
            Writer.Remove(169);

            // Remove part with giving pages for ending the game
            Writer.ToPattern(OpCodes.Call, OpCodes.Callvirt, OpCodes.Callvirt, OpCodes.Stloc_S, OpCodes.Br);
            Writer.Remove(175);
            
            return Writer.Instructions;
        }
    }
}

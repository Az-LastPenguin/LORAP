using HarmonyLib;
using StoryScene;
using UI;
using UnityEngine;

namespace LORAP.Patches
{
    [HarmonyPatch(typeof(StageController))]
    internal class BattleStoryPatches
    {
        // Those are self explainatory
        [HarmonyPatch("CheckStoryAfterBattle")]
        [HarmonyPrefix]
        static bool RemoveAfterBattleStory(StageController __instance)
        {
            __instance.CloseBattleScene();

            return false;
        }

        [HarmonyPatch("CheckStoryBeforeBattle")]
        [HarmonyPrefix]
        static bool RemovePreBattleStory(StageController __instance, ref bool __result)
        {
            __result = false;

            return false;
        }
    }

    [HarmonyPatch(typeof(UIMenuItem))]
    internal class CredenzaPatches1
    {
        // Disable access to Credenza
        [HarmonyPatch(nameof(UIMenuItem.SetTargetReveal))]
        [HarmonyPrefix]
        static bool CredenzaMenuItemReveal(UIMenuItem __instance)
        {
            if (__instance.TapState == UIMainMenuTap.Story)
            {
                __instance.anim.SetTrigger("Reveal");
                return false;
            }

            return true;
        }

        [HarmonyPatch(nameof(UIMenuItem.SetTargetHide))]
        [HarmonyPrefix]
        static bool CredenzaMenuItemHide(UIMenuItem __instance)
        {
            if (__instance.TapState == UIMainMenuTap.Story)
            {
                __instance.anim.SetTrigger("Hide");
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(UIControlButtonPanel))]
    internal class CredenzaPatches2
    {
        // Disable access to Credenza
        [HarmonyPatch(nameof(UIControlButtonPanel.UpdateButtons))]
        [HarmonyPostfix]
        static void CredenzaMenuItemBlock(UIControlButtonPanel __instance)
        {
            var item = __instance.menuItems.Find(i => i.TapState == UIMainMenuTap.Story);

            item.SetDisabled();
            item.SetTargetHide();
            item.SetActiveOrigin(false);
            item.isDisabled = true;
        }
    }

    [HarmonyPatch(typeof(LibraryModel))]
    internal class SmallTalkPatches 
    {
        // Additional patch so that game does not try to load some story cutscenes related to small talk in the library
        [HarmonyPatch(nameof(LibraryModel.GetEpNumberTalkStory))]
        [HarmonyPrefix]
        static bool TalkStoryEpisodePatch(LibraryModel __instance, ref int __result)
        {
            __result = 0;

            return false;
        }
    }

    [HarmonyPatch(typeof(UI.UIController))]
    internal class ForceStoryEndPatch
    {
        // Skip Story if it should ever appear and has an end func
        [HarmonyPatch("OpenStory", typeof(StageStoryInfo), typeof(StoryRoot.OnEndStoryFunc), typeof(bool), typeof(bool), typeof(bool))]
        [HarmonyPrefix]
        static bool OpenStoryPatch(UI.UIController __instance, StoryRoot.OnEndStoryFunc endFunc)
        {
            endFunc();

            return false;
        }
    }

    [HarmonyPatch(typeof(UIInvitationInfoPanel))]
    internal class StoryRecallPatch
    {
        // Hide Recall Story Button in the invitation screen
        [HarmonyPatch("Initialized")]
        [HarmonyPostfix]
        static void RecallStoryButton(UIInvitationInfoPanel __instance)
        {
            __instance.transform.Find("[Script]EnemyStageInfoPanel/[Root]ShowStoryPanel").gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(UIAlarmPopup))]
    internal class EndContentPatch
    {
        // Remove story from end content (Literally does not work??????)
        [HarmonyPatch(nameof(UIAlarmPopup.StartEndContentsStage))]
        [HarmonyPrefix]
        static void EndContents(UIAlarmPopup __instance, EndContentsStageId id, ref bool showstory, bool save, bool inv, bool ignoreprepare)
        {
            showstory = false;
        }
    }
}

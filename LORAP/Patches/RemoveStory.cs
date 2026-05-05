using HarmonyLib;
using StoryScene;
using UI;
using UnityEngine;

namespace LORAP.Patches
{
    internal class RemoveStory
    {
        // StageController patches. Don't show story before or after the battle. //
        [HarmonyPatch(typeof(StageController), nameof(StageController.CheckStoryAfterBattle))]
        [HarmonyPrefix]
        static bool RemoveAfterBattleStory(StageController __instance)
        {
            __instance.CloseBattleScene();

            return false;
        }

        [HarmonyPatch(typeof(StageController), nameof(StageController.CheckStoryBeforeBattle))]
        [HarmonyPrefix]
        static bool RemovePreBattleStory(StageController __instance, ref bool __result)
        {
            __result = false;

            return false;
        }



        // UIMenuItem patches. Disable access to credenza. //
        // Disable access to Credenza
        [HarmonyPatch(typeof(UIMenuItem), nameof(UIMenuItem.SetTargetReveal))]
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

        [HarmonyPatch(typeof(UIMenuItem), nameof(UIMenuItem.SetTargetHide))]
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



        // UIControlButtonPanel patch. Also disable access to credenza. //
        [HarmonyPatch(typeof(UIControlButtonPanel), nameof(UIControlButtonPanel.UpdateButtons))]
        [HarmonyPostfix]
        static void CredenzaMenuItemBlock(UIControlButtonPanel __instance)
        {
            var item = __instance.menuItems.Find(i => i.TapState == UIMainMenuTap.Story);

            item.SetDisabled();
            item.SetTargetHide();
            item.SetActiveOrigin(false);
            item.isDisabled = true;
        }



        // LibraryModel patch. Don't load small story episodes. //
        [HarmonyPatch(typeof(LibraryModel), nameof(LibraryModel.GetEpNumberTalkStory))]
        [HarmonyPrefix]
        static bool TalkStoryEpisodePatch(LibraryModel __instance, ref int __result)
        {
            __result = 0;

            return false;
        }



        // UIController patch. If a story is SOMEHOW tryin to load, skip it immediately. //
        [HarmonyPatch(typeof(UI.UIController), nameof(UI.UIController.OpenStory), typeof(StageStoryInfo), typeof(StoryRoot.OnEndStoryFunc), typeof(bool), typeof(bool), typeof(bool))]
        [HarmonyPrefix]
        static bool OpenStoryPatch(UI.UIController __instance, StoryRoot.OnEndStoryFunc endFunc)
        {
            endFunc();

            return false;
        }



        // UIInvitationInfoPanel patch. Hide show story button in reception description. //
        [HarmonyPatch(typeof(UIInvitationInfoPanel), nameof(UIInvitationInfoPanel.Initialized))]
        [HarmonyPostfix]
        static void RecallStoryButton(UIInvitationInfoPanel __instance)
        {
            __instance.transform.Find("[Script]EnemyStageInfoPanel/[Root]ShowStoryPanel").gameObject.SetActive(false);
        }



        // UIAlarmPopup patch. Remove story from end content (Literally does not work??????). //
        [HarmonyPatch(typeof(UIAlarmPopup), nameof(UIAlarmPopup.StartEndContentsStage))]
        [HarmonyPrefix]
        static void EndContents(UIAlarmPopup __instance, EndContentsStageId id, ref bool showstory, bool save, bool inv, bool ignoreprepare)
        {
            showstory = false;
        }
    }
}

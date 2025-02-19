using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UI;

namespace LORAP.Patches
{
    [HarmonyPatch(typeof(UIInvitationRightMainPanel))]
    internal class RemoveInvitationBookReq
    {
        // Hide Workshop checkbox (Can't set custom recipes anyway)
        [HarmonyPatch(nameof(UIInvitationRightMainPanel.OpenInit))]
        [HarmonyPostfix]
        static void RedUI(UIInvitationRightMainPanel __instance)
        {
            __instance.ob_customMode.gameObject.SetActive(false);
        }

        // Set the UI Red as if all the needed books are selected, also make books unable to be selected
        [HarmonyPatch(nameof(UIInvitationRightMainPanel.SetInvBookApplyState))]
        [HarmonyPrefix]
        static bool RedUI(UIInvitationRightMainPanel __instance, ref InvitationApply_State state)
        {
            if (state == InvitationApply_State.Normal || state == InvitationApply_State.Fixed)
            {
                __instance.currentinvState = state;
                __instance.SetActiveEndEffect(on: false);
                __instance.invitationbookSlots.ForEach(s => s.SetDisabledSlot());
                __instance.SetUpdatePanel();

                return false;
            }

            return true;
        }

        // Make "Send Invitation" button clickable. Next Patch is related
        [HarmonyPatch(nameof(UIInvitationRightMainPanel.SetSendButton))]
        [HarmonyPrefix]
        static bool SendInvitationClickable(UIInvitationRightMainPanel __instance)
        {
            __instance.button_SendButton.gameObject.SetActive(value: true);
            __instance.confirmAreaRoot.SetActive(value: false);

            __instance.ispossibleSend = __instance.invPanel.CurrentStage != null && __instance.invPanel.CurrentApplyState != InvitationApply_State.Normal;
            __instance.ButtonFrameHighlight.enabled = __instance.ispossibleSend;
            __instance.button_SendButton.interactable = __instance.ispossibleSend;
            __instance.SetColorAllFrames(__instance.ispossibleSend ? __instance.Color_Selectedcolor : UIColorManager.Manager.GetUIColor(UIColor.Default));
            __instance.SetColorInvitationSlots(__instance.ispossibleSend ? __instance.Color_Selectedcolor : UIColorManager.Manager.GetUIColor(UIColor.Default));

            return false;
        }

        [HarmonyPatch(nameof(UIInvitationRightMainPanel.SendInvitation))]
        [HarmonyPrefix]
        static bool SendButtonClickable(UIInvitationRightMainPanel __instance)
        {
            if (__instance.GetBookRecipe() != null)
                __instance.confirmAreaRoot.SetActive(value: true);

            return false;
        }


        // Make game think player has selected all the needed books. Next Patch is related, it's for general receptions
        [HarmonyPatch(nameof(UIInvitationRightMainPanel.GetAppliedBookModel))]
        [HarmonyPrefix]
        static bool FakeMoreBooks(UIInvitationRightMainPanel __instance, ref List<DropBookXmlInfo> __result)
        {
            if (__instance.invPanel.CurrentStage == null || __instance.invPanel.CurrentApplyState == InvitationApply_State.Normal)
                return true;

            __result = __instance.invPanel.CurrentStage.invitationInfo.needsBooks.Select(id => DropBookXmlList.Instance.GetData(id)).ToList();

            return false;
        }

        [HarmonyPatch(nameof(UIInvitationRightMainPanel.GetBookRecipe))]
        [HarmonyPrefix]
        static bool FakeEvenMoreBooks(UIInvitationRightMainPanel __instance, ref StageClassInfo __result)
        {
            var cur = __instance.invPanel.CurrentStage;
            if (cur != null)
            {
                __result = cur;

                return false;
            }

            return true;
        }
    }
}

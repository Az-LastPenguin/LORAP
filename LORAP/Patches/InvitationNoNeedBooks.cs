using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UI;
using UnityEngine;

namespace LORAP.Patches
{
    [HarmonyPatch(typeof(UIInvitationRightMainPanel))]
    internal class InvitationNoNeedBooks
    {
        // Set the UI Red as if all the needed books are selected, also make books unable to be selected
        [HarmonyPatch(nameof(UIInvitationRightMainPanel.SetInvBookApplyState))]
        [HarmonyPrefix]
        static bool FakeBooks(UIInvitationRightMainPanel __instance, ref InvitationApply_State state)
        {
            if (state == InvitationApply_State.Normal || state == InvitationApply_State.Fixed)
            {
                __instance.currentinvState = state;
                __instance.SetActiveEndEffect(on: false);

                foreach (var slot in __instance.invitationbookSlots)
                {
                    slot.SetDisabledSlot();
                }

                typeof(UIInvitationRightMainPanel).GetMethod("SetUpdatePanel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null);

                return false;
            }

            return true;
        }

        // Make "Send Invitation" button clickable. Next Patch is related
        [HarmonyPatch(nameof(UIInvitationRightMainPanel.SetSendButton))]
        [HarmonyPrefix]
        static bool SendInvitationClickable(UIInvitationRightMainPanel __instance)
        {
            ((GameObject)Traverse.Create(__instance).Field("ob_tutorialSendButtonhighlight").GetValue()).SetActive(value: false);
            SingletonBehavior<UIMainAutoTooltipManager>.Instance.AllCloseTooltip();
            __instance.button_SendButton.gameObject.SetActive(value: true);
            //__instance.txt_SendButton.text = TextDataModel.GetText("ui_invitation_send");
            __instance.confirmAreaRoot.SetActive(value: false);

            if (((UIInvitationPanel)Traverse.Create(__instance).Field("invPanel").GetValue()).CurrentStage == null || ((UIInvitationPanel)Traverse.Create(__instance).Field("invPanel").GetValue()).CurrentApplyState == InvitationApply_State.Normal)
            {
                Traverse.Create(__instance).Field("ispossiblesend").SetValue(false);
                ((Animator)Traverse.Create(__instance).Field("ButtonFrameHighlight").GetValue()).enabled = false;
                __instance.button_SendButton.interactable = false;
                __instance.SetColorAllFrames(UIColorManager.Manager.GetUIColor(UIColor.Default));
                __instance.SetColorInvitationSlots(UIColorManager.Manager.GetUIColor(UIColor.Default));
            }
            else
            {
                Traverse.Create(__instance).Field("ispossiblesend").SetValue(true);
                ((Animator)Traverse.Create(__instance).Field("ButtonFrameHighlight").GetValue()).enabled = true;
                __instance.button_SendButton.interactable = true;
                __instance.SetColorAllFrames(__instance.Color_Selectedcolor);
                __instance.SetColorInvitationSlots(__instance.Color_Selectedcolor);
            }

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
            if (((UIInvitationPanel)Traverse.Create(__instance).Field("invPanel").GetValue()).CurrentStage == null || ((UIInvitationPanel)Traverse.Create(__instance).Field("invPanel").GetValue()).CurrentApplyState == InvitationApply_State.Normal)
                return true;

            List<DropBookXmlInfo> list = new List<DropBookXmlInfo>();
            foreach (var id in ((UIInvitationPanel)Traverse.Create(__instance).Field("invPanel").GetValue()).CurrentStage.invitationInfo.needsBooks)
            {
                list.Add(Singleton<DropBookXmlList>.Instance.GetData(id));
            }
            __result = list;

            return false;
        }

        [HarmonyPatch(nameof(UIInvitationRightMainPanel.GetBookRecipe))]
        [HarmonyPrefix]
        static bool FakeEvenMoreBooks(UIInvitationRightMainPanel __instance, ref StageClassInfo __result)
        {
            var cur = ((UIInvitationPanel)Traverse.Create(__instance).Field("invPanel").GetValue()).CurrentStage;
            if (cur != null)
            {
                __result = cur;

                return false;
            }

            return true;
        }
    }
}

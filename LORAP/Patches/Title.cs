using HarmonyLib;
using LORAP.CustomUI;
using TMPro;
using UI;
using UI.Title;

namespace LORAP.Patches
{
    [HarmonyPatch(typeof(UITitleController))]
    internal class TitlePatches
    {
        // To Load the save and connect to AP
        [HarmonyPatch("Continue")]
        [HarmonyPrefix]
        static bool Prefix(UITitleController __instance)
        {
            APConnectWindow.Open();

            return false;
        }

        // Change Title Buttons
        [HarmonyPatch(nameof(UITitleController.OnSelectButton))]
        [HarmonyPostfix]
        static void TitleButtonsPatch(UITitleController __instance)
        {
            foreach (var b in __instance.TitleButtons)
            {
                if (b.type == TitleActionType.New_Game)
                {
                    b.SetState(ButtonState.Disabled);
                }

                if (b.type == TitleActionType.Continue)
                {
                    b.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Archipelago Connect";
                }
            }
        }
    }
}

using HarmonyLib;
using LORAP.CustomUI;
using UI;

namespace LORAP.Patches
{
    [HarmonyPatch(typeof(UIGetAbnormalityPanel))]
    internal class AbnoAndEGOPopup
    {
        [HarmonyPatch("SetData")]
        [HarmonyPrefix]
        static bool CustomSetData(UIGetAbnormalityPanel __instance)
        {
            return false;
        }

        [HarmonyPatch("PointerClickButton")]
        [HarmonyPrefix]
        static bool ConfirmClickPatch(UIGetAbnormalityPanel __instance)
        {
            MessagePopup.PagesClose();

            return false;
        }
    }
}

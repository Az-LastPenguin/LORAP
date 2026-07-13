using HarmonyLib;

namespace LORAP.Patches
{
    internal class PatchManager
    {
        internal static Harmony Harmony = null;

        internal static void PatchAll()
        {
            Harmony = new Harmony("LORAP");

            Harmony.PatchAll(typeof(AbnoAndEGOPages));
            Harmony.PatchAll(typeof(EmotionPatches));
            Harmony.PatchAll(typeof(GachaPatches));
            Harmony.PatchAll(typeof(MapPatches));
            Harmony.PatchAll(typeof(OtherPatches));
            Harmony.PatchAll(typeof(RemoveStory));
            Harmony.PatchAll(typeof(SuppressionsAndReceptions));
            Harmony.PatchAll(typeof(TitlePatches));
        }
    }
}

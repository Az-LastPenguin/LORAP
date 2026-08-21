using LORAP.Patches;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace LORAP
{
    public class LORAP : ModInitializer
    {
        internal static LORAP Instance { get; private set; }

        internal static string ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        internal static string ModVersion = "v1.0-eta";

        public override void OnInitializeMod()
        {
            base.OnInitializeMod();

            Instance = this;

            PatchManager.PatchAll();

            Debug.Log($"[LORAP] Loaded!");
        }
    }
}

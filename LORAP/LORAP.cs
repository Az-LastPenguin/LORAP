using HarmonyLib;
using LORAP.Patches;
using LORAP.Utils;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LORAP
{
    public class LORAP : ModInitializer
    {
        internal static Harmony Harmony = null;

        internal static LORAP Instance { get; private set; }

        internal static string ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        internal static string ModVersion = "v0.4";

        public override void OnInitializeMod()
        {
            base.OnInitializeMod();

            Instance = this;

            Harmony = new Harmony("LORAP");

            Harmony.PatchAll(typeof(AbnoAndEGOPages));
            Harmony.PatchAll(typeof(GachaPatches));
            Harmony.PatchAll(typeof(OtherPatches));
            Harmony.PatchAll(typeof(RemoveStory));
            Harmony.PatchAll(typeof(SuppressionsAndReceptions));
            Harmony.PatchAll(typeof(TitlePatches));

            SceneManager.sceneLoaded += OnSceneLoad;

            Debug.Log($"LORAP loaded!");
        }

        public void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Stage_Hod_New")
            {
                // Setup something for coroutines
                var gameObject = new GameObject("LORAP Coroutines");
                gameObject.AddComponent<Timing>();
                Timing.Setup(gameObject);

                //GameOpeningController.Instance.SetOnPlayEndMethod(ContentManager.AddCustomContent);
            }
        }
    }
}

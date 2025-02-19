using HarmonyLib;
using LORAP.Gameplay;
using LORAP.Utils;
using Opening;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LORAP
{
    public class LORAP : ModInitializer
    {
        internal static Harmony harmony;

        internal static LORAP Instance { get; private set; }

        internal static string ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public override void OnInitializeMod()
        {
            base.OnInitializeMod();

            Instance = this;

            harmony = new Harmony($"LORAP-Harmony");

            harmony.PatchAll();

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

                GameOpeningController.Instance.SetOnPlayEndMethod(ContentManager.AddCustomContent);
            }
        }
    }
}

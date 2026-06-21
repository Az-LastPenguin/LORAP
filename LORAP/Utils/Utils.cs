using LORAP.Archipelago;
using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace LORAP.Utils
{
    internal static class GameUtils
    {
        internal static List<SephirahType> FloorSephs = new List<SephirahType>()
        {
            SephirahType.Malkuth,
            SephirahType.Yesod,
            SephirahType.Hod,
            SephirahType.Netzach,
            SephirahType.Tiphereth,
            SephirahType.Gebura,
            SephirahType.Chesed,
            SephirahType.Binah,
            SephirahType.Hokma,
            SephirahType.Keter,
        };
    }

    // Class to help work with UI
    internal static class UIUtils
    {
        internal static ColorBlock BasicButtonColors = new ColorBlock()
        {
            normalColor = new Color(0.9372f, 0.7607f, 0.5058f, 1f),
            highlightedColor = new Color(0.1333f, 1f, 0.8941f, 1f),
            pressedColor = new Color(0.1069f, 0.802f, 0.7170f, 1f),
            colorMultiplier = 1f,
            fadeDuration = 0f,
        };

        internal static Sprite FillerSprite = AssetBundleHelper.GetAsset<Sprite>("filler");
        internal static Sprite UsefulSprite = AssetBundleHelper.GetAsset<Sprite>("useful");
        internal static Sprite ProgSprite = AssetBundleHelper.GetAsset<Sprite>("prog");
        internal static Sprite FillerSmallSprite = AssetBundleHelper.GetAsset<Sprite>("fillerSmall");
        internal static Sprite UsefulSmallSprite = AssetBundleHelper.GetAsset<Sprite>("usefulSmall");
        internal static Sprite ProgSmallSprite = AssetBundleHelper.GetAsset<Sprite>("progSmall");

        internal static Sprite CheckmarkSprite = AssetBundleHelper.GetAsset<Sprite>("checkmark");
        internal static Sprite ExclamationSprite = AssetBundleHelper.GetAsset<Sprite>("exclamation");

        internal static Sprite Transparent = AssetBundleHelper.GetAsset<Sprite>("Transparent");

        internal static Dictionary<SephirahType, List<UIIconManager.IconSet>> FloorTierSprites = new Dictionary<SephirahType, List<UIIconManager.IconSet>>();

        internal static void Init()
        {
            // Create IconSets for every floor stage
            foreach (SephirahType seph in GameUtils.FloorSephs)
            {
                FloorTierSprites[seph] = new List<UIIconManager.IconSet>();

                for (int i = 1; i <= 5; i++)
                {
                    Sprite sprite = AssetBundleHelper.GetAsset<Sprite>($"{seph.ToString()}{i}");

                    FloorTierSprites[seph].Add(new UIIconManager.IconSet()
                    {
                        icon = sprite,
                        iconGlow = sprite,
                        color = Color.clear,
                        colorGlow = Color.clear,
                        type = "",
                    });
                }
            }
        }

        internal static UIIconManager.IconSet GetFloorIconSet(int stageId, SephirahType seph)
        {
            if (stageId >= 210005 && stageId <= 210008)
                stageId = 210009;

            return FloorTierSprites[seph][SlotDataManager.AbnoFightOrder[seph].IndexOf(stageId)];
        }
    }

    // Custom class made to access Coroutines without needing to create a new GameObject or search for one
    // Also has some utility functions for timing of things
    internal class Timing : MonoBehaviour
    {
        internal static void Init(GameObject newInstance)
        {
            instance = newInstance.GetComponent<Timing>();
        }

        private static Timing instance;

        public static Coroutine Coroutine(IEnumerator enumerator)
        {
            return instance.StartCoroutine(enumerator);
        }

        public static Coroutine After(float time, Action func)
        {
            IEnumerator Coroutine()
            {
                yield return new WaitForSeconds(time);

                func.Invoke();
            };

            return instance.StartCoroutine(Coroutine());
        }
    }
}

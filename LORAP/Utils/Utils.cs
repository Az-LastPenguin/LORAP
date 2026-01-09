using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LORAP.Utils
{
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

        internal static Sprite FillerSprite = AssetBundleHelper.GetAsset<Sprite>("fillersprite");
        internal static Sprite UsefulSprite = AssetBundleHelper.GetAsset<Sprite>("usefulsprite");
        internal static Sprite ProgSprite = AssetBundleHelper.GetAsset<Sprite>("progsprite");

        internal static void Setup()
        {

        }
    }

    // Custom class made to access Coroutines without needing to create a new GameObject or search for one
    // Also has some utility functions for timing of things
    internal class Timing : MonoBehaviour
    {
        internal static void Setup(GameObject newInstance)
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

    internal static class ListExtensions
    {
        internal static T PopRandom<T>(this List<T> list, System.Random random)
        {
            int rng = random.Next(list.Count);
            T element = list.ElementAt(rng);
            list.RemoveAt(rng);
            return element;
        }
    }
}

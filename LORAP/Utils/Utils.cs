using LORAP.Archipelago;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace LORAP.Utils
{
    // Some other general stuff
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

        private static uint MixHash(uint hash, int value)
        {
            unchecked
            {
                hash ^= (uint)value;
                hash *= 16777619u;
                hash ^= (uint)(value >> 8);
                hash *= 16777619u;
                hash ^= (uint)(value >> 16);
                hash *= 16777619u;
                hash ^= (uint)(value >> 24);
                hash *= 16777619u;
                return hash;
            }
        }

        private static int CreateSeed(string stream, int salt = 0)
        {
            unchecked
            {
                uint hash = 2166136261u;
                hash = MixHash(hash, SlotDataManager.ClientSeed);
                hash = MixHash(hash, salt);

                if (stream != null)
                {
                    for (int i = 0; i < stream.Length; i++)
                        hash = MixHash(hash, stream[i]);
                }

                int seed = (int)(hash & 0x7FFFFFFF);
                return seed == 0 ? 1 : seed;
            }
        }

        internal static System.Random CreateRandom(string stream, int salt = 0)
        {
            return new System.Random(CreateSeed(stream, salt));
        }

        internal static T DeseriallizeFromFile<T>(string path, string filename)
        {
            string fullPath = $"{path}/{filename}";

            if (!Directory.Exists(path) || !File.Exists(fullPath))
                return default;

            try
            {
                T Data;
                using (FileStream fileStream = File.Open(fullPath, FileMode.Open))
                    Data = (T)new BinaryFormatter().Deserialize(fileStream);

                if (Data == null)
                    return default;

                return Data;
            }
            catch (Exception)
            {
                return default;
            }
        }

        internal static bool SerializeToFile(object wtvr, string path, string filename)
        {
            string fullPath = $"{path}/{filename}";

            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                using (FileStream serializationStream = File.Create(fullPath))
                    new BinaryFormatter().Serialize(serializationStream, wtvr);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                return false;
            }
        }
    }

    // Class to help work with UI
    internal static class UIUtils
    {
        internal static ColorBlock BasicButtonColors = new ColorBlock()
        {
            normalColor = new Color(0.9372f, 0.7607f, 0.5058f, 1f),
            highlightedColor = new Color(0.1333f, 1f, 0.8941f, 1f),
            pressedColor = new Color(0.1069f, 0.802f, 0.7170f, 1f),
            selectedColor = new Color(0.9372f, 0.7607f, 0.5058f, 1f),
            colorMultiplier = 1f,
            fadeDuration = 0f,
        };

        internal static ColorBlock TabButtonColors = new ColorBlock()
        {
            normalColor = new Color(0f, 0f, 0f, 0f),
            highlightedColor = new Color(1f, 1f, 1f, 1f),
            pressedColor = new Color(1f, 1f, 1f, 1f),
            selectedColor = new Color(1f, 1f, 1f, 1f),
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
            // Add LORAP icons for books & library level
            UIIconManager.IconSet progIcon = new UIIconManager.IconSet()
            {
                icon = ProgSmallSprite,
                iconGlow = ProgSmallSprite,
                color = Color.clear,
                colorGlow = Color.clear,
                type = "",
            };

            UIIconManager.IconSet fillerIcon = new UIIconManager.IconSet()
            {
                icon = FillerSmallSprite,
                iconGlow = FillerSmallSprite,
                color = Color.clear,
                colorGlow = Color.clear,
                type = "",
            };

            UISpriteDataManager.instance.StoryIcons.Add(progIcon);
            UISpriteDataManager.instance.StoryIconDic.Add("prog", progIcon);
            UISpriteDataManager.instance.StoryIcons.Add(fillerIcon);
            UISpriteDataManager.instance.StoryIconDic.Add("filler", fillerIcon);

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

    // Class made to access Coroutines without needing to create a new GameObject or search for one // TODO: Remove it
    // Also has some utility functions for timing of things
    internal class Timing : SingletonBehavior<Timing>
    {
        public Coroutine Coroutine(IEnumerator enumerator)
        {
            return StartCoroutine(enumerator);
        }

        public Coroutine InvokeDelayed(Action func, float time)
        {
            IEnumerator Coroutine()
            {
                yield return new WaitForSeconds(time);

                func.Invoke();
            };

            return StartCoroutine(Coroutine());
        }
    }
}

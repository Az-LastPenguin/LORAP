using System.Collections.Generic;
using UnityEngine;

namespace LORAP.Utils
{
    internal static class AssetBundleHelper
    {
        private static AssetBundle Bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(LORAP.ModPath, "lorap"));

        public static GameObject GetAsset(string asset)
        {
            return Bundle.LoadAsset<GameObject>(asset);
        }

        public static T GetAsset<T>(string asset) where T : Object
        {
            return Bundle.LoadAsset<T>(asset);
        }
    }
}

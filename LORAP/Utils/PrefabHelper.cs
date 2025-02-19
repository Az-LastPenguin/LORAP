using System.Collections.Generic;
using UnityEngine;

namespace LORAP.Utils
{
    internal static class PrefabHelper
    {
        private static Dictionary<string, AssetBundle> AssetBundles = new();

        public static GameObject GetPrefab(string assetBundle, string prefab)
        {
            if (!AssetBundles.ContainsKey(assetBundle))
                AssetBundles.Add(assetBundle, AssetBundle.LoadFromFile(System.IO.Path.Combine(LORAP.ModPath, assetBundle)));

            return AssetBundles[assetBundle].LoadAsset<GameObject>(prefab);
        }
    }
}

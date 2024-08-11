using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LORAP
{
    internal static class PrefabHelper
    {
        private static Dictionary<string, AssetBundle> AssetBundles = new();

        public static GameObject GetPrefab(string assetBundle, string prefab)
        {
            if (!AssetBundles.ContainsKey(assetBundle))
                AssetBundles.Add(assetBundle, AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(LORAP.Instance.Info.Location), assetBundle)));

            return AssetBundles[assetBundle].LoadAsset<GameObject>(prefab);
        }
    }
}

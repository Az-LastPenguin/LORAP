using LORAP.Playthru;
using LORAP.Gameplay;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LORAP.Archipelago
{
    internal class BattleNode
    {
        public string Key;
        public int Id;
        public string Name;
        public BattleNodeKind Kind;
        public int Chapter;
        public int Sphere;
        public int SphereLayer;
        public int GlobalLayer;
        public int RequiredLibrarians;
        public SephirahType AssignedFloor = SephirahType.None;
        public List<string> Next = new List<string>();
        //public int VisualX;
        //public int VisualY;

        public bool AreBattleParentsComplete()
        {
            if (SlotDataManager.BoELayersEnabled)
            {
                if (!PlaythruManager.IsLayerUnlocked(GlobalLayer))
                    return false;
                if (Sphere <= 7)
                    return true;
            }

            List<BattleNode> parents = SlotDataManager.BattleTree?.GetPrevNodes(Key) ?? new List<BattleNode>();
            if (parents.Count == 0)
                return true;

            foreach (BattleNode parent in parents)
            {
                if (!PlaythruManager.IsStageComplete(parent.Id))
                    continue;

                return true;
            }

            return false;
        }
    }

    internal class BattleTree
    {
        public Dictionary<string, BattleNode> Nodes = new Dictionary<string, BattleNode>();

        public BattleNode GetNode(string key) => Nodes.ContainsKey(key) ? Nodes[key] : null;

        public BattleNode GetNodeById(int id) => Nodes.Values.FirstOrDefault(n => n.Id == id);

        public List<BattleNode> GetPrevNodes(string key) => Nodes.Values.Where(n => n.Next.Contains(key)).ToList();

        public List<BattleNode> GetPrevNodesById(int id)
        {
            BattleNode node = GetNodeById(id);
            return node == null ? new List<BattleNode>() : GetPrevNodes(node.Key);
        }
    }

    // NOTE: This handles generation specific SlotData like reception tree, Reception requirements, etc. Options are handled in SettingsManager
    internal static class SlotDataManager
    {
        internal static int ClientSeed;

        internal const int SupportedSlotDataVersion = 3;

        internal static int SlotDataVersion;

        //internal static int SphereClearPercentage;
  
        internal static Dictionary<int, List<int>> ReceptionBookRequirements;

        internal static BattleTree BattleTree;

        internal static Dictionary<SephirahType, List<List<int>>> AbnoBookRequirements;

        internal static Dictionary<SephirahType, List<int>> AbnoFightOrder;

        internal static Dictionary<int, int> AbnoStageChapters;

        internal static List<int> BoEBundlesPerSphere;

        internal static List<int> BoEBundlesCumulative;

        internal static int BoERequiredTotal;

        internal static int BoEPoolTotal;

        internal static int LayerCount;

        internal static int VanillaBooksRequiredTotal;

        internal static int VanillaBooksPoolTotal;

        internal static int FirstReception;

        internal static int LastReception;

        // Utils
        internal static bool HasBattleTree => BattleTree != null && BattleTree.Nodes.Count > 0;

        internal static bool BoELayersEnabled => SettingsManager.RunProgressionMode == ProgressionMode.BoELayers;

        internal static int GetBoEBundlesRequiredThroughSphere(int sphere)
        {
            if (BoEBundlesCumulative == null || sphere <= 0)
                return 0;

            int index = Math.Min(sphere, BoEBundlesCumulative.Count) - 1;
            return index >= 0 ? BoEBundlesCumulative[index] : 0;
        }

        internal static bool IsSphereClearEnough(int sphere)
        {
            if (!BoELayersEnabled)
                return true;

            List<BattleNode> sphereNodes = GetLogicalSphereNodes(sphere);
            if (sphereNodes.Count == 0)
                return true;

            int percentage = Math.Max(0, Math.Min(100, SettingsManager.SphereClearPercentage.GetValue()));
            int required = (int)Math.Ceiling(sphereNodes.Count * percentage / 100.0);
            if (required <= 0)
                return true;

            int cleared = sphereNodes.Count(n => PlaythruManager.IsStageComplete(n.Id));
            return cleared >= required;
        }

        internal static int GetSphereClearPercentage(int sphere)
        {
            if (!HasBattleTree)
                return 0;

            List<BattleNode> sphereNodes = GetLogicalSphereNodes(sphere);
            if (sphereNodes.Count == 0)
                return 100;

            int cleared = sphereNodes.Count(n => PlaythruManager.IsStageComplete(n.Id));
            return (int)Math.Floor(cleared * 100.0 / sphereNodes.Count);
        }

        private static List<BattleNode> GetLogicalSphereNodes(int sphere)
        {
            // 210005-210008 only exist on the client, so don't count them as four extra battles in the sphere.
            return BattleTree.Nodes.Values
                .Where(node => node.Sphere == sphere && (node.Id < 210005 || node.Id > 210008))
                .ToList();
        }

        // Main Code
        private static object GetSlotData(Dictionary<string, object> slotData, string key)
        {
            if (!slotData.ContainsKey(key))
                throw new Exception($"Data {key} is missing from SlotData! Possible mod and .apworld version mismatch?");

            return slotData[key];
        }

        internal static void ParseSlotData(Dictionary<string, object> slotData)
        {
            SlotDataVersion = Convert.ToInt32(GetSlotData(slotData, "slot_data_version"));
            if (SlotDataVersion != SupportedSlotDataVersion)
                throw new Exception($"Unsupported LORAP slot data version {SlotDataVersion}; expected {SupportedSlotDataVersion}.");

            ClientSeed = Convert.ToInt32(GetSlotData(slotData, "lorap_client_seed"));

            FirstReception = Convert.ToInt32(GetSlotData(slotData, "first_reception"));

            LastReception = Convert.ToInt32(GetSlotData(slotData, "last_reception"));

            ReceptionBookRequirements = new Dictionary<int, List<int>>();
            foreach (var o in (JObject)GetSlotData(slotData, "reception_book_requirements"))
            {
                ReceptionBookRequirements[Int32.Parse(o.Key)] = o.Value.Select(v => (int)v.Value<long>()).ToList();
            }

            AbnoBookRequirements = new Dictionary<SephirahType, List<List<int>>>();
            var abnoBooks = (JArray)GetSlotData(slotData, "abno_book_requirements");
            for (int i = 0; i < 10; i++)
            {
                AbnoBookRequirements[(SephirahType)(i + 1)] = abnoBooks[i].Select(j => j.Select(k => (int)k.Value<long>()).ToList()).ToList();
            }

            AbnoFightOrder = new Dictionary<SephirahType, List<int>>();
            var abnoOrder = (JArray)GetSlotData(slotData, "abno_fight_order");
            for (int i = 0; i < 10; i++)
            {
                AbnoFightOrder[(SephirahType)(i + 1)] = abnoOrder[i].Select(j => (int)j.Value<long>()).ToList();
            }

            AbnoStageChapters = new Dictionary<int, int>();
            foreach (var o in (JObject)GetSlotData(slotData, "abno_stage_chapters"))
            {
                AbnoStageChapters[Int32.Parse(o.Key)] = o.Value.Value<int>();
            }

            BoEBundlesPerSphere = ((JArray)GetSlotData(slotData, "boe_bundles_per_sphere"))
                .Select(i => i.Value<int>()).ToList();
            BoEBundlesCumulative = ((JArray)GetSlotData(slotData, "boe_bundles_cumulative"))
                .Select(i => i.Value<int>()).ToList();
            BoERequiredTotal = Convert.ToInt32(GetSlotData(slotData, "boe_required_total"));
            BoEPoolTotal = Convert.ToInt32(GetSlotData(slotData, "boe_pool_total"));
            LayerCount = Convert.ToInt32(GetSlotData(slotData, "layer_count"));
            VanillaBooksRequiredTotal = Convert.ToInt32(GetSlotData(slotData, "vanilla_books_required_total"));
            VanillaBooksPoolTotal = Convert.ToInt32(GetSlotData(slotData, "vanilla_books_pool_total"));

            if (!slotData.ContainsKey("battle_nodes") || !slotData.ContainsKey("battle_edges"))
                throw new Exception("LORAP slot data is missing battle tree data.");

            BattleTree = new BattleTree();

            foreach (var o in (JObject)slotData["battle_nodes"])
            {
                JObject data = (JObject)o.Value;
                BattleNodeKind kind = data["kind"].Value<string>() == "stage"
                    ? BattleNodeKind.Stage
                    : BattleNodeKind.Reception;

                //if (!data.ContainsKey("visual_x") || !data.ContainsKey("visual_y"))
                //    throw new Exception($"LORAP battle node {o.Key} is missing visual coordinates.");

                BattleNode node = new BattleNode
                {
                    Key = o.Key,
                    Id = data["id"].Value<int>(),
                    Name = data["name"].Value<string>(),
                    Kind = kind,
                    Chapter = data["chapter"].Value<int>(),
                    Sphere = data.ContainsKey("sphere") ? data["sphere"].Value<int>() : data["chapter"].Value<int>(),
                    SphereLayer = data.ContainsKey("sphere_layer") ? data["sphere_layer"].Value<int>() : 0,
                    GlobalLayer = data["global_layer"].Value<int>(),
                    RequiredLibrarians = data["req_librarians"].Value<int>(),
                    //VisualX = data["visual_x"].Value<int>(),
                    //VisualY = data["visual_y"].Value<int>(),
                };

                if (kind == BattleNodeKind.Stage && data.ContainsKey("assigned_floor"))
                    node.AssignedFloor = (SephirahType)data["assigned_floor"].Value<int>();

                BattleTree.Nodes[node.Key] = node;
            }

            foreach (JObject edge in (JArray)slotData["battle_edges"])
            {
                string source = edge["source"].Value<string>();
                string target = edge["target"].Value<string>();

                if (BattleTree.Nodes.ContainsKey(source) && BattleTree.Nodes.ContainsKey(target))
                    BattleTree.Nodes[source].Next.Add(target);
            }
        }
    }
}

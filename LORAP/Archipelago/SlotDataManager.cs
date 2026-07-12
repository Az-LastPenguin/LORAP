using LORAP.Playthru;
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
        public int RequiredLibrarians;
        public SephirahType AssignedFloor = SephirahType.None;
        public List<string> Next = new List<string>();
        //public int VisualX;
        //public int VisualY;

        public bool AreBattleParentsComplete()
        {
            List<BattleNode> parents = SlotDataManager.BattleTree?.GetPrevNodes(Key) ?? new List<BattleNode>();
            return parents.Count == 0 || parents.Any(parent => PlaythruManager.IsStageComplete(parent.Id));
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

        internal static Dictionary<int, List<int>> ReceptionBookRequirements;

        internal static BattleTree BattleTree;

        internal static bool HasBattleTree => BattleTree != null && BattleTree.Nodes.Count > 0;

        internal static Dictionary<SephirahType, List<List<int>>> AbnoBookRequirements;

        internal static Dictionary<SephirahType, List<int>> AbnoFightOrder;

        internal static Dictionary<int, int> AbnoStageChapters;

        internal static int FirstReception;

        internal static int LastReception;

        internal static void ParseSlotData(Dictionary<string, object> slotData)
        {
            ClientSeed = Convert.ToInt32(slotData["lorap_client_seed"]);

            FirstReception = Convert.ToInt32(slotData["first_reception"]);

            LastReception = Convert.ToInt32(slotData["last_reception"]);

            ReceptionBookRequirements = new Dictionary<int, List<int>>();
            foreach (var o in (JObject)slotData["reception_book_requirements"])
            {
                ReceptionBookRequirements[Int32.Parse(o.Key)] = o.Value.Select(v => (int)v.Value<long>()).ToList();
            }

            AbnoBookRequirements = new Dictionary<SephirahType, List<List<int>>>();
            var abnoBooks = (JArray)slotData["abno_book_requirements"];
            for (int i = 0; i < 10; i++)
            {
                AbnoBookRequirements[(SephirahType)(i + 1)] = abnoBooks[i].Select(j => j.Select(k => (int)k.Value<long>()).ToList()).ToList();
            }

            AbnoFightOrder = new Dictionary<SephirahType, List<int>>();
            var abnoOrder = (JArray)slotData["abno_fight_order"];
            for (int i = 0; i < 10; i++)
            {
                AbnoFightOrder[(SephirahType)(i + 1)] = abnoOrder[i].Select(j => (int)j.Value<long>()).ToList();
            }

            AbnoStageChapters = new Dictionary<int, int>();
            if (slotData.ContainsKey("abno_stage_chapters"))
            {
                foreach (var o in (JObject)slotData["abno_stage_chapters"])
                {
                    AbnoStageChapters[Int32.Parse(o.Key)] = o.Value.Value<int>();
                }
            }

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

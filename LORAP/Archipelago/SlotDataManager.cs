using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace LORAP.Archipelago
{
    internal enum Endgoal
    {
        ReverberationEnsemble,
        BlackSilence,
        KeterRealization,
        DistortedEnsemble
    }

    internal enum AbnoPageShuffle
    {
        None,
        InFloor,
        Sets,
        Pages,
    }

    internal enum AbnoPageRandomization
    {
        None,
        Guarantee,
        Unbound
    }

    internal enum BookContentsRandomization
    {
        BookChapter,
        StageChapter,
        Chaotic
    }

    internal enum BattleNodeKind
    {
        Reception,
        Stage
    }

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
        public float VisualX;
        public float VisualY;
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

    internal static class SlotDataManager
    {
        internal static int Seed;

        internal static List<Endgoal> Endgoals;

        internal static long EnsembleBattles;

        internal static AbnoPageShuffle AbnoPageShuffle;

        internal static AbnoPageRandomization AbnoPageRandomization;

        internal static bool ExodiaGuarantee;

        internal static bool EgoPageShuffle;

        internal static bool RandomizeReceptionTree;

        internal static bool ReceptionsRequireBooks;

        internal static bool EnemiesTurnIntoChecks;

        internal static bool ShuffleAbnos;

        internal static bool ShuffleRealizations;

        internal static bool FloorsRequireBooks;

        internal static bool RandomizeBlackSilencePage;

        internal static BookContentsRandomization BookContentsRandomization;

        internal static Dictionary<int, List<int>> ReceptionBookRequirements;

        internal static BattleTree BattleTree;

        internal static bool HasBattleTree => BattleTree != null && BattleTree.Nodes.Count > 0;

        internal static Dictionary<SephirahType, List<List<int>>> AbnoBookRequirements;

        internal static Dictionary<SephirahType, List<int>> AbnoFightOrder;

        internal static Dictionary<int, int> AbnoStageChapters;

        internal static int FirstReception;

        internal static int LastReception;

        internal static void Parse(Dictionary<string, object> slotData)
        {
            Seed = (int)(long)slotData["random_seed"];

            Endgoals = ((JArray)slotData["endgoals"]).Select(i => (Endgoal)Enum.Parse(typeof(Endgoal), i.Value<string>().Replace(" ", ""))).ToList();

            EnsembleBattles = (long)slotData["ensemble_battles"];

            AbnoPageShuffle = (AbnoPageShuffle)(long)slotData["abno_page_shuffle"];

            AbnoPageRandomization = (AbnoPageRandomization)(long)slotData["abno_page_randomization"];

            ExodiaGuarantee = (long)slotData["exodia_guaratnee"] == 1;

            EgoPageShuffle = (long)slotData["ego_page_shuffle"] == 1;

            RandomizeReceptionTree = (long)slotData["randomize_reception_tree"] == 1;

            ReceptionsRequireBooks = (long)slotData["receptions_require_books"] == 1;

            EnemiesTurnIntoChecks = (long)slotData["enemies_turn_into_checks"] == 1;

            ShuffleAbnos = (long)slotData["shuffle_abnos"] == 1;

            ShuffleRealizations = (long)slotData["shuffle_realizations"] == 1;

            FloorsRequireBooks = (long)slotData["floors_require_books"] == 1;

            RandomizeBlackSilencePage = (long)slotData["randomize_black_silence_page"] == 1;

            BookContentsRandomization = (BookContentsRandomization)(long)slotData["book_contents_randomization"];

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

                if (!data.ContainsKey("visual_x") || !data.ContainsKey("visual_y"))
                    throw new Exception($"LORAP battle node {o.Key} is missing visual coordinates.");

                BattleNode node = new BattleNode
                {
                    Key = o.Key,
                    Id = data["id"].Value<int>(),
                    Name = data["name"].Value<string>(),
                    Kind = kind,
                    Chapter = data["chapter"].Value<int>(),
                    RequiredLibrarians = data["req_librarians"].Value<int>(),
                    VisualX = data["visual_x"].Value<float>(),
                    VisualY = data["visual_y"].Value<float>(),
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

using LORAP.Archipelago;
using LORAP.CustomUI.Components;
using LORAP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace LORAP.Gameplay
{
    internal static class ContentManager // TODO: Separate Map / Stage / Abno&Ego pages into different classes // TODO: This whole fucking class needs a refactor god motherfucking damn it
    {
        private class MapNode
        {
            public string Key = "";
            public List<MapNode> Next = new List<MapNode>();
            public List<MapNode> Prev = new List<MapNode>();
            public float X = 0;
            public float Y = 0;
        }

        private const float SphereSeparatorGap = 300f;
        private static readonly Vector2 SphereSeparatorOffset = new Vector2(0f,-10f);
        private const float FirstSphereSeparatorExtraDown = 100f;

        private static Dictionary<SephirahType, int> EnsembleFloorToStage = new Dictionary<SephirahType, int>()
        {
            [SephirahType.Malkuth] = 70001,
            [SephirahType.Yesod] = 70002,
            [SephirahType.Hod] = 70003,
            [SephirahType.Netzach] = 70004,
            [SephirahType.Tiphereth] = 70005,
            [SephirahType.Gebura] = 70006,
            [SephirahType.Chesed] = 70007,
            [SephirahType.Binah] = 70008,
            [SephirahType.Hokma] = 70009,
            [SephirahType.Keter] = 70010,
        };


        private static UIStoryProgressIconSlot MapIconTemplate;
        private static GameObject LineTemplate;
        private static GameObject ChapterSeparatorTemplate = AssetBundleHelper.GetAsset("ChapterSeparator");

        private static List<GameObject> NodeLines = new List<GameObject>();
        internal static List<ChapterSeparator> ChapterSeparators = new List<ChapterSeparator>();

        internal static Dictionary<int, DropBookXmlInfo> CustomBooks = new Dictionary<int, DropBookXmlInfo>();

        private static List<EmotionCardXmlInfo> AbnoPageInitialList;

        private static List<EmotionEgoXmlInfo> EGOPageInitialList;

        internal static void Init()
        {
            Debug.Log("[LORAP] Initializing Custom Content");

            // Save vanilla lists of Abno and EGO Pages to return or modify later on
            AbnoPageInitialList = EmotionCardXmlList.Instance._list.ToList();
            EGOPageInitialList = EmotionEgoXmlList.Instance._list.ToList();

            // Modify some parts of the game
            ApplyMapChanges();

            // Add names for realization stages so map doesn't show them all as "Unknown" (yay hardcoding)
            Dictionary<int, string> stageNames = new Dictionary<int, string>()
            {
                [201005] = $"{SephirahType.Malkuth.FloorName()} Realization",
                [202005] = $"{SephirahType.Yesod.FloorName()} Realization",
                [203005] = $"{SephirahType.Hod.FloorName()} Realization",
                [204005] = $"{SephirahType.Netzach.FloorName()} Realization",
                [205005] = $"{SephirahType.Tiphereth.FloorName()} Realization",
                [206005] = $"{SephirahType.Gebura.FloorName()} Realization",
                [207005] = $"{SephirahType.Chesed.FloorName()} Realization",
                [208004] = $"{SephirahType.Binah.FloorName()} Realization",
                [209004] = $"{SephirahType.Hokma.FloorName()} Realization",

                [210005] = $"{SephirahType.Keter.FloorName()} Realization",
                [210006] = $"{SephirahType.Keter.FloorName()} Realization",
                [210007] = $"{SephirahType.Keter.FloorName()} Realization",
                [210008] = $"{SephirahType.Keter.FloorName()} Realization",
                [210009] = $"{SephirahType.Keter.FloorName()} Realization",
            };

            foreach (KeyValuePair<int, string> pair in stageNames) // I do it this way so it doesn't throw an error due to 210005-210009 names already existing
                StageNameXmlList._instance._dictionary[pair.Key] = pair.Value;

            // Add BOE and Booster Pack to book list
            CreateCustomBook(123456, "Book of Everything", "prog");
            CreateCustomBook(123457, "Booster Pack", "filler");
        }

        private static void ApplyMapChanges()
        {
            Debug.Log("[LORAP] Applying Map Changes");

            UIStoryProgressPanel MapPanel = UIControlManager.Instance.GetInvitationPanel().InvCenterStoryPanel;

            // Save an icon to later use as a template and hide it
            MapIconTemplate = MapPanel.iconList.First();
            LineTemplate = MapIconTemplate.connectLineList.First();
            LineTemplate.SetActive(false);
            MapIconTemplate.connectLineList.Clear();
            MapPanel.iconList.Remove(MapIconTemplate);
            MapIconTemplate.SetActiveStory(false);

            // Clear vanilla map
            foreach (UIStoryProgressIconSlot icon in MapPanel.iconList)
            {
                foreach (var line in icon.connectLineList)
                {
                    GameObject.Destroy(line);
                }

                GameObject.Destroy(icon.gameObject);
                GameObject.Destroy(icon);
            }

            foreach (var item in MapPanel.chapterIconList)
            {
                item.gameObject.SetActive(false);
            }

            foreach (var item in MapPanel.blockChapterList)
            {
                item.root.gameObject.SetActive(false);
            }

            MapPanel.iconList.Clear();
        }

        internal static void SetupRunContent()
        {
            Debug.Log("[LORAP] Initializing Run");

            ShuffleAbnoPages();

            RandomizeAbnoPages();

            ShuffleEGOPages();

            PrepareStages();

            PrepareMap();
        }

        private static void ShuffleAbnoPages()
        {
            // If we don't wanna shuffle pages, just ensure that list is same as vanilla
            if (SettingsManager.AbnoPageShuffle == AbnoPageShuffle.None)
            {
                EmotionCardXmlList.Instance._list = AbnoPageInitialList.ToList();

                return;
            }

            Debug.Log("[LORAP] Shuffling Abno Pages");

            // Randomize Abno Pages' floors
            var Random = GameUtils.CreateRandom("abno_page_shuffle");

            // Deep copy of the initial list
            List<EmotionCardXmlInfo> allPages = AbnoPageInitialList.ConvertAll(p =>
            {
                var page = new EmotionCardXmlInfo();
                page.id = p.id;
                page.Name = p.Name;
                page._artwork = p._artwork;
                page.State = p.State;
                page.TargetType = p.TargetType;
                page.Level = p.Level;
                page.EmotionLevel = p.EmotionLevel;
                page.EmotionRate = p.EmotionRate;
                page.Locked = p.Locked;
                page.Sephirah = p.Sephirah;
                page.Script = p.Script;

                return page;
            });

            // Pool of pages that contains only relevant sephirahs abno pages (aka all sephirahs, but the game has other non relevant values)
            List<EmotionCardXmlInfo> abnoPagePool = allPages.Where(x => GameUtils.FloorSephs.Contains(x.Sephirah)).ToList();

            // Resulting list of abno pages
            List<EmotionCardXmlInfo> abnoPages = allPages.Where(p => !GameUtils.FloorSephs.Contains(p.Sephirah)).ToList();

            // If we randomize in same floor, just shuffle them around, it's good enough, no need to ensure anything else
            if (SettingsManager.AbnoPageShuffle == AbnoPageShuffle.InFloor)
            {
                foreach (var seph in GameUtils.FloorSephs)
                {
                    List<EmotionCardXmlInfo> sephPages = abnoPagePool.Where(p => p.Sephirah == seph).ToList();

                    for (int i = 2; i < 7; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            EmotionCardXmlInfo page = sephPages.TakeRandom(Random);

                            page.Level = i;
                            abnoPages.Add(page);
                        }
                    }
                }

                EmotionCardXmlList.Instance._list = abnoPages;

                return;
            }


            // If Exodia Guarantee is true, put Exodia pages onto a random floor.
            // After that, if AbnoPageShuffle is set to Sets, put other pages from the same set there
            // Then randomize every other page/set
            Dictionary<SephirahType, List<int>> exodiaPages = new Dictionary<SephirahType, List<int>>()
            {
                [SephirahType.Tiphereth] = new List<int>() { 3, 5, 9, 10, 15 },
                [SephirahType.Binah] = new List<int>() { 10, 11, 12, 13 },
                [SephirahType.Hokma] = new List<int>() { 11, 12, 15 },
            };

            // Make a base dict where pages of certain levels will be saved
            Dictionary<SephirahType, List<List<EmotionCardXmlInfo>>> floorAbnoPages = new Dictionary<SephirahType, List<List<EmotionCardXmlInfo>>>()
            {
                [SephirahType.Malkuth] = new List<List<EmotionCardXmlInfo>>(),
                [SephirahType.Yesod] = new List<List<EmotionCardXmlInfo>>(),
                [SephirahType.Hod] = new List<List<EmotionCardXmlInfo>>(),
                [SephirahType.Netzach] = new List<List<EmotionCardXmlInfo>>(),
                [SephirahType.Tiphereth] = new List<List<EmotionCardXmlInfo>>(),
                [SephirahType.Gebura] = new List<List<EmotionCardXmlInfo>>(),
                [SephirahType.Chesed] = new List<List<EmotionCardXmlInfo>>(),
                [SephirahType.Binah] = new List<List<EmotionCardXmlInfo>>(),
                [SephirahType.Hokma] = new List<List<EmotionCardXmlInfo>>(),
                [SephirahType.Keter] = new List<List<EmotionCardXmlInfo>>(),
            };

            // Fill each with 5 lists that correspond to each level
            for (int i = 0; i < 5; i++)
            {
                foreach (var item in floorAbnoPages)
                {
                    item.Value.Add(new List<EmotionCardXmlInfo>());
                }
            }

            // Guarantee placement of certain exodia abno pages in the same floor
            if (SettingsManager.ExodiaGuarantee)
            {
                foreach ((SephirahType seph, List<int> pages) in exodiaPages.Select(x => (x.Key, x.Value)))
                {
                    // Select which seph to put exodias to
                    SephirahType targetSeph = GameUtils.FloorSephs.Where(s => floorAbnoPages[s].Where(l => l.Count <= 2).Count() >= pages.Count).ToList().TakeRandom(Random); // That's a copy so it doesn't matter if we pop

                    foreach (int pid in pages)
                    {
                        EmotionCardXmlInfo exPage = abnoPagePool.FirstOrDefault(p => p.Sephirah == seph && p.id == pid);

                        if (exPage == null)
                            continue;

                        var vacantLevels = floorAbnoPages[targetSeph].Where(l => l.Count <= 2).ToList();
                        var levelList = vacantLevels.TakeRandom(Random);

                        abnoPagePool.Remove(exPage);
                        levelList.Add(exPage);

                        // If pages are randomized by sets, get other pages from same set and place them in same level
                        if (SettingsManager.AbnoPageShuffle != AbnoPageShuffle.Sets)
                            continue;

                        foreach (var page in abnoPagePool.Where(p => p.Sephirah == seph && p.Level == exPage.Level).ToList())
                        {
                            abnoPagePool.Remove(page);
                            levelList.Add(page);
                        }
                    }
                }
            }

            // Place every other abno page
            while (abnoPagePool.Count > 0)
            {
                var vacantLevels = floorAbnoPages.Values.SelectMany(l => l.Where(ll => ll.Count <= 2)).ToList();
                var levelList = vacantLevels.TakeRandom(Random);

                EmotionCardXmlInfo exPage = abnoPagePool.TakeRandom(Random);

                levelList.Add(exPage);

                if (SettingsManager.AbnoPageShuffle != AbnoPageShuffle.Sets)
                    continue;

                foreach (var page in abnoPagePool.Where(p => p.Sephirah == exPage.Sephirah && p.Level == exPage.Level).ToList())
                {
                    abnoPagePool.Remove(page);
                    levelList.Add(page);
                }
            }

            foreach (var (seph, list) in floorAbnoPages.Select(l => (l.Key, l.Value)))
            {
                for (int i = 0; i < 5; i++)
                {
                    foreach (var page in list[i])
                    {
                        page.Level = i + 2;
                        page.Sephirah = seph;
                        abnoPages.Add(page);
                    }
                }
            }

            EmotionCardXmlList.Instance._list = abnoPages;
        }

        private static void RandomizeAbnoPages()
        {
            if (SettingsManager.AbnoPageRandomization == AbnoPageRandomization.None)
                return;

            Debug.Log("[LORAP] Randomizing Abno Pages");

            var Random = GameUtils.CreateRandom("abno_page_randomization");

            foreach (SephirahType seph in GameUtils.FloorSephs)
            {
                // Pool of Levels & States
                List<MentalState> statesPool = new List<MentalState>();
                List<int> levelsPool = new List<int>();

                if (SettingsManager.AbnoPageRandomization == AbnoPageRandomization.Guarantee)
                {
                    // Guarantee 4 of Positive and 4 of Negative pages
                    statesPool.AddRange(new List<MentalState>()
                    {
                        MentalState.Positive, MentalState.Positive, MentalState.Positive, MentalState.Positive,
                        MentalState.Negative, MentalState.Negative, MentalState.Negative, MentalState.Negative,
                    });

                    // Guarantee 3 pages of each emtoion level
                    levelsPool.AddRange(new List<int>()
                    {
                        1, 1, 1,
                        2, 2, 2,
                        3, 3, 3,
                    });
                }

                // Get all the pages for this floor
                List<EmotionCardXmlInfo> pages = EmotionCardXmlList.Instance._list.Where(p => p.Sephirah == seph).ToList();

                // Fill up the pools
                while (statesPool.Count < pages.Count)
                {
                    statesPool.Add(Random.Next(2) == 0 ? MentalState.Negative : MentalState.Positive);
                }

                while (levelsPool.Count < pages.Count)
                {
                    levelsPool.Add(Random.Next(1, 4));
                }

                // Distribute everything
                foreach (EmotionCardXmlInfo page in pages)
                {
                    page.State = statesPool.TakeRandom(Random);
                    page.EmotionLevel = levelsPool.TakeRandom(Random);
                    page.EmotionRate = Random.Next(4) * (page.State == MentalState.Positive ? 1 : -1);
                }
            }
        }

        private static void ShuffleEGOPages()
        {
            if (!SettingsManager.EgoPageShuffle)
            {
                EmotionEgoXmlList.Instance._list = EGOPageInitialList;

                return;
            }

            Debug.Log("[LORAP] Shuffling EGO Pages");

            var Random = GameUtils.CreateRandom("ego_page_shuffle");

            List<EmotionEgoXmlInfo> EGOPages = EGOPageInitialList.ToList();
            List<EmotionEgoXmlInfo> shuffledEGO = new List<EmotionEgoXmlInfo>();

            foreach (SephirahType seph in GameUtils.FloorSephs)
            {
                for (int i = 0; i < 5; i++)
                {
                    EmotionEgoXmlInfo card = EGOPages.TakeRandom(Random);

                    card.Sephirah = seph;
                    shuffledEGO.Add(card);
                }
            }

            EmotionEgoXmlList.Instance._list = shuffledEGO;
        }

        private static void PrepareStages()
        {
            Debug.Log("[LORAP] Preparing different Stages");

            // Reset Ensemble floors & Shuffle Ensemble floors if needed
            List<int> ensembleStages = EnsembleFloorToStage.Values.ToList();
            if (SettingsManager.ShuffleEnsembleFloors)
            {
                System.Random rng = GameUtils.CreateRandom("ensemble_shuffle");
                ensembleStages.Shuffle(rng);
            }

            foreach (SephirahType seph in GameUtils.FloorSephs)
            {
                StageClassInfo stage = StageClassInfoList.Instance.GetData(ensembleStages[(int)seph - 1]);
                stage.floorOnlyList.Clear();
                stage.floorOnlyList.Add(seph);
            }

            // Make stages require books if needed
            foreach (BattleNode node in SlotDataManager.BattleTree.Nodes.Values) // TODO: Write a manager to reset game data (so i don't have to save anything and reset manually)
            {
                StageClassInfo info = StageClassInfoList.Instance.GetData(node.Id);
                if (info == null)
                    continue;

                info.invitationInfo.combine = StageCombineType.BookRecipe;

                if (node.Kind == BattleNodeKind.Reception)
                {
                    info.invitationInfo.needsBooks = SlotDataManager.ReceptionBookRequirements.ContainsKey(node.Id)
                        ? SlotDataManager.ReceptionBookRequirements[node.Id].Select(b => new LorId(b)).ToList()
                        : new List<LorId>();
                }
                else
                {
                    List<LorId> requirements = new List<LorId>();

                    int stageIndex = SlotDataManager.AbnoFightOrder[node.AssignedFloor].IndexOf(node.Id);
                    if (SlotDataManager.AbnoBookRequirements[node.AssignedFloor].Count > stageIndex)
                        requirements = SlotDataManager.AbnoBookRequirements[node.AssignedFloor][stageIndex].Select(b => new LorId(b)).ToList();

                    info.invitationInfo.needsBooks = requirements;

                    // If this was the last stage of keter realization, also make same requirements for every other stage of it
                    if (node.Id == 210009)
                    {
                        for (int i = 210005; i <= 210008; i++)
                        {
                            StageClassInfo keterInfo = StageClassInfoList.Instance.GetData(i);
                            keterInfo.invitationInfo.needsBooks = requirements;
                        }
                    }
                }
            }

            // Make fake BattleNodes for I-IV stages of Keter Realization so that it can ACTUALLY FUCKING WORK
            //BattleNode lastKeterNode = SlotDataManager.BattleTree.GetNodeById(210009);
            //if (lastKeterNode == null)
            //    return;
            //
            //for (int i = 210005; i <= 210008; i++)
            //{
            //    BattleNode keterNode = new BattleNode()
            //    {
            //        Key = $"stage:{i}",
            //        Id = i,
            //        Name = "Keter Realization {i}",
            //        Kind = BattleNodeKind.Stage,
            //        Chapter = lastKeterNode.Chapter,
            //        Sphere = lastKeterNode.Sphere,
            //        SphereLayer = lastKeterNode.SphereLayer,
            //        GlobalLayer = lastKeterNode.GlobalLayer,
            //        RequiredLibrarians = lastKeterNode.RequiredLibrarians,
            //        AssignedFloor = lastKeterNode.AssignedFloor,
            //    };
            //
            //    SlotDataManager.BattleTree.Nodes[keterNode.Key] = keterNode;
            //    foreach (BattleNode prev in SlotDataManager.BattleTree.GetPrevNodesById(210009))
            //    {
            //        prev.Next.Add(keterNode.Key);
            //    }
            //}

            // Make floor stages based on received abno fight order
            List<FloorLevelXmlInfo> floorLevels = new List<FloorLevelXmlInfo>();

            foreach ((SephirahType seph, List<int> abnos) in SlotDataManager.AbnoFightOrder.Select(p => (p.Key, p.Value)))
            {
                for (int i = 1; i <= abnos.Count; i++)
                {
                    int stageId = abnos[i - 1];

                    // If it's Keter realization, before adding it add 1-4 phases of it
                    if (stageId == 210009)
                    {
                        for (int j = 210005; j < 210009; j++, i++)
                        {
                            FloorLevelXmlInfo keterinfo = new FloorLevelXmlInfo();
                            keterinfo.sephirahType = seph;
                            keterinfo.stageId = j;
                            keterinfo.level = i;

                            floorLevels.Add(keterinfo);
                        }
                    }

                    FloorLevelXmlInfo info = new FloorLevelXmlInfo();
                    info.sephirahType = seph;
                    info.stageId = stageId;
                    info.level = i;

                    floorLevels.Add(info);
                }
            }

            FloorLevelXmlList.Instance._list = floorLevels;
        }

        private static void PrepareMap()
        {
            if (!SlotDataManager.HasBattleTree)
                throw new Exception("LORAP battle tree data was not parsed.");

            Debug.Log("[LORAP/MAP] Preparing Map");

            // Convert BattleNodes into MapNodes for convenience
            List<MapNode> mapNodes = SlotDataManager.BattleTree.Nodes.Values.Select(n => new MapNode() { Key = n.Key }).ToList();
            Dictionary<string, MapNode> mapNodeByKey = mapNodes.ToDictionary(n => n.Key, n => n);
            foreach (BattleNode bNode in SlotDataManager.BattleTree.Nodes.Values)
            {
                MapNode mNode = mapNodeByKey[bNode.Key];
                mNode.Next.AddRange(bNode.Next.Select(bn => mapNodeByKey[bn]));
                mNode.Prev.AddRange(mapNodeByKey.Values.Where(n => n.Next.Contains(mNode)));
            }

            // Depending on the selected Progression more, we prepare the map differently.
            if (SettingsManager.RunProgressionMode == ProgressionMode.BattleGraph)
            {
                CreateGraphUsingSugiyama(ref mapNodes, ref mapNodeByKey);
            }
            else
            {
                CreateLayeredGraph(ref mapNodes, ref mapNodeByKey);
            }

            Debug.Log("[LORAP/MAP] Finalizing Map");

            // Space out nodes in every chapter in order to make space for separators
            AddChapterSpacing(mapNodes, mapNodeByKey);

            // Before rendering, place endogal receptions in a cool way (Yeah i know, hardcoding this doesn't look that good but oh well)
            MapNode oliverNode = mapNodeByKey["reception:60002"];
            oliverNode.Next.Clear();

            Vector2 endgoalPos = new Vector2(oliverNode.X, oliverNode.Y);
            endgoalPos += new Vector2(0, mapNodes.Max(n => n.Y) - oliverNode.Y + 220);

            MapNode spacingDummy = new MapNode()
            {
                Key = $"dummy_spacing",
                Prev = new List<MapNode>() { oliverNode },
                X = endgoalPos.x,
                Y = endgoalPos.y,
            };

            oliverNode.Next.Add(spacingDummy);

            mapNodes.Add(spacingDummy);

            // Place every endgoal except for Distorted Ensemble
            Dictionary<int, Vector2> EndgoalPositions = new Dictionary<int, Vector2>()
            {
                [60003] = new Vector2(-240, -110),
                [210009] = new Vector2(-240, 110),
                [60004] = new Vector2(240, 0),
            };

            foreach (var pair in EndgoalPositions)
            {
                string key = $"endgoal:{pair.Key}";
                if (!mapNodeByKey.TryGetValue(key, out MapNode endgoalNode))
                    continue;

                endgoalNode.X = spacingDummy.X + pair.Value.x;
                endgoalNode.Y = spacingDummy.Y + pair.Value.y;

                spacingDummy.Next.Add(endgoalNode);
            }

            // Place Distorted Ensemble in the shape of tree of life
            Dictionary<SephirahType, Vector2> DEPositions = new Dictionary<SephirahType, Vector2>()
            {
                [SephirahType.Malkuth]   = new Vector2(0, 240),
                [SephirahType.Yesod]     = new Vector2(0, 460),
                [SephirahType.Hod]       = new Vector2(-250, 670),
                [SephirahType.Netzach]   = new Vector2(250, 670),
                [SephirahType.Tiphereth] = new Vector2(0, 870),
                [SephirahType.Gebura]    = new Vector2(-250, 1070),
                [SephirahType.Chesed]    = new Vector2(250, 1070),
                [SephirahType.Binah]     = new Vector2(-250, 1320),
                [SephirahType.Hokma]     = new Vector2(250, 1320),
                [SephirahType.Keter]     = new Vector2(0, 1470),
            };
            Dictionary<SephirahType, List<SephirahType>> DELinks = new Dictionary<SephirahType, List<SephirahType>>()
            {
                [SephirahType.Malkuth]   = new List<SephirahType>() { SephirahType.Hod, SephirahType.Yesod, SephirahType.Netzach },
                [SephirahType.Yesod]     = new List<SephirahType>() { SephirahType.Hod, SephirahType.Netzach, SephirahType.Tiphereth },
                [SephirahType.Hod]       = new List<SephirahType>() { SephirahType.Tiphereth, SephirahType.Gebura },
                [SephirahType.Netzach]   = new List<SephirahType>() { SephirahType.Tiphereth, SephirahType.Chesed },
                [SephirahType.Tiphereth] = new List<SephirahType>() { SephirahType.Gebura, SephirahType.Chesed, SephirahType.Keter },
                [SephirahType.Gebura]    = new List<SephirahType>() { SephirahType.Binah, SephirahType.Hokma },
                [SephirahType.Chesed]    = new List<SephirahType>() { SephirahType.Binah, SephirahType.Hokma },
                [SephirahType.Binah]     = new List<SephirahType>() { SephirahType.Keter },
                [SephirahType.Hokma]     = new List<SephirahType>() { SephirahType.Keter },
                [SephirahType.Keter]     = new List<SephirahType>() { },
            };

            if (SettingsManager.Endgoals.GetValue().Contains(Endgoal.ReverberationEnsemble))
            {
                foreach (var pair in DEPositions)
                {
                    string key = $"endgoal:{EnsembleFloorToStage[pair.Key]}";

                    MapNode node = mapNodeByKey[key];
                    node.X = spacingDummy.X + pair.Value.x;
                    node.Y = spacingDummy.Y + pair.Value.y;

                    foreach (SephirahType seph in DELinks[pair.Key])
                    {
                        node.Next.Add(mapNodeByKey[$"endgoal:{EnsembleFloorToStage[seph]}"]);
                    }
                }

                spacingDummy.Next.Add(mapNodeByKey[$"endgoal:{EnsembleFloorToStage[SephirahType.Malkuth]}"]);
            }

            // Render the graph
            RenderGraph(mapNodes);
        }

        private static void CreateGraphUsingSugiyama(ref List<MapNode> mapNodes, ref Dictionary<string, MapNode> mapNodeByKey)
        {
            // Create a graph using Sugiyama network
            Debug.Log("[LORAP/MAP] Creating graph using Sugiyama");

            // Step 1.1 Is skipped because reception tree is always a DAG.
            // Step 1.2 Divide all nodes into layers such that if node A is in layer x, then next node B is in layer x+1
            // BattleNodes are topologically sorted by the server already we just get to use them
            foreach (MapNode node in mapNodes)
            {
                foreach (MapNode next in node.Next)
                {
                    if (next.Y <= node.Y)
                        next.Y = node.Y + 220f;
                }
            }

            // Step 1.3 Make the tree into a "proper hierarchy" by inserting dummy nodes in the skipped layers
            foreach (MapNode node in mapNodes.ToList())
            {
                foreach (MapNode next in node.Next.ToList())
                {
                    int diff = (int)((next.Y - node.Y) / 220 - 1);

                    if (diff <= 0)
                        continue;

                    node.Next.Remove(next);
                    next.Prev.Remove(node);

                    int i = 0;
                    MapNode prev = node;
                    for (int _ = 0; _ < diff; _++)
                    {
                        MapNode dummy = new MapNode()
                        {
                            Key = $"dummy_{node.Key}_{next.Key}_{i}",
                            Prev = new List<MapNode>() { prev },
                            Y = prev.Y + 220f,
                        };
                        mapNodes.Add(dummy);
                        mapNodeByKey[dummy.Key] = dummy;
                        prev.Next.Add(dummy);
                        prev = dummy;
                        i++;
                    }

                    prev.Next.Add(next);
                    next.Prev.Add(prev);
                }
            }

            // Place nodes inside levels
            List<float> levels = mapNodes.Select(n => n.Y).Distinct().OrderBy(n => n).ToList();

            foreach (float level in levels)
            {
                List<MapNode> nodesAtLevel = mapNodes.Where(n => n.Y == level).ToList();

                for (int i = 0; i < nodesAtLevel.Count; i++)
                {
                    nodesAtLevel[i].X = (-120f * (nodesAtLevel.Count - 1)) + (240 * i);
                }
            }

            // Step 2 Minimize edge crossing using the Down-Up Procedure (25 passes)
            for (int _ = 0; _ < 25; _++)
            {
                int i = levels.Count - 1;

                // Go Up
                do
                {
                    // Get nodes at level i-1
                    List<MapNode> nextNodes = mapNodes.Where(n => n.Y == levels[i - 1]).OrderBy(n => n.X).ToList();

                    //Calculate barycenters for every node at that level
                    Dictionary<MapNode, float> barycenters = new Dictionary<MapNode, float>();
                    foreach (MapNode node in nextNodes)
                    {
                        if (node.Next.Count == 0)
                        {
                            barycenters[node] = node.X;
                            continue;
                        }

                        barycenters[node] = node.Next.Sum(n => n.X) / node.Next.Count;
                    }

                    // Sort nodes by barycenters
                    List<MapNode> ordered = nextNodes.OrderBy(n => barycenters[n]).ToList();

                    // Update positions
                    List<float> positions = nextNodes.Select(n => n.X).ToList();
                    for (int j = 0; j < ordered.Count; j++)
                    {
                        ordered[j].X = positions[j];
                    }

                    i--;
                } while (i > 0);

                // Go Down
                do
                {
                    // Get nodes at level i+1
                    List<MapNode> nextNodes = mapNodes.Where(n => n.Y == levels[i + 1]).OrderBy(n => n.X).ToList();

                    //Calculate barycenters for every node at that level
                    Dictionary<MapNode, float> barycenters = new Dictionary<MapNode, float>();
                    foreach (MapNode node in nextNodes)
                    {
                        barycenters[node] = node.Prev.Sum(n => n.X) / node.Prev.Count;
                    }

                    // Sort nodes by barycenters
                    List<MapNode> ordered = nextNodes.OrderBy(n => barycenters[n]).ToList();

                    // Update positions
                    List<float> positions = nextNodes.Select(n => n.X).ToList();
                    for (int j = 0; j < ordered.Count; j++)
                    {
                        ordered[j].X = positions[j];
                    }

                    i++;
                } while (i < levels.Count - 1);
            }

            // Add a dummy node so that rats reception has a tail line
            MapNode ratsNode = mapNodeByKey["reception:2"];
            MapNode tailDummy = new MapNode()
            {
                Key = $"dummy_tail",
                Next = new List<MapNode>() { ratsNode },
                X = ratsNode.X,
                Y = ratsNode.Y - 1000,
            };
            mapNodes.Add(tailDummy);
        }

        private static void CreateLayeredGraph(ref List<MapNode> mapNodes, ref Dictionary<string, MapNode> mapNodeByKey)
        {
            List<List<MapNode>> layers = mapNodes.Where(n => SlotDataManager.BattleTree.GetNode(n.Key).Sphere <= 7).GroupBy(n => SlotDataManager.BattleTree.GetNode(n.Key).GlobalLayer).Select(l => l.ToList()).ToList();
            MapNode prevCenterDummy = new MapNode()
            {
                Key = $"dummy_center_start",
                Y = -1000f,
            };
            mapNodes.Add(prevCenterDummy);
            for (int layer = 0; layer < layers.Count; layer++)
            {
                List<MapNode> nodes = layers[layer];

                // Remove nodes' connections & Place all nodes in the layer
                for (int node = 0; node < nodes.Count; node++)
                {
                    nodes[node].Prev.Clear();
                    nodes[node].Next.Clear();

                    nodes[node].Y = 220f * layer;
                    nodes[node].X = (-120f * (nodes.Count - 1)) + (240f * node);
                }

                // Place dummy nodes for lines
                MapNode centerDummy = new MapNode() {
                    Key = $"dummy_center_{layer}",
                    Y = 220f * layer,
                };
                prevCenterDummy.Next.Add(centerDummy);
                centerDummy.Prev.Add(prevCenterDummy);
                mapNodes.Add(centerDummy);

                MapNode leftDummy = new MapNode()
                {
                    Key = $"dummy_left_{layer}",
                    Y = 220f * layer,
                    X = -120f * (nodes.Count - 1),
                };
                leftDummy.Prev.Add(centerDummy);
                centerDummy.Next.Add(leftDummy);
                mapNodes.Add(leftDummy);

                MapNode rightDummy = new MapNode()
                {
                    Key = $"dummy_right_{layer}",
                    Y = 220f * layer,
                    X = 120f * (nodes.Count - 1),
                };
                rightDummy.Prev.Add(centerDummy);
                centerDummy.Next.Add(rightDummy);
                mapNodes.Add(rightDummy);

                prevCenterDummy = centerDummy;
            }
        }

        private static void AddChapterSpacing(List<MapNode> mapNodes, Dictionary<string, MapNode> mapNodeByKey)
        {
            List<float> sphereStarts = SlotDataManager.BattleTree.Nodes.Values
                .Where(node => node.Sphere > 1 && node.Sphere <= 7 && mapNodeByKey.ContainsKey(node.Key))
                .GroupBy(node => node.Sphere)
                .OrderBy(group => group.Key)
                .Select(group => group.Min(node => mapNodeByKey[node.Key].Y))
                .ToList();

            // Give every sphere header some breathing room, including the first one.
            foreach (MapNode node in mapNodes)
            {
                float originalY = node.Y;
                int boundariesBeforeNode = sphereStarts.Count(start => originalY >= start);
                node.Y += SphereSeparatorGap * (boundariesBeforeNode + 1);
            }
        }

        private static void RenderGraph(List<MapNode> mapNodes)
        {
            UIStoryProgressPanel MapPanel = UIControlManager.Instance.GetInvitationPanel().InvCenterStoryPanel;
            Dictionary<string, MapNode> mapNodeByKey = mapNodes.ToDictionary(n => n.Key, n => n);

            // Clear the map
            foreach (UIStoryProgressIconSlot icon in MapPanel.iconList)
            {
                GameObject.Destroy(icon.gameObject);
                GameObject.Destroy(icon);
            }
            MapPanel.iconList.Clear();

            foreach (GameObject line in NodeLines)
            {
                GameObject.Destroy(line);
            }
            NodeLines.Clear();

            foreach (ChapterSeparator separator in ChapterSeparators)
            {
                GameObject.Destroy(separator.gameObject);
            }
            ChapterSeparators.Clear();

            // Now, render allat
            Dictionary<string, UIStoryProgressIconSlot> icons = new Dictionary<string, UIStoryProgressIconSlot>();

            // Connect nodes
            foreach (MapNode node in mapNodes)
            {
                foreach (MapNode nextNode in node.Next)
                {
                    Vector3 curPos = new Vector3(node.X, node.Y, 0);
                    Vector3 nextPos = new Vector3(nextNode.X, nextNode.Y, 0);
                    Vector3 diff = nextPos - curPos;

                    var line = UnityEngine.Object.Instantiate(LineTemplate, MapPanel.chapterList.First().transform);
                    line.transform.localPosition = new Vector3(0, 140, 0) + curPos + diff / 2;
                    line.transform.right = diff.normalized;
                    line.transform.localScale = new Vector3(diff.magnitude / 220, 1, 1);
                    line.SetActive(true);

                    NodeLines.Add(line);
                }
            }

            // Lines first, headers next. Otherwise the spaghetti lines win the header text.
            RenderChapterSeparators(MapPanel, mapNodeByKey);

            // Place nodes on the map
            foreach (MapNode mapNode in mapNodes)
            {
                if (mapNode.Key.Contains("dummy_"))
                    continue;

                BattleNode node = SlotDataManager.BattleTree.GetNode(mapNode.Key);
                StageClassInfo info = StageClassInfoList.Instance.GetData(node.Id);
                if (info == null)
                    continue;

                UIStoryLine storyline = UIStoryLine.Chapter1;
                if (node.Kind == BattleNodeKind.Reception)
                    storyline = GetStoryLineForStageInfo(info);
                else
                    storyline = GetStoryLineForChapter(Math.Max(1, Math.Min(7, node.Chapter)));

                Vector3 position = new Vector3(mapNode.X, mapNode.Y, 0f);
                icons[node.Key] = PlaceBattleNodeOnMap(node.Id, storyline, position);
            }

            ResizeMap(mapNodes);
        }

        private static void RenderChapterSeparators(UIStoryProgressPanel mapPanel, Dictionary<string, MapNode> mapNodeByKey)
        {
            List<(int Sphere, float MinY, float MaxY)> sphereRanges = SlotDataManager.BattleTree.Nodes.Values
                .Where(node => node.Sphere > 0 && node.Sphere <= 7 && mapNodeByKey.ContainsKey(node.Key))
                .GroupBy(node => node.Sphere)
                .OrderBy(group => group.Key)
                .Select(group => (
                    Sphere: group.Key,
                    MinY: group.Min(node => mapNodeByKey[node.Key].Y),
                    MaxY: group.Max(node => mapNodeByKey[node.Key].Y)))
                .ToList();

            for (int index = 0; index < sphereRanges.Count; index++)
            {
                (int sphere, float minY, float maxY) = sphereRanges[index];
                float separatorY = index == 0
                    ? minY + 30f
                    : (sphereRanges[index - 1].MaxY + minY) / 2f + 140f;

                GameObject separator = GameObject.Instantiate(ChapterSeparatorTemplate, mapPanel.chapterList.First().transform);
                separator.transform.localPosition = new Vector3(
                    SphereSeparatorOffset.x,
                    separatorY + SphereSeparatorOffset.y - (sphere == 1 ? FirstSphereSeparatorExtraDown : 0f),
                    0f);

                ChapterSeparator sepComponent = separator.AddComponent<ChapterSeparator>();
                sepComponent.Chapter = sphere;

                ChapterSeparators.Add(sepComponent);
            }
        }

        internal static void UpdateSphereSeparators()
        {
            foreach (ChapterSeparator separator in ChapterSeparators)
                separator.UpdateSeparator();
        }

        private static void ResizeMap(List<MapNode> mapNodes)
        {
            UIStoryProgressPanel MapPanel = UIControlManager.Instance.GetInvitationPanel().InvCenterStoryPanel;

            MapPanel.posRect.sizeDelta = new Vector2(mapNodes.Max(n => n.X) - mapNodes.Min(n => n.X) + 1800f, mapNodes.Max(n => n.Y) + 1800f);
        }

        private static DropBookXmlInfo CreateCustomBook(int id, string name, string icon)
        {
            var Book = new DropBookXmlInfo();
            Book._id = id;
            Book.workshopName = name;
            Book.workshopID = "lorap";
            Book._bookIcon = icon;

            DropBookXmlList.Instance.AddBookByMod("lorap", new List<DropBookXmlInfo>() { Book });

            CustomBooks[id] = Book;

            return Book;
        }

        private static UIStoryProgressIconSlot PlaceBattleNodeOnMap(int id, UIStoryLine story, Vector3 position)
        {
            UIStoryProgressPanel MapPanel = UIControlManager.Instance.GetInvitationPanel().InvCenterStoryPanel;

            var icon = GameObject.Instantiate(MapIconTemplate, MapPanel.chapterList.First().transform);
            icon.transform.localPosition = position;
            icon.StoryProgressPanel = MapPanel;
            icon.connectLineList = new List<GameObject>();
            icon.storyData = new List<StageClassInfo>() { StageClassInfoList.Instance.GetData(id) };
            icon.currentStory = story;

            var status = new GameObject("Status", typeof(Image));
            status.transform.SetParent(icon.transform);
            status.transform.localPosition = new Vector3(40, 100, 0);
            status.transform.SetSiblingIndex(2);
            status.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
            status.GetComponent<Image>().raycastTarget = false;
            status.SetActive(false);

            MapPanel.iconList.Add(icon);

            return icon;
        }

        private static UIStoryLine GetStoryLineForChapter(int chapter)
        {
            switch (chapter)
            {
                case 1: return UIStoryLine.Chapter1;
                case 2: return UIStoryLine.Chapter2;
                case 3: return UIStoryLine.Chapter3;
                case 4: return UIStoryLine.Chapter4;
                case 5: return UIStoryLine.Chapter5;
                case 6: return UIStoryLine.Chapter6;
                case 7: return UIStoryLine.Chapter7;
                default: return UIStoryLine.Chapter1;
            }
        }

        private static UIStoryLine GetStoryLineForStageInfo(StageClassInfo info)
        {
            if (info == null)
                return UIStoryLine.Chapter1;

            UIStoryLine parsedStoryLine;
            if (!string.IsNullOrWhiteSpace(info.storyType) && Enum.TryParse(info.storyType, out parsedStoryLine))
                return parsedStoryLine;

            return GetStoryLineForChapter(Math.Max(1, Math.Min(7, info.chapter)));
        }
    }
}

using LORAP.Archipelago;
using LORAP.CustomUI;
using LORAP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static BattleUnitInformationUI_PassiveList;

namespace LORAP.Gameplay
{
    internal static class ContentManager
    {
        private class MapNode
        {
            public string Key = "";
            //public MapGraph Graph;
            public List<MapNode> Next = new List<MapNode>();
            public List<MapNode> Prev = new List<MapNode>();
            public float X = 0;
            public float Y = 0;
        }

        //private class MapGraph
        //{
        //    public MapNode FirstNode;
        //    public List<MapNode> Nodes = new List<MapNode>();
        //    public List<MapGraph> NextGraphs = new List<MapGraph>();
        //    public List<MapGraph> PrevGraphs = new List<MapGraph>();
        //    public int Level = 0;
        //}
        //private static List<MapGraph> MapGraphs = new List<MapGraph>();


        private static UIStoryProgressIconSlot MapIconTemplate;
        private static GameObject LineTemplate;

        private static List<MapNode> MapNodes = new List<MapNode>();
        private static List<GameObject> NodeLines = new List<GameObject>();


        internal static Dictionary<int, DropBookXmlInfo> CustomBooks = new Dictionary<int, DropBookXmlInfo>();


        private static List<EmotionCardXmlInfo> AbnoPageInitialList;

        private static List<EmotionEgoXmlInfo> EGOPageInitialList;

        internal static void Init()
        {
            Debug.Log("[LORAP] Initializing Custom Content");

            // Save vanilla lists of Abno and EGO Pages to return or modify later on
            AbnoPageInitialList = EmotionCardXmlList.Instance._list.ToList();
            EGOPageInitialList = EmotionEgoXmlList.Instance._list.ToList();

            // Init UI Utils
            UIUtils.Init();

            // Initialize Custom UI
            APConnectWindow.Init();
            MessagePopup.Init();
            AbnoEgoPagePopup.Init();

            // Modify some parts of the game
            ApplyUIChanges();
            ApplyMapChanges();

            // Add BOE and Booster Pack to book list // TODO: Make book icons
            CreateCustomBook(123456, "Book of Everything");
            CreateCustomBook(123457, "Booster Pack");
        }

        private static void ApplyUIChanges()
        {
            Debug.Log("[LORAP] Applying UI Changes");

            // Make Esc menu above everything else
            GameObject.Find("[Canvas][Script]PopupCanvas").GetComponent<Canvas>().sortingOrder = 90;
            GameObject.Find("[Canvas][Script]PopupCanvas/[Script]PopupManager").transform.SetAsLastSibling();
            Canvas newCanvas = GameObject.Find("[Canvas][Script]PopupCanvas/[Script]PopupManager").AddComponent<Canvas>();
            newCanvas.overrideSorting = true;
            newCanvas.sortingOrder = 100;
            newCanvas.gameObject.AddComponent<GraphicRaycaster>();

            // Change UIFloorPanel: Remove floor level (irrelevant in this mod) & move quest info up, also add more info lines
            UIFloorPanel floorPanel = UI.UIController.Instance.GetUIPanel(UIPanelType.FloorInfo) as UIFloorPanel;
            floorPanel.transform.Find("PanelActiveController/[Rect]Info_Panel/[Rect]LevelBg").gameObject.SetActive(false);
            floorPanel.questPanel.transform.localPosition = new Vector3(-558.4f, 406f, 0);
            floorPanel.questPanel.questSlotsRoot.GetComponent<VerticalLayoutGroup>().spacing = 0;

            for (int i = 0; i < 2; i++)
            {
                GameObject copy = GameObject.Instantiate(floorPanel.questPanel.questSlotsRoot.transform.Find("[Script]Quest_Condition_Slot").gameObject);
                UIFloorQuestSlot slot = copy.GetComponent<UIFloorQuestSlot>();
                slot.cg = copy.GetComponent<CanvasGroup>();
                slot.img_BgFrame = copy.transform.Find("[Image]Bg").GetComponent<Image>();
                slot.img_Icon = copy.transform.Find("[ImageQuest_Condition_Icon").GetComponent<Image>();
                slot.img_lineframe = copy.transform.Find("[Image]Line").GetComponent<Image>();
                slot.txt_QuestName = copy.transform.Find("[Text]Quest_Name").GetComponent<TextMeshProUGUI>();
                slot.txt_QuestProgress = copy.transform.Find("[Text]Quest_Progress").GetComponent<TextMeshProUGUI>();
                copy.transform.parent = floorPanel.questPanel.questSlotsRoot.transform;
                floorPanel.questPanel.questSlotList = floorPanel.questPanel.questSlotList.ToList().Append(slot).ToArray();
            }

            // Hide amount of pages from burning books
            UIBookPanel bookPanel = UI.UIController.Instance.GetUIPanel(UIPanelType.Book) as UIBookPanel;
            foreach (UIRewardEquipPageSlot slot in bookPanel.DropBookInfoPanel.rewardItemList.equipPageSlotList)
            {
                slot.ob_PerAlarm.SetActive(false);
            }
            foreach (UIRewardCardSlot slot in bookPanel.DropBookInfoPanel.rewardItemList.cardSlotList)
            {
                slot.ob_peralarm.SetActive(false);
            }

            // Hide "Reset Rewards" button from the book burning screen
            UIShowUsingBookInfoPanel dropBookPanel = (UI.UIController.Instance.Panels.ElementAt(3) as UIBookPanel).DropBookInfoPanel;
            dropBookPanel.button_rewardResetButton.gameObject.SetActive(false);

            // Add more slots (4 -> 16) for books in the passive succession menu
            UIPassiveSuccessionEquipBookList passiveBookList = UIPassiveSuccessionPopup.Instance.equipBookList;

            // Enable masking for left book panel
            passiveBookList.rect_ViewPort.GetComponent<Mask>().enabled = true;

            // Add slots to left book panel
            for (int i = 0; i < 12; i++)
            {
                GameObject copy = GameObject.Instantiate(passiveBookList.bookslotlist[0].gameObject, passiveBookList.bookslotlist[0].transform.parent);
                passiveBookList.bookslotlist[0].transform.parent.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 28);
                passiveBookList.bookslotlist.Add(copy.GetComponent<UIPassiveSuccessionEquipBookSlot>());
            }

            // Add event to left book slots for scrolling to pass it through to the scrollrect
            ScrollRect bookListRect = passiveBookList.rect_ViewPort.parent.GetComponent<ScrollRect>();
            foreach (var slot in passiveBookList.bookslotlist)
            {
                EventTrigger evt = slot.GetComponentInChildren<EventTrigger>();

                evt.AddCallback(EventTriggerType.Scroll, (data) => { bookListRect.OnScroll((PointerEventData)data); });
            }

            // Add slots to center bool panel
            UIPassiveSuccessionCenterPanel centerBookList = UIPassiveSuccessionPopup.Instance.centerBookListPanel;
            for (int i = 0; i < 12; i++)
            {
                GameObject slot = centerBookList.rect_bookSlotsLayout.GetChild(0).gameObject;
                GameObject.Instantiate(slot, slot.transform.parent);
            }

            // Add more left panel passive slots
            UIPassiveSuccessionList passiveList = UIPassiveSuccessionPopup.Instance.equipPassiveList;
            for (int i = 0; i < 40; i++)
            {
                GameObject slot = passiveList.rect_slotsLayout.transform.GetChild(0).gameObject;
                GameObject.Instantiate(slot, slot.transform.parent);
            }

            // Add slots to library unit info
            UICardPanel cardPanel = UI.UIController.Instance.GetUIPanel(UIPanelType.Page) as UICardPanel;
            UISetInfoSlotListSc charPassiveSlotList = cardPanel.librarianInfoPanel.passiveSlotsPanel;
            for (int i = 0; i < 46; i++)
            {
                GameObject slot = charPassiveSlotList._slotList[0].gameObject;
                GameObject copy = GameObject.Instantiate(slot, slot.transform.parent);
                charPassiveSlotList._slotList.Add(copy.GetComponent<UILibrarianEquipInfoSlot>());
            }

            // Add slots to enemy and library unit passive list in battle
            BattleUnitInformationUI_PassiveList enemyPassive = BattleManagerUI.Instance.ui_unitInformation.passivelistManager;
            for (int i = 0; i < 46; i++) // I HATE PROJECT MOON CODING
            {
                GameObject slot = enemyPassive.passiveSlotList[0].Rect.gameObject;
                GameObject copy = GameObject.Instantiate(slot, slot.transform.parent);

                BattleUnitInformationPassiveSlot ps = new BattleUnitInformationPassiveSlot();
                ps.Rect = copy.GetComponent<RectTransform>();
                ps.txt_PassiveDesc = copy.GetComponentInChildren<TextMeshProUGUI>();
                ps.img_Icon = copy.transform.Find("[Image]Icon").GetComponent<Image>();
                ps.img_IconGlow = copy.transform.Find("[Image]IconGlow").GetComponent<Image>();

                enemyPassive.passiveSlotList.Add(ps);
            }

            BattleUnitInformationUI_PassiveList playerPassive = BattleManagerUI.Instance.ui_unitInformationPlayer.passivelistManager;
            for (int i = 0; i < 46; i++) // I FUCKING HATE IT
            {
                GameObject slot = playerPassive.passiveSlotList[0].Rect.gameObject;
                GameObject copy = GameObject.Instantiate(slot, slot.transform.parent);

                BattleUnitInformationPassiveSlot ps = new BattleUnitInformationPassiveSlot();
                ps.Rect = copy.GetComponent<RectTransform>();
                ps.txt_PassiveDesc = copy.GetComponentInChildren<TextMeshProUGUI>();
                ps.img_Icon = copy.transform.Find("[Image]Icon").GetComponent<Image>();
                ps.img_IconGlow = copy.transform.Find("[Image]IconGlow").GetComponent<Image>();

                playerPassive.passiveSlotList.Add(ps);
            }

            // TODO: Change Icon for the library level in the level progress bar to AP icon
        }

        private static void ApplyMapChanges()
        {
            Debug.Log("[LORAP] Applying Map Changes");

            UIStoryProgressPanel MapPanel = (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel;

            // Save an icon to later use as a template and hide it
            MapIconTemplate = MapPanel.iconList.First();
            LineTemplate = MapIconTemplate.connectLineList.First();
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

            // Hide shortcut buttons on map
            foreach (UIStoryGradeFilterSlot filter in MapPanel.gradeFilter.gradeSlots)
            {
                filter.gameObject.SetActive(false);
            }
            MapPanel.gradeFilter.gradeSlots.Clear();
        }

        internal static void SetupRunContent()
        {
            Debug.Log("[LORAP] Initializing Run");

            ShuffleAbnoPages();

            RandomizeAbnoPages();

            ShuffleEGOPages();

            PrepareSuppressions();

            PrepareMap();

            // This exists solely to make Keter Realization ACTUALLY FUCKING WORK
            BattleNode lastKeterNode = SlotDataManager.BattleTree.GetNodeById(210009);
            for (int i = 210005; i <= 210008; i++)
            {
                BattleNode keterNode = new BattleNode()
                {
                    Key = $"stage:{i}",
                    Id = i,
                    Name = "Keter Realization {i}",
                    Kind = BattleNodeKind.Stage,
                    Chapter = lastKeterNode.Chapter,
                    RequiredLibrarians = lastKeterNode.RequiredLibrarians,
                    AssignedFloor = lastKeterNode.AssignedFloor,
                };

                SlotDataManager.BattleTree.Nodes[keterNode.Key] = keterNode;
                foreach (BattleNode prev in SlotDataManager.BattleTree.GetPrevNodesById(210009))
                {
                    prev.Next.Add(keterNode.Key);
                }
            }
        }

        private static void ShuffleAbnoPages()
        {
            // If we don't wanna shuffle pages, just ensure that list is same as vanilla
            if (SlotDataManager.AbnoPageShuffle == AbnoPageShuffle.None)
            {
                EmotionCardXmlList.Instance._list = AbnoPageInitialList.ToList();

                return;
            }

            Debug.Log("[LORAP] Shuffling Abno Pages");

            // Randomize Abno Pages' floors
            var Random = SlotDataManager.CreateRandom("abno_page_shuffle");

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
            if (SlotDataManager.AbnoPageShuffle == AbnoPageShuffle.InFloor)
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
            if (SlotDataManager.ExodiaGuarantee)
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
                        if (SlotDataManager.AbnoPageShuffle != AbnoPageShuffle.Sets)
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

                if (SlotDataManager.AbnoPageShuffle != AbnoPageShuffle.Sets)
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
            if (SlotDataManager.AbnoPageRandomization == AbnoPageRandomization.None)
                return;

            Debug.Log("[LORAP] Randomizing Abno Pages");

            var Random = SlotDataManager.CreateRandom("abno_page_randomization");

            foreach (SephirahType seph in GameUtils.FloorSephs)
            {
                // Pool of Levels & States
                List<MentalState> statesPool = new List<MentalState>();
                List<int> levelsPool = new List<int>();

                if (SlotDataManager.AbnoPageRandomization == AbnoPageRandomization.Guarantee)
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
            if (SlotDataManager.EgoPageShuffle == false)
            {
                EmotionEgoXmlList.Instance._list = EGOPageInitialList;

                return;
            }

            Debug.Log("[LORAP] Shuffling EGO Pages");

            var Random = SlotDataManager.CreateRandom("ego_page_shuffle");

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

        private static void PrepareSuppressions()
        {
            Debug.Log("[LORAP] Preparing Suppressions and Realizations");

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
            MapNodes = SlotDataManager.BattleTree.Nodes.Values.Select(n => new MapNode() { Key = n.Key }).ToList();
            Dictionary<string, MapNode> mapNodeByKey = MapNodes.ToDictionary(n => n.Key, n => n);
            foreach (BattleNode bNode in SlotDataManager.BattleTree.Nodes.Values)
            {
                MapNode mNode = mapNodeByKey[bNode.Key];
                mNode.Next.AddRange(bNode.Next.Select(bn => mapNodeByKey[bn]));
                mNode.Prev.AddRange(mapNodeByKey.Values.Where(n => n.Next.Contains(mNode)));
            }

            // UNFINISHED MAP CODE MIGHT USE LATER
            /*

            // Divide the nodes into graphs
            Debug.Log("[LORAP/MAP] Parsing nodes into graphs");
            List<MapGraph> graphs = new List<MapGraph>();

            // Add nodes recursively to the graph, if encountered a node with two or more prevous nodes, make it a new graph and do same shit to it
            void ParseGraph(MapGraph graph)
            {
                bool finished = false;
                List<MapNode> queue = new List<MapNode>() { graph.FirstNode };
                List<MapNode> potentialGraphs = new List<MapNode>();

                while (!finished)
                {
                    Debug.Log($"[LORAP/MAP] {graph.FirstNode.Key} Parse Loop...");
                    // Parse the queue
                    while (queue.Count > 0)
                    {
                        MapNode node = queue.Pop();

                        // Check it's children
                        foreach (MapNode nextNode in node.Next)
                        {
                            if (nextNode.Prev.Count > 1 & !potentialGraphs.Contains(nextNode))
                            {
                                potentialGraphs.Add(nextNode);
                            }
                            else
                            {
                                graph.Nodes.Add(nextNode);
                                nextNode.Graph = graph;
                                queue.Add(nextNode);
                            }
                        }
                    }

                    // Check potential graphs
                    Debug.Log($"[LORAP/MAP] {graph.FirstNode.Key} Checking potential graphs...");
                    foreach (MapNode node in potentialGraphs)
                    {
                        // It's a starter node of another graph, check if it already has a graph and if not make one
                        MapGraph nextGraph = graphs.FirstOrDefault(g => g.Nodes.Contains(node));
                        if (nextGraph == null)
                        {
                            nextGraph = new MapGraph() { Nodes = { node }, FirstNode = node };
                            graph.NextGraphs.Add(nextGraph);
                            ParseGraph(nextGraph);
                        }

                        if (!nextGraph.PrevGraphs.Contains(graph))
                            nextGraph.PrevGraphs.Add(graph);
                    }
                    potentialGraphs.Clear();

                    // If after checking potential graphs the queue is empty, we're done with this graph
                    if (queue.Count == 0)
                        finished = true;
                }

                // Add this graph to the list
                graphs.Add(graph);
            }

            // Parse nodes
            MapNode startNode = mapNodeByKey[SlotDataManager.BattleTree.GetNodeById(SlotDataManager.FirstReception).Key];
            ParseGraph(new MapGraph() { Nodes = new List<MapNode>() { startNode }, FirstNode = startNode });

            // Remove unnecessary subgraphs by checking if any subgraph startnode has all previous nodes from same graph, add that subgraph's node to the previous graph
            //Debug.Log("[LORAP/MAP] Removing unnecessary subgraphs & connecting subgraphs");
            //foreach (MapGraph graph in graphs.ToList())
            //{
            //    if (graph.PrevGraphs.Count != 1)
            //        continue;
            //
            //    MapGraph prevGraph = graph.PrevGraphs[0];
            //
            //    foreach (MapNode node in graph.Nodes)
            //    {
            //        node.Graph = prevGraph;
            //    }
            //
            //    prevGraph.Nodes.AddRange(graph.Nodes);
            //    graph.Nodes.Clear();
            //
            //    prevGraph.NextGraphs.Remove(graph);
            //    prevGraph.NextGraphs.AddRange(graph.NextGraphs);
            //
            //    foreach (MapGraph nextGraph in graph.NextGraphs)
            //    {
            //        nextGraph.PrevGraphs.Remove(graph);
            //        nextGraph.PrevGraphs.Add(prevGraph);
            //    }
            //
            //    graphs.Remove(graph);
            //}

            MapGraphs = graphs;

            // Place graphs on the map
            Debug.Log("[LORAP/MAP] Placing subgraphs on the map");

            // Figure out levels
            List<MapGraph> graphQueue = new List<MapGraph>() { graphs.First(g => g.FirstNode == startNode) };
            while (graphQueue.Count > 0)
            {
                MapGraph graph = graphQueue.Pop();

                foreach (MapGraph next in graph.NextGraphs)
                {
                    next.Level = Math.Max(next.Level, graph.Level + 1);
                    graphQueue.Add(next);
                }
            }

            // Split them inside levels
            foreach (int level in graphs.Select(g => g.Level).Distinct())
            {
                List<MapGraph> levelGraphs = graphs.Where(g => g.Level == level).ToList();

                for (int i = 0; i < levelGraphs.Count; i++)
                {
                    MapGraph graph = levelGraphs[i];
                    MapNode start = graph.FirstNode;
                    start.Y = graph.Level * 4000;
                    start.X = (-750 * levelGraphs.Count) + (1500 * i);
                }
            }

            // Figure out nodes' positions within each graph
            Debug.Log("[LORAP/MAP] Placing nodes within subgraphs");
            foreach (MapGraph graph in graphs)
            {
                Debug.Log("[LORAP/MAP] dummy1");
                // Add dummy nodes to make a single path between this and next graph
                foreach (MapGraph nextGraph in graph.NextGraphs)
                {
                    List<MapNode> prevNodesOfThisGraph = nextGraph.FirstNode.Prev.Where(n => graph.Nodes.Contains(n)).ToList();

                    if (prevNodesOfThisGraph.Count <= 1)
                        continue;

                    MapNode dummy = new MapNode() { Key = $"dummy_prev_to_{nextGraph.FirstNode.Key}" };
                    MapNodes.Add(dummy);
                    mapNodeByKey[dummy.Key] = dummy;
                    graph.Nodes.Add(dummy);
                    dummy.Graph = graph;
                    dummy.Prev.AddRange(prevNodesOfThisGraph);
                    dummy.Next.Add(nextGraph.FirstNode);
                    nextGraph.FirstNode.Prev.RemoveAll(p => prevNodesOfThisGraph.Contains(p));

                    foreach (MapNode prevNode in prevNodesOfThisGraph)
                    {
                        prevNode.Next.Remove(nextGraph.FirstNode);
                        prevNode.Next.Add(dummy);
                    }
                }

                // Divide them into layers above the start node
                List<MapNode> queue = new List<MapNode>() { graph.FirstNode };

                while (queue.Count > 0)
                {
                    MapNode node = queue.Pop();

                    foreach (MapNode nextNode in node.Next)
                    {
                        if (nextNode.Graph != graph)
                            continue;

                        nextNode.Y = Math.Max(nextNode.Y, node.Y + 220);
                        queue.Add(nextNode);
                    }
                }
                Debug.Log("[LORAP/MAP] dummy2");
                // Add dummy nodes to make graph into a proper hierarchy
                foreach (MapNode node in graph.Nodes.ToList())
                {
                    foreach (MapNode nextNode in node.Next.ToList())
                    {
                        if (nextNode.Graph != graph)
                            continue;
                
                        int diff = (int)(nextNode.Y - node.Y) / 220 - 1;
                
                        if (diff <= 0)
                            continue;
                
                        node.Next.Remove(nextNode);
                        nextNode.Prev.Remove(node);
                
                        MapNode prev = node;
                        for (int i = 0; i < diff; i++)
                        {
                            MapNode dummy = new MapNode() { Key = $"dummy_{node.Key}_to_{nextNode.Key}{i}" };
                            MapNodes.Add(dummy);
                            mapNodeByKey[dummy.Key] = dummy;
                            graph.Nodes.Add(dummy);
                            dummy.Graph = graph;
                            dummy.Y = prev.Y + 220;
                            dummy.Prev.Add(prev);
                            prev.Next.Add(dummy);
                            prev = dummy;
                        }
                
                        prev.Next.Add(nextNode);
                        nextNode.Prev.Add(prev);
                    }
                }
                Debug.Log("[LORAP/MAP] dummy3");
                // Add even more dummy nodes so that outgoing connections happen on the top of the graph
                List<float> nodeYs = graph.Nodes.Select(g => g.Y).Distinct().OrderBy(y => y).ToList();
                float maxY = nodeYs.Max();

                foreach (MapNode node in graph.Nodes.Where(n => n.Next.Any(nn => nn.Graph != graph) && n.Y < maxY).ToList())
                {
                    int diff = (int)(maxY - node.Y) / 220;
                
                    if (diff < 1)
                        continue;
                
                    List<MapNode> otherGraphNextNodes = node.Next.Where(n => n.Graph != graph).ToList();
                    node.Next.RemoveAll(n => otherGraphNextNodes.Contains(n));
                
                    otherGraphNextNodes.ForEach(n => n.Prev.Remove(node));
                
                    MapNode prev = node;
                    for (int i = 0; i < diff; i++)
                    {
                        MapNode dummy = new MapNode() { Key = $"dummy_from_{node.Key}{i}" };
                        MapNodes.Add(dummy);
                        mapNodeByKey[dummy.Key] = dummy;
                        graph.Nodes.Add(dummy);
                        dummy.Graph = graph;
                        dummy.Y = prev.Y + 220;
                        dummy.Prev.Add(prev);
                        prev.Next.Add(dummy);
                        prev = dummy;
                    }
                
                    prev.Next.AddRange(otherGraphNextNodes);
                    otherGraphNextNodes.ForEach(n => n.Prev.Add(prev));
                }

                Debug.Log("[LORAP/MAP] xpos");
                // Figure out nodes' X position in each layer
                foreach (int y in graph.Nodes.Select(g => g.Y).Distinct())
                {
                    List<MapNode> levelNodes = graph.Nodes.Where(g => g.Y == y).ToList();

                    for (int i = 0; i < levelNodes.Count; i++)
                    {
                        MapNode node = levelNodes[i];
                        node.X = graph.FirstNode.X + (-120f * (levelNodes.Count - 1)) + (240 * i);
                    }
                }
                Debug.Log("[LORAP/MAP] edgecrossing");
                // Minimize edge-crossing (5 times for every graph)
                if (nodeYs.Count <= 1)
                    continue;

                for (int _ = 0; _ < 10; _++)
                {
                    int i = 0;
                    bool changed = false;

                    // Go Down
                    do
                    {
                        // Get nodes at level i+1
                        List<MapNode> nextNodes = graph.Nodes.Where(n => n.Y == nodeYs[i + 1]).OrderBy(n => n.X).ToList();

                        //Calculate barycenters for every node at that level
                        Dictionary<MapNode, float> barycenters = new Dictionary<MapNode, float>();
                        foreach (MapNode node in nextNodes)
                        {
                            barycenters[node] = (float)node.Prev.Sum(n => n.X) / node.Prev.Count;
                        }

                        // Sort nodes by barycenters
                        List<float> positions = nextNodes.Select(n => n.X).ToList();
                        List<MapNode> ordered = nextNodes.OrderBy(n => barycenters[n]).ToList();

                        // Update positions
                        for (int j = 0; j < ordered.Count; j++)
                        {
                            ordered[j].X = positions[j];
                        }

                        i++;
                    } while (i < nodeYs.Count - 1);

                    // Go Up
                    do
                    {
                        // Get nodes at level i-1
                        List<MapNode> nextNodes = graph.Nodes.Where(n => n.Y == nodeYs[i - 1]).OrderBy(n => n.X).ToList();

                        //Calculate barycenters for every node at that level
                        Dictionary<MapNode, float> barycenters = new Dictionary<MapNode, float>();
                        foreach (MapNode node in nextNodes)
                        {
                            if (node.Next.Count == 0)
                            {
                                barycenters[node] = node.X;
                                continue;
                            }

                            barycenters[node] = (float)node.Next.Sum(n => n.X) / node.Next.Count;
                        }

                        // Sort nodes by barycenters
                        List<float> positions = nextNodes.Select(n => n.X).ToList();
                        List<MapNode> ordered = nextNodes.OrderBy(n => barycenters[n]).ToList();

                        // Update positions
                        for (int j = 0; j < ordered.Count; j++)
                        {
                            ordered[j].X = positions[j];
                        }

                        i--;
                    } while (i > 0);

                    if (!changed)
                        break;
                }
            }




            // Resize for a test
            UIStoryProgressPanel MapPanel = (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel;
            MapPanel.posRect.sizeDelta = new Vector2(10000, 20000);

            */

            // Create a graph using Sugiyama network
            Debug.Log("[LORAP] Graph Creation Step 1");
            // Step 1.1 Is skipped because reception tree is always a DAG.
            // Step 1.2 Divide all nodes into layers such that if node A is in layer x, then next node B is in layer x+1
            // BattleNodes are topologically sorted by the server already we just get to use them
            foreach (MapNode node in MapNodes)
            {
                foreach (MapNode next in node.Next)
                {
                    if (next.Y <= node.Y)
                        next.Y = node.Y + 220f;
                }
            }
            
            // Step 1.3 Make the tree into a "proper hierarchy" by inserting dummy nodes in the skipped layers
            foreach (MapNode node in MapNodes.ToList())
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
                            Key = $"dummy_{node.Key}_{i}",
                            Prev = new List<MapNode>() { prev },
                            Y = prev.Y + 220f,
                        };
                        MapNodes.Add(dummy);
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
            List<float> levels = MapNodes.Select(n => n.Y).Distinct().OrderBy(n => n).ToList();

            foreach (float level in levels)
            {
                List<MapNode> nodesAtLevel = MapNodes.Where(n => n.Y == level).ToList();

                for (int i = 0; i < nodesAtLevel.Count; i++)
                {
                    nodesAtLevel[i].X = (-120f * (nodesAtLevel.Count - 1)) + (240 * i);
                }
            }

            Debug.Log("[LORAP] Graph Creation Step 2");
            // Step 2 Minimize edge crossing using the Down-Up Procedure (15 passes)
            for (int _ = 0; _ < 15; _++)
            {
                int i = 0;

                // Go Down
                do
                {
                    // Get nodes at level i+1
                    List<MapNode> nextNodes = MapNodes.Where(n => n.Y == levels[i + 1]).OrderBy(n => n.X).ToList();

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

                // Go Up
                do
                {
                    // Get nodes at level i-1
                    List<MapNode> nextNodes = MapNodes.Where(n => n.Y == levels[i - 1]).OrderBy(n => n.X).ToList();

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
            }

            Debug.Log("[LORAP] Graph Creation Step 3");
            // Step 3 Make the graph real

            // Render the graph
            RenderGraph();
        }

        private static void RenderGraph()
        {
            UIStoryProgressPanel MapPanel = (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel;
            Dictionary<string, MapNode> mapNodeByKey = MapNodes.ToDictionary(n => n.Key, n => n);

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

            // Now, render allat
            Dictionary<string, UIStoryProgressIconSlot> icons = new Dictionary<string, UIStoryProgressIconSlot>();

            // Connect nodes
            foreach (MapNode node in MapNodes)
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

            // Place nodes on the map
            foreach (MapNode mapNode in MapNodes)
            {
                if (mapNode.Key.Contains("dummy_"))
                    continue;

                BattleNode node = SlotDataManager.BattleTree.GetNode(mapNode.Key);
                StageClassInfo info = StageClassInfoList.Instance.GetData(node.Id);
                if (info == null)
                    continue;

                UIStoryLine storyline = UIStoryLine.Chapter1;
                if (node.Kind == BattleNodeKind.Reception)
                {
                    storyline = GetStoryLineForStageInfo(info);
                    info.invitationInfo.needsBooks = SlotDataManager.ReceptionBookRequirements.ContainsKey(node.Id)
                        ? SlotDataManager.ReceptionBookRequirements[node.Id].Select(b => new LorId(b)).ToList()
                        : new List<LorId>();
                }
                else
                {
                    storyline = GetStoryLineForChapter(Math.Max(1, Math.Min(7, node.Chapter)));

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

                Vector3 position = new Vector3(mapNode.X, mapNode.Y, 0f);
                icons[node.Key] = PlaceBattleNodeOnMap(node.Id, storyline, position);
            }

            ResizeMap();
        }

        private static void ResizeMap()
        {
            UIStoryProgressPanel MapPanel = (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel;

            MapPanel.posRect.sizeDelta = new Vector2(MapNodes.Max(n => n.X) - MapNodes.Min(n => n.X) + 1800f, MapNodes.Max(n => n.Y) + 1800f);
        }

        private static DropBookXmlInfo CreateCustomBook(int id, string name)
        {
            var Book = new DropBookXmlInfo(); // TODO: AP Icons
            Book._id = id;
            Book.workshopName = name;
            Book.workshopID = "lorap";
            //Book.DropNum = dropNum;   
            //Book.DropItemList = dropList;
            //Singleton<DropBookXmlList>.Instance._list.Add(Book);
            //Singleton<DropBookXmlList>.Instance._dict.Add(Book.id, Book);
            //Singleton<DropBookXmlList>.Instance._workshopDict["lorap"].Add(Book);
            DropBookXmlList.Instance.AddBookByMod("lorap", new List<DropBookXmlInfo>() { Book });

            CustomBooks[id] = Book;

            return Book;
        }

        private static UIStoryProgressIconSlot PlaceBattleNodeOnMap(int id, UIStoryLine story, Vector3 position)
        {
            UIStoryProgressPanel MapPanel = (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel;

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
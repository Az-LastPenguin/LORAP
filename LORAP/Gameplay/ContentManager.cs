using LORAP.Archipelago;
using LORAP.CustomUI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace LORAP.Gameplay
{
    internal class ReceptionNode
    {
        public int id;

        public List<int> next;

        public int y;
    }

    internal class ReceptionTree
    {
        public List<ReceptionNode> Nodes = new List<ReceptionNode>();

        public Dictionary<int, List<int>> NodesOfDepth {
            get
            {
                Dictionary<int, List<int>> result = new Dictionary<int, List<int>>();

                foreach (var node in Nodes) 
                {
                    if (!result.ContainsKey(node.y))
                        result[node.y] = new List<int>();

                    result[node.y].Add(node.id);
                }

                return result;
            }
            set 
            {
                NodesOfDepth = value;
            } 
        }

        public int First;

        public int Last;

        
        public ReceptionNode GetNode(int id) => Nodes.Find(n => n.id == id);

        public List<ReceptionNode> GetPrevNodes(int id) => Nodes.Where(n => n.next.Contains(id)).ToList();
    }

    internal static class ContentManager
    {
        internal static Dictionary<int, DropBookXmlInfo> CustomBooks = new Dictionary<int, DropBookXmlInfo>();

        private static List<EmotionCardXmlInfo> AbnoPageInitialList;

        private static List<EmotionEgoXmlInfo> EGOPageInitialList;

        private static List<UIStoryProgressIconSlot> VanillaIconsBackup = new List<UIStoryProgressIconSlot>();

        private static UIStoryProgressIconSlot MapIconTemplate;
        private static GameObject LineTemplate;
        private static GameObject CheckmarkIconTemplate = UICardListDetailFilterPopup.Instance.transform.Find("[Image]Frame/Scroll View/Viewport/Content/RarityGroup/Group/[Toggle]DetailSlot/[Toggle]SelectableToggle/[Image]IconGlow").gameObject;

        private static DropBookXmlInfo CreateCustomBook(int id, string name, int dropNum, List<BookDropItemInfo> dropList)
        {
            var Book = new DropBookXmlInfo();
            Book._id = id;
            Book.workshopName = name;
            Book.workshopID = "lorap";
            Book.DropNum = dropNum;
            Book.DropItemList = dropList;
            Singleton<DropBookXmlList>.Instance._list.Add(Book);
            Singleton<DropBookXmlList>.Instance._dict.Add(Book.id, Book);

            CustomBooks[id] = Book;

            return Book;
        }

        private static UIStoryProgressIconSlot PlaceReceptionOnMap(int id, UIStoryLine story, Vector3 position)
        {
            UIStoryProgressPanel MapPanel = (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel;

            var icon = UnityEngine.Object.Instantiate(MapIconTemplate, MapPanel.chapterList.First().transform);
            icon.transform.localPosition = position;
            icon.StoryProgressPanel = MapPanel;
            icon.connectLineList = new List<GameObject>();
            icon.storyData = new List<StageClassInfo>() { StageClassInfoList.Instance.GetData(id) };
            icon.currentStory = story;

            var check = UnityEngine.Object.Instantiate(CheckmarkIconTemplate, icon.transform);
            check.transform.localPosition = new Vector3(30, 100, 0);
            check.name = "Checkmark";
            check.transform.SetSiblingIndex(2);

            MapPanel.iconList.Add(icon);

            return icon;
        }

        internal static void SetupRunContent()
        {
            var Random = new System.Random(SlotDataManager.Seed);

            // Setup Reception Tree
            UIStoryProgressPanel MapPanel = (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel;
            if (SlotDataManager.RandomizeReceptionTree && (SlotDataManager.ReceptionsProgression == ReceptionsProgression.Progressive || SlotDataManager.ReceptionsProgression == ReceptionsProgression.ProgressiveBooks))
            {
                Dictionary<int, List<int>> Depths = SlotDataManager.ReceptionTree.NodesOfDepth;

                Dictionary<string, UIStoryLine> storylines = Enum.GetValues(typeof(UIStoryLine)).Cast<UIStoryLine>().ToDictionary(e => e.ToString(), e => e);

                Dictionary<int, UIStoryProgressIconSlot> icons = new Dictionary<int, UIStoryProgressIconSlot>();
                foreach (var pair in Depths)
                {
                    int cur_depth = pair.Key;
                    int nodes_num = pair.Value.Count;
                    int cur_node = 1;
                    foreach (int id in pair.Value)
                    {
                        StageClassInfo info = StageClassInfoList.Instance.GetData(id);
                        UIStoryLine storyline = storylines.Where(p => p.Key == info.storyType).First().Value;

                        icons[id] = PlaceReceptionOnMap(id, storyline, new Vector3(-(240 * (nodes_num - 1)) / 2 + (240 * (cur_node - 1)), -220 + cur_depth * 220, 0));

                        // Add recipes
                        info.invitationInfo.needsBooks = SlotDataManager.ReceptionBookRequirements[id].Select(b => new LorId(b)).ToList();

                        cur_node++;
                    }
                }

                foreach (var pair in icons)
                {
                    int id = pair.Key;
                    UIStoryProgressIconSlot icon = pair.Value;
                    icon.connectLineList.Clear();
                    // Create Paths
                    foreach (int next in SlotDataManager.ReceptionTree.GetNode(id).next)
                    {
                        UIStoryProgressIconSlot nextIcon = icons[next];
                        var line = UnityEngine.Object.Instantiate(LineTemplate, icon.transform.Find("[Rect]Lines"));
                        line.transform.localPosition = (nextIcon.transform.localPosition - icon.transform.localPosition)/2;
                        line.transform.right = (nextIcon.transform.localPosition - icon.transform.localPosition).normalized;
                        line.transform.localScale = new Vector3((nextIcon.transform.localPosition - icon.transform.localPosition).magnitude / 220, 1,1);
                        line.SetActive(true);

                        icon.connectLineList.Add(line);
                    }
                }

                // TODO Place endgoals after Oliver
            }
            else if (SlotDataManager.ReceptionsProgression == ReceptionsProgression.Unlocked || SlotDataManager.ReceptionsProgression == ReceptionsProgression.Books)
            {
                // Place receptions randomly on the map
                // To do this we select same amount of points as there are receptions and randomly place them on the map,
                // also move them far enough from eachother
                int TotalReceptions = SlotDataManager.ReceptionTree.Nodes.Count;
                List<Vector2> points = new List<Vector2>();

                for (int i = 0; i < TotalReceptions; i++)
                {
                    points.Add(new Vector2(Random.Next(-1000, 1000), Random.Next(-220, 3000)));
                }

                for (int k = 0; k < 10; k++)
                {
                    for (int i = 0; i < TotalReceptions; i++)
                    {
                        // Check every point that it's more than 220 units further than any other point
                        for (int j = 0; j < TotalReceptions; j++)
                        {
                            if (i == j) continue;
                            var point = points[i];
                            var point2 = points[j];

                            if ((point2 - point).magnitude < 220f)
                            {
                                float coef = 1f - (point2 - point).magnitude / 219f;
                                Vector2 dir = point2 - point;
                                point2 += dir * coef/2;
                                point -= dir * coef/2;

                                points[i] = point;
                                points[j] = point2;
                            }
                        }
                    }
                }

                Dictionary<string, UIStoryLine> storylines = Enum.GetValues(typeof(UIStoryLine)).Cast<UIStoryLine>().ToDictionary(e => e.ToString(), e => e);
                for (int i = 0; i < TotalReceptions; i++)
                {
                    ReceptionNode node = SlotDataManager.ReceptionTree.Nodes[i];
                    Vector2 vector = points[i];

                    StageClassInfo info = StageClassInfoList.Instance.GetData(node.id);
                    UIStoryLine storyline = storylines.Where(p => p.Key == info.storyType).First().Value;

                    PlaceReceptionOnMap(node.id, storyline, new Vector3(vector.x, vector.y, 0));
                }
            }
            else
            {
                MapPanel.iconList = VanillaIconsBackup;
            }



            /*

            // Save vanilla lists of abno and ego pages to randomize them every run open
            // .ToList() is a hacky way to create a clone of the list
            if (AbnoPageInitialList == null)
                AbnoPageInitialList = EmotionCardXmlList.Instance._list.ToList();
            if (EGOPageInitialList == null)
                EGOPageInitialList = EmotionEgoXmlList.Instance._list.ToList();

            //Debug.Log($"INIT: {AbnoPageInitialList.Count}, {EGOPageInitialList.Count}");

            // Randomize Abno Pages
            List<EmotionCardXmlInfo> abnoPages = AbnoPageInitialList;

            // Make a list without Non-sephirah abno pages
            List<EmotionCardXmlInfo> sephPages = abnoPages.Where(x => x.Sephirah != SephirahType.None && x.Sephirah != SephirahType.ETC).ToList();
            //Debug.Log($"SEPH: {sephPages.Count}");

            // Shuffle Abno Pages
            List<EmotionCardXmlInfo> shuffledAbno = new List<EmotionCardXmlInfo>();
            var Random = new System.Random(PlaythruManager.Seed);
            for (int seph = 1; seph <= 10; seph++)
            {
                // Fill a pool of emotion levels abno pages should be, based on the selected page balance setting
                List<int> ELevelsPool = new List<int>();
                switch (PlaythruManager.AbnoPageBalance)
                {
                    case AbnoPagesBalance.Vanilla:
                        ELevelsPool = new List<int>() {1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 3};
                        break;
                    case AbnoPagesBalance.Balanced:
                        ELevelsPool = new List<int>() { 1, 1, 1, 2, 2, 2, 3, 3, 3 };
                        for (int p = 0; p < 6; p++)
                            ELevelsPool.Add(Random.Next(1, 4));
                        break;
                    case AbnoPagesBalance.Unbalanced:
                        for (int p = 0; p < 15; p++)
                            ELevelsPool.Add(Random.Next(1, 4));
                        break;
                }

                for (int i = 0; i < 15; i++)
                {
                    int lv = 2 + i / 3;

                    var rng = Random.Next(sephPages.Count);
                    var card = sephPages.ElementAt(rng);
                    sephPages.RemoveAt(rng);

                    rng = Random.Next(ELevelsPool.Count);
                    var ELV = ELevelsPool.ElementAt(rng);
                    ELevelsPool.RemoveAt(rng);

                    card.Sephirah = (SephirahType)seph;
                    card.Level = lv;
                    card.EmotionLevel = ELV;
                    card.State = Random.Next(2) == 1 ? MentalState.Positive : MentalState.Negative;
                    card.EmotionRate = Random.Next(4) * (card.State == MentalState.Positive ? 1 : -1);
                    shuffledAbno.Add(card);
                }
            }
            // Return Non-sephirah abno pages
            shuffledAbno.AddRange(abnoPages.Where(x => x.Sephirah == SephirahType.None).ToList());

            EmotionCardXmlList.Instance._list = shuffledAbno;


            // Randomize EGO Pages
            List<EmotionEgoXmlInfo> EGOPages = EGOPageInitialList.ToList();
            List<EmotionEgoXmlInfo> shuffledEGO = new List<EmotionEgoXmlInfo>();
            Random = new System.Random(PlaythruManager.Seed);

            for (int seph = 1; seph <= 10; seph++)
            {
                for (int i = 0; i < 5; i++)
                {
                    var rng = Random.Next(EGOPages.Count);
                    var card = EGOPages.ElementAt(rng);
                    EGOPages.RemoveAt(rng);

                    card.Sephirah = (SephirahType)seph;
                    shuffledEGO.Add(card);
                }
            }

            EmotionEgoXmlList.Instance._list = shuffledEGO;

            */
        }
    
        internal static void Init()
        {
            Debug.Log("[LORAP] Custom Content Init!");

            // Init some custom UI
            APConnectWindow.Init();
            MessagePopup.Init();
            AbnoEgoPagePopup.Init();

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


            // Add Keter Realization stages to FloorLevelXmlList TODO: You know.
            //FloorLevelXmlList._instance._list.Add(new FloorLevelXmlInfo() { level = 5, stageId = 210005, sephirahType = SephirahType.Keter });
            //FloorLevelXmlList._instance._list.Add(new FloorLevelXmlInfo() { level = 6, stageId = 210006, sephirahType = SephirahType.Keter });
            //FloorLevelXmlList._instance._list.Add(new FloorLevelXmlInfo() { level = 7, stageId = 210007, sephirahType = SephirahType.Keter });
            //FloorLevelXmlList._instance._list.Add(new FloorLevelXmlInfo() { level = 8, stageId = 210008, sephirahType = SephirahType.Keter });
            //FloorLevelXmlList._instance._list.Add(new FloorLevelXmlInfo() { level = 9, stageId = 210009, sephirahType = SephirahType.Keter });

            // Make map bigger
            UIStoryProgressPanel MapPanel = (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel;
            MapPanel.posRect.sizeDelta = new Vector2(3600, 7000);

            // Move Black Silence and Distorted Ensemble receptions on the map
            var BlackSilence = MapPanel.iconList.Find(i => i.currentStory == UIStoryLine.BlackSilence);
            BlackSilence.transform.localPosition = new Vector3(-200, 6915, 0);
            BlackSilence.connectLineList.First().transform.localPosition = new Vector3(-100, -40, 0);
            BlackSilence.connectLineList.First().transform.eulerAngles = new Vector3(0, 0, 310);

            var Distorted = MapPanel.iconList.Find(i => i.currentStory == UIStoryLine.TwistedBlue);
            Distorted.transform.localPosition = new Vector3(200, 6915, 0);
            Distorted.connectLineList.First().transform.localPosition = new Vector3(100, -40, 0);
            Distorted.connectLineList.First().transform.eulerAngles = new Vector3(0, 0, 230);

            // Hide "Reset Rewards" button from the book burning screen
            UIShowUsingBookInfoPanel dropBookPanel = (UI.UIController.Instance.Panels.ElementAt(3) as UIBookPanel).DropBookInfoPanel;
            dropBookPanel.button_rewardResetButton.gameObject.SetActive(false);

            // Save a "Template" for a map icon
            MapIconTemplate = MapPanel.iconList.First();
            LineTemplate = MapIconTemplate.connectLineList.First();


            // Adding receptions to the map
            // General Receptions
            PlaceReceptionOnMap(100001, UIStoryLine.PierresMeatPies, new Vector3(-260, 1880, 0)); // Backstreets Butchers
            PlaceReceptionOnMap(100002, UIStoryLine.HookOfficeRemnant, new Vector3(-520, 1880, 0)); // Hook Office Remnants
            PlaceReceptionOnMap(100003, UIStoryLine.Chapter2, new Vector3(260, 1880, 0));  // Urban Myth-class Syndicate

            PlaceReceptionOnMap(100004, UIStoryLine.Grade8Fixers, new Vector3(-450, 2900, 0)); // Grade 8 Fixers
            PlaceReceptionOnMap(100006, UIStoryLine.Grade7Fixers, new Vector3(450, 2900, 0));  // Grade 7 Fixers 
            PlaceReceptionOnMap(100005, UIStoryLine.Chapter3, new Vector3(0, 2900, 0));    // Urban Legend-class Office
            PlaceReceptionOnMap(100007, UIStoryLine.Chapter3, new Vector3(-900, 2900, 0)); // Urban Legend-class Syndicate
            PlaceReceptionOnMap(100008, UIStoryLine.AxeGang, new Vector3(900, 2900, 0));  // Axe Gang

            PlaceReceptionOnMap(100009, UIStoryLine.RustyChainGroup, new Vector3(-450, 3610, 0)); // Rusted Chains
            PlaceReceptionOnMap(100010, UIStoryLine.WorkshopFixer, new Vector3(0, 3610, 0));    // Workshop-affiliated Fixers
            PlaceReceptionOnMap(100014, UIStoryLine.Jeong, new Vector3(450, 3610, 0));  // Jeong's Office

            PlaceReceptionOnMap(100011, UIStoryLine.SevenAssociation, new Vector3(-450, 4520, 0)); // Seven Association
            PlaceReceptionOnMap(100012, UIStoryLine.Sword, new Vector3(450, 4520, 0));  // Blade Lineage

            PlaceReceptionOnMap(100013, UIStoryLine.ClassOneFixer, new Vector3(-450, 5550, 0)); // Dong-hwan the Grade 1 Fixer
            PlaceReceptionOnMap(100015, UIStoryLine.AwlOfNight, new Vector3(450, 5550, 0));  // Night Awls
            PlaceReceptionOnMap(100016, UIStoryLine.Usett, new Vector3(0, 5690, 0));    // The Udjat
            PlaceReceptionOnMap(100017, UIStoryLine.Mirae, new Vector3(0, 5420, 0));    // Mirae Life Insurance
            PlaceReceptionOnMap(100018, UIStoryLine.Workshop, new Vector3(-900, 5550, 0)); // Leaflet Workshop
            PlaceReceptionOnMap(100019, UIStoryLine.Bayyard, new Vector3(900, 5550, 0));  // Bayard


            // Backup vanilla map icons (we restore them if reception tree is not randomized)
            foreach (var icon in MapPanel.iconList)
            {
                icon.SetActiveStory(false);
            }

            VanillaIconsBackup = MapPanel.iconList;
            MapPanel.iconList = new List<UIStoryProgressIconSlot>();

            // Additions
            // Checkmarks for all found books receptions
            foreach (var icon in MapPanel.iconList)
            {
                var check = UnityEngine.Object.Instantiate(CheckmarkIconTemplate, icon.transform);
                check.transform.localPosition = new Vector3(30, 100, 0);
                check.name = "Checkmark";
                check.transform.SetSiblingIndex(2);
            }


            return;

            // Custom Books ( maybe move that to the respective drop system? )
            // Get all the possible drops
            List<BookDropItemInfo> allDrops = new List<BookDropItemInfo>();

            // Add pages that drop from completing certain receptions (yay wall of text)
            allDrops.Add(new BookDropItemInfo() { id = new LorId(408013), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(408012), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(704008), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(704003), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(704002), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(704018), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(704005), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(704016), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(704006), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(704015), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(704007), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(704004), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(704011), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(704012), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(704013), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(704014), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(704001), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(704009), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(704010), itemType = DropItemType.Card });
            // Post Game
            allDrops.Add(new BookDropItemInfo() { id = new LorId(705002), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(705003), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(705004), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(705010), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(705011), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(705013), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(705014), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(705015), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(705016), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(705017), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(705018), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(705019), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(705020), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(705021), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(705031), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(705032), itemType = DropItemType.Card });
            allDrops.Add(new BookDropItemInfo() { id = new LorId(705033), itemType = DropItemType.Card });

            // I decided to let this comically large one-liner be just because it's silly
            var EveryEnemyDropAndReward = MapPanel.iconList.Where(i => i.storyData != null).SelectMany(i => i._storyData).SelectMany(s => s.rewardList).Concat(Enum.GetValues(typeof(UIStoryLine)).Cast<UIStoryLine>().ToList().SelectMany(e => StageClassInfoList.Instance.recipeCondList.FindAll(i => i.storyType == e.ToString()).SelectMany(s => s.waveList).SelectMany(w => w.enemyUnitIdList).SelectMany(uid => EnemyUnitClassInfoList.Instance.GetData(uid).dropTableList).SelectMany(t => t.dropItemList).SelectMany(i => DropBookXmlList.Instance.GetData(i.bookId).DropItemList))).ToList();

            foreach (var drop in EveryEnemyDropAndReward)
            {
                if (allDrops.Exists(d => d.id == drop.id)) continue;
                //Debug.Log($"Drop: {drop.id} - {(drop.itemType == DropItemType.Card ? ItemXmlDataList.instance.GetCardItem(drop.id).Name : BookXmlList.Instance.GetData(drop.id).Name)}");
                allDrops.Add(drop);
            }
             
            CreateCustomBook(123456, "Book of Everything", 16, allDrops);
        }
    }
}
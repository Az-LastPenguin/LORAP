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
using UnityEngine.UI;

namespace LORAP.Gameplay
{
    internal static class ContentManager
    {
        internal static Dictionary<int, DropBookXmlInfo> CustomBooks = new Dictionary<int, DropBookXmlInfo>();

        private static List<EmotionCardXmlInfo> AbnoPageInitialList;

        private static List<EmotionEgoXmlInfo> EGOPageInitialList;

        private static List<UIStoryProgressIconSlot> VanillaIconsBackup = new List<UIStoryProgressIconSlot>();

        private static UIStoryProgressIconSlot MapIconTemplate;
        private static GameObject LineTemplate;
        private static GameObject CheckmarkIconTemplate = UICardListDetailFilterPopup.Instance.transform.Find("[Image]Frame/Scroll View/Viewport/Content/RarityGroup/Group/[Toggle]DetailSlot/[Toggle]SelectableToggle/[Image]IconGlow").gameObject;

        private static List<SephirahType> GameplaySephirahs = new List<SephirahType>()
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

        private static DropBookXmlInfo CreateCustomBook(int id, string name/*, int dropNum, List<BookDropItemInfo> dropList*/)
        {
            var Book = new DropBookXmlInfo();
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

        private static UIStoryProgressIconSlot PlaceBattleNodeOnMap(int id, UIStoryLine story, Vector3 position, SephirahType assignedFloor = SephirahType.None)
        {
            UIStoryProgressPanel MapPanel = (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel;

            var icon = UnityEngine.Object.Instantiate(MapIconTemplate, MapPanel.chapterList.First().transform);
            icon.transform.localPosition = position;
            icon.StoryProgressPanel = MapPanel;
            icon.connectLineList = new List<GameObject>();
            icon.storyData = new List<StageClassInfo>() { StageClassInfoList.Instance.GetData(id) };
            icon.currentStory = story;

            if (assignedFloor != SephirahType.None)
                ApplyFloorIcon(icon, assignedFloor);

            var check = UnityEngine.Object.Instantiate(CheckmarkIconTemplate, icon.transform);
            check.transform.localPosition = new Vector3(30, 100, 0);
            check.name = "Checkmark";
            check.transform.SetSiblingIndex(2);
            check.SetActive(false);

            MapPanel.iconList.Add(icon);

            return icon;
        }

        internal static void ApplyFloorIcon(UIStoryProgressIconSlot icon, SephirahType floor)
        {
            Sprite contentSprite;
            Sprite glowSprite;
            TryGetFloorSprites(floor, out contentSprite, out glowSprite);
            if (contentSprite == null)
                return;

            SetStoryIconSet(icon, "closeIconset", contentSprite, glowSprite, Color.white, Color.clear);
            SetStoryIconSet(icon, "openIconset", contentSprite, glowSprite, Color.white, Color.clear);
        }

        internal static void ConfigureBattleNodeLevelIcons(UIStoryProgressIconSlot icon, bool isRevealed)
        {
            if (icon == null || icon.IconLevels == null)
                return;

            for (int i = 0; i < icon.IconLevels.Length; i++)
            {
                storyIconLevel levelIcon = icon.IconLevels[i];
                if (levelIcon.root == null)
                    continue;

                levelIcon.root.SetActive(isRevealed && i == 0);
            }
        }

        private static void SetStoryIconSet(UIStoryProgressIconSlot icon, string fieldName, Sprite content, Sprite glow, Color contentColor, Color glowColor)
        {
            FieldInfo iconSetField = typeof(UIStoryProgressIconSlot).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            object iconSet = iconSetField?.GetValue(icon);
            if (iconSet == null)
                return;

            Image contentImage = GetImageField(iconSet, "img_iconContent") ?? GetImageField(iconSet, "img_icon");
            Image bgImage = GetImageField(iconSet, "img_iconbg");
            Image frameImage = GetImageField(iconSet, "img_iconFrame");

            if (contentImage != null)
            {
                if (content != null)
                {
                    contentImage.sprite = content;
                    contentImage.SetNativeSize();
                }
                contentImage.color = contentColor;
            }

            if (bgImage != null)
            {
                if (glow != null)
                {
                    bgImage.sprite = glow;
                    bgImage.SetNativeSize();
                }
                bgImage.color = glowColor;
            }

            if (frameImage != null)
            {
                if (glow != null)
                {
                    frameImage.sprite = glow;
                    frameImage.SetNativeSize();
                }
                frameImage.color = glowColor;
            }
        }

        private static Image GetImageField(object target, string fieldName)
        {
            return target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(target) as Image;
        }

        private static void TryGetFloorSprites(SephirahType floor, out Sprite content, out Sprite glow)
        {
            content = null;
            glow = null;

            try
            {
                string spriteName = GetFloorIconSpriteName(floor);
                if (string.IsNullOrEmpty(spriteName))
                    return;

                content = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(s => s != null && s.name == spriteName);
                glow = null;
            }
            catch
            {
                content = null;
                glow = null;
            }
        }

        private static string GetFloorIconSpriteName(SephirahType floor)
        {
            switch (floor)
            {
                case SephirahType.Keter: return "Icon_Sephirah_0";
                case SephirahType.Malkuth: return "Icon_Sephirah_1";
                case SephirahType.Yesod: return "Icon_Sephirah_2";
                case SephirahType.Hod: return "Icon_Sephirah_4";
                case SephirahType.Netzach: return "Icon_Sephirah_3";
                case SephirahType.Tiphereth: return "Icon_Sephirah_5";
                case SephirahType.Gebura: return "Icon_Sephirah_6";
                case SephirahType.Chesed: return "Icon_Sephirah_7";
                case SephirahType.Binah: return "Icon_Sephirah_9";
                case SephirahType.Hokma: return "Icon_Sephirah_8";
                default: return null;
            }
        }


        internal static void SetupRunContent()
        {
            Debug.Log("[LORAP] Initializing Run");

            RandomizeReceptionTree();

            ShuffleAbnoPages();

            RandomizeAbnoPages();

            ShuffleEGOPages();

            PrepareSuppressions();
        }

        private static void RandomizeReceptionTree()
        {
            UIStoryProgressPanel MapPanel = (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel;

            if (!SlotDataManager.HasBattleTree)
                throw new Exception("LORAP battle tree data was not parsed.");

            Debug.Log("[LORAP] Drawing Battle Tree");

            Dictionary<string, UIStoryProgressIconSlot> icons = new Dictionary<string, UIStoryProgressIconSlot>();
            List<BattleNode> nodes = SlotDataManager.BattleTree.Nodes.Values
                .OrderBy(n => n.VisualY)
                .ThenBy(n => n.VisualX)
                .ThenBy(n => n.Id)
                .ToList();

            foreach (BattleNode node in nodes)
            {
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
                }

                Vector3 position = new Vector3(node.VisualX, node.VisualY, 0f);
                icons[node.Key] = PlaceBattleNodeOnMap(node.Id, storyline, position, node.Kind == BattleNodeKind.Stage ? node.AssignedFloor : SephirahType.None);
            }

            foreach (BattleNode node in SlotDataManager.BattleTree.Nodes.Values)
            {
                if (!icons.ContainsKey(node.Key))
                    continue;

                UIStoryProgressIconSlot icon = icons[node.Key];
                icon.connectLineList.Clear();

                foreach (string nextKey in node.Next)
                {
                    if (!icons.ContainsKey(nextKey))
                        continue;

                    UIStoryProgressIconSlot nextIcon = icons[nextKey];
                    var line = UnityEngine.Object.Instantiate(LineTemplate, icon.transform.Find("[Rect]Lines"));
                    line.transform.localPosition = (nextIcon.transform.localPosition - icon.transform.localPosition) / 2;
                    line.transform.right = (nextIcon.transform.localPosition - icon.transform.localPosition).normalized;
                    line.transform.localScale = new Vector3((nextIcon.transform.localPosition - icon.transform.localPosition).magnitude / 220, 1, 1);
                    line.SetActive(true);

                    icon.connectLineList.Add(line);
                }
            }

            ResizeBattleTreeMap(MapPanel, nodes);
        }

        private static void ResizeBattleTreeMap(UIStoryProgressPanel mapPanel, List<BattleNode> nodes)
        {
            if (mapPanel == null || nodes == null || nodes.Count == 0)
                return;

            float minX = nodes.Min(n => n.VisualX);
            float maxX = nodes.Max(n => n.VisualX);
            float minY = nodes.Min(n => n.VisualY);
            float maxY = nodes.Max(n => n.VisualY);

            float width = Math.Max(5000f, Math.Abs(maxX - minX) + 1800f);
            float height = Math.Max(9000f, Math.Abs(maxY - minY) + 1800f);

            mapPanel.posRect.sizeDelta = new Vector2(width, height);
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
            var Random = new System.Random(SlotDataManager.Seed);

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
            List<EmotionCardXmlInfo> abnoPagePool = allPages.Where(x => GameplaySephirahs.Contains(x.Sephirah)).ToList();

            // Resulting list of abno pages
            List<EmotionCardXmlInfo> abnoPages = allPages.Where(p => !GameplaySephirahs.Contains(p.Sephirah)).ToList();

            // If we randomize in same floor, just shuffle them around, it's good enough, no need to ensure anything else
            if (SlotDataManager.AbnoPageShuffle == AbnoPageShuffle.InFloor)
            {
                foreach (var seph in GameplaySephirahs)
                {
                    List<EmotionCardXmlInfo> sephPages = abnoPagePool.Where(p => p.Sephirah == seph).ToList();

                    for (int i = 2; i < 7; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            EmotionCardXmlInfo page = sephPages.PopRandom(Random);

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
                    SephirahType targetSeph = GameplaySephirahs.Where(s => floorAbnoPages[s].Where(l => l.Count <= 2).Count() >= pages.Count).ToList().PopRandom(Random); // That's a copy so it doesn't matter if we pop

                    foreach (int pid in pages)
                    {
                        EmotionCardXmlInfo exPage = abnoPagePool.FirstOrDefault(p => p.Sephirah == seph && p.id == pid);

                        if (exPage == null)
                            continue;

                        var vacantLevels = floorAbnoPages[targetSeph].Where(l => l.Count <= 2).ToList();
                        var levelList = vacantLevels.PopRandom(Random);

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
                var levelList = vacantLevels.PopRandom(Random);

                EmotionCardXmlInfo exPage = abnoPagePool.PopRandom(Random);

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

            var Random = new System.Random(SlotDataManager.Seed);

            foreach (SephirahType seph in GameplaySephirahs)
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
                    page.State = statesPool.PopRandom(Random);
                    page.EmotionLevel = levelsPool.PopRandom(Random);
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

            var Random = new System.Random(SlotDataManager.Seed);

            List<EmotionEgoXmlInfo> EGOPages = EGOPageInitialList.ToList();
            List<EmotionEgoXmlInfo> shuffledEGO = new List<EmotionEgoXmlInfo>();

            foreach (SephirahType seph in GameplaySephirahs)
            {
                for (int i = 0; i < 5; i++)
                {
                    EmotionEgoXmlInfo card = EGOPages.PopRandom(Random);

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

        internal static void Init()
        {
            Debug.Log("[LORAP] Initializing Custom Content");

            // Save vanilla lists of Abno and EGO Pages to return or modify later on
            AbnoPageInitialList = EmotionCardXmlList.Instance._list.ToList();
            EGOPageInitialList = EmotionEgoXmlList.Instance._list.ToList();

            // Initialize Custom UI
            APConnectWindow.Init();
            MessagePopup.Init();
            AbnoEgoPagePopup.Init();


            ApplyUIChanges();

            ApplyMapChanges();

            // Add BOE and Booster Pack to book list
            CreateCustomBook(123456, "Book of Everything");
            CreateCustomBook(123457, "Booster Pack");


            // Add Keter Realization stages to FloorLevelXmlList TODO: You know.
            //FloorLevelXmlList._instance._list.Add(new FloorLevelXmlInfo() { level = 5, stageId = 210005, sephirahType = SephirahType.Keter });
            //FloorLevelXmlList._instance._list.Add(new FloorLevelXmlInfo() { level = 6, stageId = 210006, sephirahType = SephirahType.Keter });
            //FloorLevelXmlList._instance._list.Add(new FloorLevelXmlInfo() { level = 7, stageId = 210007, sephirahType = SephirahType.Keter });
            //FloorLevelXmlList._instance._list.Add(new FloorLevelXmlInfo() { level = 8, stageId = 210008, sephirahType = SephirahType.Keter });
            //FloorLevelXmlList._instance._list.Add(new FloorLevelXmlInfo() { level = 9, stageId = 210009, sephirahType = SephirahType.Keter });
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

            // TODO: Change Icon for the library level in the level progress bar to AP icon
        }

        private static void ApplyMapChanges()
        {
            Debug.Log("[LORAP] Applying Map Changes");

            // Make map bigger
            UIStoryProgressPanel MapPanel = (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel;
            MapPanel.posRect.sizeDelta = new Vector2(5000, 9000);

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
            PlaceBattleNodeOnMap(100001, UIStoryLine.PierresMeatPies, new Vector3(-260, 1880, 0)); // Backstreets Butchers
            PlaceBattleNodeOnMap(100002, UIStoryLine.HookOfficeRemnant, new Vector3(-520, 1880, 0)); // Hook Office Remnants
            PlaceBattleNodeOnMap(100003, UIStoryLine.Chapter2, new Vector3(260, 1880, 0));  // Urban Myth-class Syndicate

            PlaceBattleNodeOnMap(100004, UIStoryLine.Grade8Fixers, new Vector3(-450, 2900, 0)); // Grade 8 Fixers
            PlaceBattleNodeOnMap(100006, UIStoryLine.Grade7Fixers, new Vector3(450, 2900, 0));  // Grade 7 Fixers 
            PlaceBattleNodeOnMap(100005, UIStoryLine.Chapter3, new Vector3(0, 2900, 0));    // Urban Legend-class Office
            PlaceBattleNodeOnMap(100007, UIStoryLine.Chapter3, new Vector3(-900, 2900, 0)); // Urban Legend-class Syndicate
            PlaceBattleNodeOnMap(100008, UIStoryLine.AxeGang, new Vector3(900, 2900, 0));  // Axe Gang

            PlaceBattleNodeOnMap(100009, UIStoryLine.RustyChainGroup, new Vector3(-450, 3610, 0)); // Rusted Chains
            PlaceBattleNodeOnMap(100010, UIStoryLine.WorkshopFixer, new Vector3(0, 3610, 0));    // Workshop-affiliated Fixers
            PlaceBattleNodeOnMap(100014, UIStoryLine.Jeong, new Vector3(450, 3610, 0));  // Jeong's Office

            PlaceBattleNodeOnMap(100011, UIStoryLine.SevenAssociation, new Vector3(-450, 4520, 0)); // Seven Association
            PlaceBattleNodeOnMap(100012, UIStoryLine.Sword, new Vector3(450, 4520, 0));  // Blade Lineage

            PlaceBattleNodeOnMap(100013, UIStoryLine.ClassOneFixer, new Vector3(-450, 5550, 0)); // Dong-hwan the Grade 1 Fixer
            PlaceBattleNodeOnMap(100015, UIStoryLine.AwlOfNight, new Vector3(450, 5550, 0));  // Night Awls
            PlaceBattleNodeOnMap(100016, UIStoryLine.Usett, new Vector3(0, 5690, 0));    // The Udjat
            PlaceBattleNodeOnMap(100017, UIStoryLine.Mirae, new Vector3(0, 5420, 0));    // Mirae Life Insurance
            PlaceBattleNodeOnMap(100018, UIStoryLine.Workshop, new Vector3(-900, 5550, 0)); // Leaflet Workshop
            PlaceBattleNodeOnMap(100019, UIStoryLine.Bayyard, new Vector3(900, 5550, 0));  // Bayard


            foreach (var icon in MapPanel.iconList)
            {
                icon.SetActiveStory(false);
            }

            // Backup vanilla map icons (we restore them if reception tree is not randomized)
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
        }
    }
}

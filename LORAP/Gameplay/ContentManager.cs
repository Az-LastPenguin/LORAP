using HarmonyLib;
using LORAP.Playthru;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private static UIStoryProgressIconSlot AddReceptionToMap(int id, UIStoryLine story, Vector3 position)
        {
            UIStoryProgressPanel MapPanel = (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel;

            var original = MapPanel.iconList.First();
            var copy = UnityEngine.Object.Instantiate(original, MapPanel.chapterList.First().transform);
            copy.transform.localPosition = position;
            copy.StoryProgressPanel = MapPanel;
            copy.connectLineList = new List<GameObject>();
            copy.storyData = new List<StageClassInfo>() { StageClassInfoList.Instance.GetData(id) };
            copy.currentStory = story;

            MapPanel.iconList.Add(copy);

            return copy;
        }

        internal static void RandomizeContent()
        {
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
                    card.State = Random.Next(1, 3) == 1 ? MentalState.Positive : MentalState.Negative;
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
        }
    
        internal static void AddCustomContent()
        {
            Debug.Log("Adding Custom Cotent!");
            // Some minor changes
            // Move the Abno and EGO page receive window to other canvas, also center EGO page display
            UIGetAbnormalityPanel.instance.gameObject.transform.SetParent(GameObject.Find("[Canvas][Script]PopupCanvas").transform);
            GameObject.Find("[Canvas][Script]PopupCanvas").GetComponent<Canvas>().sortingOrder = 90;
            UIGetAbnormalityPanel.instance.EgoCardsRoot.transform.Find("[Prefab]DetailEgoCardSlot").gameObject.GetComponent<Canvas>().sortingOrder = 90;
            UIGetAbnormalityPanel.instance.EgoCardsRoot.transform.Find("[Layout]CardViewList").localPosition = new Vector3(-90, 17.7f, 0);
            UIGetAbnormalityPanel.instance.EgoCardsRoot.transform.Find("[Layout]CardViewList").gameObject.GetComponent<GridLayoutGroup>().childAlignment = TextAnchor.UpperCenter;

            // Make map bigger
            UIStoryProgressPanel MapPanel = (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel;
            Traverse.Create(MapPanel).Field<RectTransform>("posRect").Value.sizeDelta = new Vector2(3600, 10000);

            // Create lines on map
            var originalLine = Traverse.Create(Traverse.Create(MapPanel).Field<List<UIStoryProgressIconSlot>>("iconList").Value.First()).Field<List<GameObject>>("connectLineList").Value.First();
            List<Tuple<Vector3, Vector3, Vector3>> LinePositions = new List<Tuple<Vector3, Vector3, Vector3>>()
            {
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(0, 7410, 0), new Vector3(0, 0, 270), new Vector3(0.5f, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(0, 7720, 0), new Vector3(0, 0, 270), new Vector3(1.3f, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(-140, 7510, 0), new Vector3(0, 0, 300), new Vector3(1.7f, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(140, 7510, 0), new Vector3(0, 0, 240), new Vector3(1.7f, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(-130, 7630, 0), new Vector3(0, 0, 315), new Vector3(1, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(130, 7630, 0), new Vector3(0, 0, 225), new Vector3(1, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(-125, 7830, 0), new Vector3(0, 0, 220), new Vector3(1, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(125, 7830, 0), new Vector3(0, 0, 320), new Vector3(1, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(-250, 7920, 0), new Vector3(0, 0, 270), new Vector3(1.4f, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(250, 7920, 0), new Vector3(0, 0, 270), new Vector3(1.4f, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(-125, 8030, 0), new Vector3(0, 0, 320), new Vector3(1.1f, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(125, 8030, 0), new Vector3(0, 0, 220), new Vector3(1.1f, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(0, 8225, 0), new Vector3(0, 0, 270), new Vector3(2.2f, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(0, 8240, 0), new Vector3(0, 0, 207), new Vector3(2.2f, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(0, 8240, 0), new Vector3(0, 0, 333), new Vector3(2.2f, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(-250, 8250, 0), new Vector3(0, 0, 270), new Vector3(0.8f, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(250, 8250, 0), new Vector3(0, 0, 270), new Vector3(0.8f, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(-125, 8450, 0), new Vector3(0, 0, 210), new Vector3(0.9f, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(125, 8450, 0), new Vector3(0, 0, 330), new Vector3(0.9f, 1, 1)),
                new Tuple<Vector3, Vector3, Vector3>(new Vector3(0, 7160, 0), new Vector3(0, 0, 270), new Vector3(1, 1, 1)),
            };

            for (int i = 0; i < 20; i++)
            {
                var line = UnityEngine.Object.Instantiate(originalLine, originalLine.transform.parent);
                line.transform.localPosition = LinePositions[i].Item1;
                line.transform.eulerAngles = LinePositions[i].Item2;
                line.transform.localScale = LinePositions[i].Item3;
            }

            // Move Black Silence and Distorted Ensemble receptions on the map
            var BlackSilence = MapPanel.iconList.Find(i => i.currentStory == UIStoryLine.BlackSilence);
            BlackSilence.transform.localPosition = new Vector3(-120, 8390, 0);
            BlackSilence.connectLineList.First().transform.localPosition = new Vector3(-80, 1450, 0);
            BlackSilence.connectLineList.First().transform.eulerAngles = new Vector3(0, 0, 300);

            var Distorted = MapPanel.iconList.Find(i => i.currentStory == UIStoryLine.TwistedBlue);
            Distorted.transform.localPosition = new Vector3(120, 8390, 0);
            Distorted.connectLineList.First().transform.localPosition = new Vector3(80, 1450, 0);
            Distorted.connectLineList.First().transform.eulerAngles = new Vector3(0, 0, 240);

            // Hide "Reset Rewards" button from the book burning screen and copy text to make custom one
            UIShowUsingBookInfoPanel dropBookPanel = (UI.UIController.Instance.Panels.ElementAt(3) as UIBookPanel).DropBookInfoPanel;
            dropBookPanel.button_rewardResetButton.gameObject.SetActive(false);
            GameObject burnBookTextObj = GameObject.Instantiate(dropBookPanel.txt_bookName.gameObject, dropBookPanel.transform);
            burnBookTextObj.name = "BurnAndSee";
            burnBookTextObj.transform.localPosition = new Vector3(300, 65, 0);
            TextMeshProUGUI burnBookText = burnBookTextObj.GetComponent<TextMeshProUGUI>();
            burnBookText.text = "Burn it and see ;)";
            burnBookText.alignment = TextAlignmentOptions.Center;
            burnBookText.color = new Color(0.9373f, 0.7608f, 0.5059f);
            TextMeshProMaterialSetter burnBookTextMat = burnBookTextObj.GetComponent<TextMeshProMaterialSetter>();
            burnBookTextMat.underlayColor = new Color(0.9373f, 0.7608f, 0.5059f);
            burnBookTextMat.enabled = false;
            burnBookTextMat.enabled = true;
            burnBookTextObj.SetActive(false);


            // Adding receptions to the map
            // General Receptions
            AddReceptionToMap(100001, UIStoryLine.Chapter2, new Vector3(-260, 1880, 0)); // Backstreets Butchers
            AddReceptionToMap(100002, UIStoryLine.Chapter2, new Vector3(-520, 1880, 0)); // Hook Office Remnants
            AddReceptionToMap(100003, UIStoryLine.Chapter2, new Vector3(260, 1880, 0));  // Urban Myth-class Syndicate

            AddReceptionToMap(100004, UIStoryLine.Chapter3, new Vector3(-450, 2900, 0)); // Grade 8 Fixers
            AddReceptionToMap(100006, UIStoryLine.Chapter3, new Vector3(450, 2900, 0));  // Grade 7 Fixers 
            AddReceptionToMap(100005, UIStoryLine.Chapter3, new Vector3(0, 2900, 0));    // Urban Legend-class Office
            AddReceptionToMap(100007, UIStoryLine.Chapter3, new Vector3(-900, 2900, 0)); // Urban Legend-class Syndicate
            AddReceptionToMap(100008, UIStoryLine.Chapter3, new Vector3(900, 2900, 0));  // Axe Gang

            AddReceptionToMap(100009, UIStoryLine.Chapter4, new Vector3(-450, 3610, 0)); // Rusted Chains
            AddReceptionToMap(100010, UIStoryLine.Chapter4, new Vector3(0, 3610, 0));    // Workshop-affiliated Fixers
            AddReceptionToMap(100014, UIStoryLine.Chapter4, new Vector3(450, 3610, 0));  // Jeong's Office

            AddReceptionToMap(100011, UIStoryLine.Chapter5, new Vector3(-450, 4520, 0)); // Seven Association
            AddReceptionToMap(100012, UIStoryLine.Chapter5, new Vector3(450, 4520, 0));  // Blade Lineage

            AddReceptionToMap(100013, UIStoryLine.Chapter6, new Vector3(-450, 5550, 0)); // Dong-hwan the Grade 1 Fixer
            AddReceptionToMap(100015, UIStoryLine.Chapter6, new Vector3(450, 5550, 0));  // Night Awls
            AddReceptionToMap(100016, UIStoryLine.Chapter6, new Vector3(0, 5690, 0));    // The Udjat
            AddReceptionToMap(100017, UIStoryLine.Chapter6, new Vector3(0, 5420, 0));    // Mirae Life Insurance
            AddReceptionToMap(100018, UIStoryLine.Chapter6, new Vector3(-900, 5550, 0)); // Leaflet Workshop
            AddReceptionToMap(100019, UIStoryLine.Chapter6, new Vector3(900, 5550, 0));  // Bayard

            // Reverb Ensemble
            foreach (var icon in UISpriteDataManager.instance.floorIconSet) // Some changes to the icons for ensemble receptions
            {
                icon.iconGlow = icon.icon;
            }

            AddReceptionToMap(70001, (UIStoryLine)151, new Vector3(0, 6920, 0)).isChapterIcon = true;
            AddReceptionToMap(70002, (UIStoryLine)152, new Vector3(0, 7150, 0)).isChapterIcon = true;
            AddReceptionToMap(70003, (UIStoryLine)153, new Vector3(-250, 7350, 0)).isChapterIcon = true;
            AddReceptionToMap(70004, (UIStoryLine)154, new Vector3(250, 7350, 0)).isChapterIcon = true;
            AddReceptionToMap(70005, (UIStoryLine)155, new Vector3(0, 7550, 0)).isChapterIcon = true;
            AddReceptionToMap(70006, (UIStoryLine)156, new Vector3(-250, 7750, 0)).isChapterIcon = true;
            AddReceptionToMap(70007, (UIStoryLine)157, new Vector3(250, 7750, 0)).isChapterIcon = true;
            AddReceptionToMap(70008, (UIStoryLine)158, new Vector3(-250, 8000, 0)).isChapterIcon = true;
            AddReceptionToMap(70009, (UIStoryLine)159, new Vector3(250, 8000, 0)).isChapterIcon = true;
            AddReceptionToMap(70010, (UIStoryLine)160, new Vector3(0, 8150, 0)).isChapterIcon = true;


            // Additions
            // Checkmarks for all found books receptions
            var CheckmarkIcon = UICardListDetailFilterPopup.Instance.transform.Find("[Image]Frame/Scroll View/Viewport/Content/RarityGroup/Group/[Toggle]DetailSlot/[Toggle]SelectableToggle/[Image]IconGlow").gameObject;

            foreach (var icon in MapPanel.iconList)
            {
                var copy = UnityEngine.Object.Instantiate(CheckmarkIcon, icon.transform);
                copy.transform.localPosition = new Vector3(30, 100, 0);
                copy.name = "Checkmark";
                copy.transform.SetSiblingIndex(2);
            }


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



// How to create Custom Reception
/*  
    StageClassInfo end = new StageClassInfo();
    end.chapter = 7;
    end.floorNum = 3;
    end.stageName = "Test";
    end._id = 123456;
    end.invitationInfo.combine = StageCombineType.BookValue;
    StageWaveInfo inf = new StageWaveInfo();
    inf.availableNumber = 5;
    inf.formationId = 2;
    inf.formationType = EnemyFormationType.Default;
    inf._enemyUnitIdList = new List<LorIdXml>() {new LorIdXml(null, 1), new LorIdXml(null, 2), new LorIdXml(null, 4)};
    inf.enemyUnitIdList = new List<LorId>() { new LorId(1), new LorId(2), new LorId(4) };
    end.waveList = new List<StageWaveInfo>() { inf };
    Traverse.Create(StageClassInfoList.Instance).Field<List<StageClassInfo>>("_list").Value.Add(end);

    AddReceptionToMap(123456, UIStoryLine.Chapter7, new Vector3(0, 7000, 0));
*/
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI;
using UnityEngine;

namespace LORAP
{
    internal static class CustomContentManager
    {
        private static List<int> BookIds = new List<int>() { 200001, 200002, 200003, 200004, 200005, 200006, 200007, 200008, 200009, 200010, 200011, 200012, 200013, 200014, 200015, 200016,
        210001, 210002, 210003, 210004, 210008, 210009,
        220001, 220002, 220003, 220004, 220005, 220006, 220007, 220008, 220009, 220010, 220011, 220012, 220013, 220014, 220015, 220016, 220017, 220018, 220019, 220020, 220021,
        230001, 230002, 230003, 230004, 230005, 230006, 230007, 230008, 230009, 230010, 230011, 230012, 230013, 230014, 230015, 230016, 230017, 230018, 230019, 230020, 230021, 230022, 230023,
        230024, 230025, 230026, 230027, 230028, 230029, 230030,
        240001, 240002, 240003, 240004, 240005, 240006, 240008, 240009, 240010, 240011, 240012, 240013, 240014, 240015, 240016, 240017, 240018, 240019, 240020, 240021, 240022, 240023, 243001,
        243002, 243003, 243004,
        250001, 250002, 250003, 250004, 250005, 250006, 250007, 250008, 250009, 250010, 250011, 250012, 250013, 250014, 250015, 250016, 250017, 250018, 250019, 250020, 250021, 250022, 250023,
        250024, 250025, 250026, 250027, 250028, 250029, 250030, 250031, 250032, 250033, 250034, 250035, 250036, 250037, 243005, 252001, 252002, 253001, 254001, 254002, 255001, 255002, 256002,
        260001, 260002, 260003, 260004};

        internal static List<DropBookXmlInfo> CustomBooks = new List<DropBookXmlInfo>();

        internal static Dictionary<Rarity, DropBookXmlInfo> BoosterPacks = new Dictionary<Rarity, DropBookXmlInfo>();

        private static DropBookXmlInfo CreateCustomBook(int id, DropItemState state, int dropNum, List<BookDropItemInfo> dropList)
        {
            var Book = new DropBookXmlInfo();
            Book._id = id;
            Book.workshopName = "";
            Book.workshopID = "Archipelago";
            Book.itemDropState = state;
            Book.DropNum = dropNum;
            Book.DropItemList = dropList;
            Traverse.Create(Singleton<DropBookXmlList>.Instance).Field<List<DropBookXmlInfo>>("_list").Value.Add(Book);
            Traverse.Create(Singleton<DropBookXmlList>.Instance).Field<Dictionary<LorId, DropBookXmlInfo>>("_dict").Value.Add(Book.id, Book);

            CustomBooks.Add(Book);

            return Book;
        }

        internal static void AddCustomContent()
        {
            // Custom Books
            List<BookDropItemInfo> CommonPages = new List<BookDropItemInfo>();
            List<BookDropItemInfo> UncommonPages = new List<BookDropItemInfo>();
            List<BookDropItemInfo> RarePages = new List<BookDropItemInfo>();
            List<BookDropItemInfo> UniquePages = new List<BookDropItemInfo>();

            List<int> foundIds = new List<int>();

            foreach (int bid in BookIds)
            {
                var book = Singleton<DropBookXmlList>.Instance.GetData(bid);

                foreach (var p in book.DropItemList)
                {
                    if (foundIds.Contains(p.id.id) || (p.itemType == DropItemType.Card && ItemXmlDataList.instance.GetCardItem(p.id).isError))
                        continue;

                    if (p.itemType == DropItemType.Equip)
                    {
                        var info = Singleton<BookXmlList>.Instance.GetData(p.id);

                        switch (info.Rarity)
                        {
                            case Rarity.Common:
                                CommonPages.Add(p);
                                break;
                            case Rarity.Uncommon:
                                UncommonPages.Add(p);
                                break;
                            case Rarity.Rare:
                                RarePages.Add(p);
                                break;
                            case Rarity.Unique:
                                UniquePages.Add(p);
                                break;
                        }
                    }
                    else if (p.itemType == DropItemType.Card)
                    {
                        var info = ItemXmlDataList.instance.GetCardItem(p.id);

                        switch (info.Rarity)
                        {
                            case Rarity.Common:
                                CommonPages.Add(p);
                                break;
                            case Rarity.Uncommon:
                                UncommonPages.Add(p);
                                break;
                            case Rarity.Rare:
                                RarePages.Add(p);
                                break;
                            case Rarity.Unique:
                                UniquePages.Add(p);
                                break;
                        }
                    }

                    foundIds.Add(p.id.id);
                }
            }

            BoosterPacks.Add(Rarity.Common, CreateCustomBook(123462, DropItemState.All, 16, CommonPages));
            BoosterPacks.Add(Rarity.Uncommon, CreateCustomBook(123463, DropItemState.All, 12, UncommonPages));
            BoosterPacks.Add(Rarity.Rare, CreateCustomBook(123464, DropItemState.All, 8, RarePages));
            BoosterPacks.Add(Rarity.Unique, CreateCustomBook(123465, DropItemState.All, 6, UniquePages));

            // Checkmarks for all found books receptions
            UIStoryProgressPanel MapPanel = (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel;
            var IconList = Traverse.Create(MapPanel).Field<List<UIStoryProgressIconSlot>>("iconList").Value;

            var CheckmarkIcon = UICardListDetailFilterPopup.Instance.transform.Find("[Image]Frame/Scroll View/Viewport/Content/RarityGroup/Group/[Toggle]DetailSlot/[Toggle]SelectableToggle/[Image]IconGlow").gameObject;

            foreach (var icon in IconList)
            {
                var copy = GameObject.Instantiate(CheckmarkIcon, icon.transform);
                copy.transform.localPosition = new Vector3(30, 100, 0);
                copy.name = "Checkmark";
            }

            // Custom Reception
            StageClassInfo end = new StageClassInfo();
            end.chapter = 7;
            end.floorNum = 3;
            end.stageName = "Test";
            end._id = 123456;
            end.invitationInfo.combine = StageCombineType.BookValue;
            StageWaveInfo i = new StageWaveInfo();
            i.availableNumber = 5;
            i.formationId = 2;
            i.formationType = EnemyFormationType.Default;
            i._enemyUnitIdList = new List<LorIdXml>() {new LorIdXml(null, 1), new LorIdXml(null, 2), new LorIdXml(null, 4)};
            i.enemyUnitIdList = new List<LorId>() { new LorId(1), new LorId(2), new LorId(4) };
            end.waveList = new List<StageWaveInfo>() { i };
            Traverse.Create(StageClassInfoList.Instance).Field<List<StageClassInfo>>("_list").Value.Add(end);

            // Add General receptions to the map
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

            AddReceptionToMap(100013, UIStoryLine.Chapter3, new Vector3(-450, 5550, 0)); // Dong-hwan the Grade 1 Fixer
            AddReceptionToMap(100015, UIStoryLine.Chapter3, new Vector3(450, 5550, 0));  // Night Awls
            AddReceptionToMap(100016, UIStoryLine.Chapter3, new Vector3(0, 5690, 0));    // The Udjat
            AddReceptionToMap(100017, UIStoryLine.Chapter3, new Vector3(0, 5420, 0));    // Mirae Life Insurance
            AddReceptionToMap(100018, UIStoryLine.Chapter3, new Vector3(-900, 5550, 0)); // Leaflet Workshop
            AddReceptionToMap(100019, UIStoryLine.Chapter3, new Vector3(900, 5550, 0));  // Bayard

            AddReceptionToMap(123456, UIStoryLine.Chapter3, new Vector3(0, 7000, 0));    // End
            APPlaythruManager.OpenReception(123456);
        }

        private static void AddReceptionToMap(int id, UIStoryLine story, Vector3 position)
        {
            UIStoryProgressPanel MapPanel = (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel;

            var original = Traverse.Create(MapPanel).Field<List<UIStoryProgressIconSlot>>("iconList").Value.First();
            var copy = GameObject.Instantiate(original, Traverse.Create(MapPanel).Field<List<GameObject>>("chapterList").Value.First().transform);
            copy.transform.localPosition = position;
            Traverse.Create(copy).Field("StoryProgressPanel").SetValue(MapPanel);
            Traverse.Create(copy).Field("connectLineList").SetValue(new List<GameObject>());
            Traverse.Create(copy).Field("storyData").SetValue(new List<StageClassInfo>() { StageClassInfoList.Instance.GetData(id) });
            Traverse.Create(copy).Field("currentStory").SetValue(story);
            Traverse.Create(MapPanel).Field<List<UIStoryProgressIconSlot>>("iconList").Value.Add(copy);
        }
    }
}

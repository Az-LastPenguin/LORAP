using HarmonyLib;
using LORAP.Gameplay;
using System.Collections.Generic; 
using UI;
using UnityEngine;

namespace LORAP.Patches
{
    internal class GachaPatches
    {
        // Make amount of books for burning show as infinite
        [HarmonyPatch(typeof(UIInvenFeedBookSlot), nameof(UIInvenFeedBookSlot.SetRemainNumber))]
        [HarmonyPostfix]
        static void FakeInfBooksForBurning(UIInvenFeedBookSlot __instance, int minusnum)
        {
            if (__instance.BookId.packageId != "lorap") // For every vanilla book
                __instance.txt_bookNum.text = "∞";
        }

        // Fake increase amount of vanilla books in the inventory so you can burn up to 20 at once
        [HarmonyPatch(typeof(UIInvenFeedBookSlot), nameof(UIInvenFeedBookSlot.SetData_DropBook))]
        [HarmonyPostfix]
        static void RedirectBookBurn(UIInvenFeedBookSlot __instance, LorId bookId)
        {
            if (bookId.packageId != "lorap") // For every vanilla book
                __instance.remainBookNum = 20;
        }

        // Redirect generation of drops after burning books
        [HarmonyPatch(typeof(UIBookPanel), nameof(UIBookPanel.FeedBookTargetSephirah))]
        [HarmonyPrefix]
        static bool RedirectBookBurn(UIBookPanel __instance, SephirahType sep)
        {
            LibraryFloorModel floor = LibraryModel.Instance.GetFloor(sep);

            List<BookDropResult> list = new List<BookDropResult>();
            foreach (LorId currentAddedBookId in __instance._currentAddedBookIdList)
            {
                list.AddRange(BookDropManager.GenerateDrops(currentAddedBookId));
            }

            UIGachaEffect.instance.StartGachaProcess(sep, floor.Level);
            UIGachaResultPopup.Instance.SetData(list, sep);

            LibraryModel.Instance.CheckAllCards();
            LibraryModel.Instance.CheckAllEquips();

            __instance.SetAddedBookClear();
            __instance.OnUpdatePhase();

            SaveManager.SaveGame();

            return false;
        }

        // Change the way contents of the books are shown in order to show random pages from BOE and show custom pools of pages
        [HarmonyPatch(typeof(UIShowUsingBookInfoPanel), nameof(UIShowUsingBookInfoPanel.ShowBookInfoData))]
        [HarmonyPrefix]
        static bool ChangeListOfContents(UIShowUsingBookInfoPanel __instance, DropBookXmlInfo dropBookInfo)
        {
            if (dropBookInfo == null) 
                return false;

            __instance.gameObject.SetActive(true);
            __instance.SetActivePanel(true);
            __instance.currentDropBookSlot.SetData_DropBook(dropBookInfo.id);
            __instance.txt_bookName.text = dropBookInfo.Name;

            if (dropBookInfo.id.id == 123456) // BoE
            {


                return false;
            }

            if (dropBookInfo.id.id == 123457) // Booster Packs
            {


                return false;
            }

            if (BookDropManager.BookDrops.ContainsKey(dropBookInfo.id))
            {
                var drops = BookDropManager.BookDrops[dropBookInfo.id];

                List<UIRewardBookData> list = new List<UIRewardBookData>();
                List<UIRewardCardData> list2 = new List<UIRewardCardData>();

                foreach (var d in drops)
                {
                    if (d.type == DropItemType.Card)
                        list2.Add(new UIRewardCardData(new DiceCardItemModel(ItemXmlDataList.instance.GetCardItem(d.id)), 1, 0));
                    else if (d.type == DropItemType.Equip)
                        list.Add(new UIRewardBookData(BookXmlList.Instance.GetData(d.id), 1, 0));
                }

                __instance.rewardItemList.SetItemsData(list, list2);
            }

            __instance.SetColor(UIColorManager.Manager.GetUIColor(UIColor.Default));
            __instance.img_BookIcon.color = Color.white;
            __instance.img_BookIcon.sprite = dropBookInfo.bookIcon;
            __instance.img_BookIconGlow.sprite = dropBookInfo.bookIconGlow;

            return false;
        }
    }
}

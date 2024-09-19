using GameSave;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace LORAP.Patches
{
    [HarmonyPatch(typeof(UIBookPanel))]
    internal class DropsGeneration
    {
        // Generate Gacha Drops and Send AP Checks on Book Burn
        [HarmonyPatch("FeedBookTargetSephirah")]
        [HarmonyPrefix]
        static bool GenerateDropsSendChecks(UIBookPanel __instance, SephirahType sep)
        {
            LibraryFloorModel floor = LibraryModel.Instance.GetFloor(sep);
            List<BookDropResult> list = new List<BookDropResult>();

            foreach (LorId currentAddedBookId in Traverse.Create(__instance).Field<List<LorId>>("_currentAddedBookIdList").Value)
            {
                var book = Singleton<DropBookXmlList>.Instance.GetData(currentAddedBookId);

                switch (currentAddedBookId.id)
                {
                    case 123462:
                        list.AddRange(APPlaythruManager.GenerateBoosterDrops(Rarity.Common));
                        break;
                    case 123463:
                        list.AddRange(APPlaythruManager.GenerateBoosterDrops(Rarity.Uncommon));
                        break;
                    case 123464:
                        list.AddRange(APPlaythruManager.GenerateBoosterDrops(Rarity.Rare));
                        break;
                    case 123465:
                        list.AddRange(APPlaythruManager.GenerateBoosterDrops(Rarity.Unique));
                        break;
                    default:
                        var result = new BookDropResult();
                        result.bookInstanceId = 161616;
                        result.number = book.id.id;
                        list.Add(result);
                        ItemLocationManager.SendBookCheck(book.id.id);
                        break;
                }

                Singleton<DropBookInventoryModel>.Instance.RemoveBook(currentAddedBookId);
            }

            UIGachaEffect.instance.StartGachaProcess(sep, floor.Level);

            UIGachaResultPopup.Instance?.SetData(list, sep);

            LibraryModel.Instance.CheckAllCards();
            LibraryModel.Instance.CheckAllEquips();
            __instance.SetAddedBookClear();
            __instance.OnUpdatePhase();
            Singleton<SaveManager>.Instance.SavePlayData(1);

            return false;
        }
    }

    [HarmonyPatch(typeof(UIGachaResultPopup))]
    internal class GachaResultPatches
    {
        // Display Gacha Drops. Next 3 patches are related
        // Remove parts of the code that update cards, and only save parts of the code that make cards visible
        // Don't even fucking ask me why. It just doesn't work otherwise.
        [HarmonyPatch(nameof(UIGachaResultPopup.UpdateGachaList))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpdateGachaListTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = instructions.ToList();

            int index1 = newInstructions.FindIndex(instruction => instruction.opcode == OpCodes.Stloc_2) - 4;
            int index2 = newInstructions.IndexOf(newInstructions.FindAll(instruction => instruction.opcode == OpCodes.Ldc_I4_1)[2]) - 4; //FindIndex(instruction => instruction.opcode == OpCodes.Ldc_I4_1) - 4;

            for (int i = index1; i < index2; i++)
            {
                newInstructions[i] = new CodeInstruction(OpCodes.Nop);
            }

            for (int z = 0; z < newInstructions.Count; z++)
            {
                yield return newInstructions[z];
            }

        }

        // This postfix updates the cards
        [HarmonyPatch(nameof(UIGachaResultPopup.UpdateGachaList))]
        [HarmonyPostfix]
        static void UpdateGachaListPostfix(UIGachaResultPopup __instance)
        {
            var _currentViewDropList = Traverse.Create(__instance).Field<List<BookDropResult>>("_currentViewDropList").Value;
            var _gachaSlotList = Traverse.Create(__instance).Field<List<UIGachaSlot>>("_gachaSlotList").Value;
            for (int j = 0; j < _gachaSlotList.Count; j++)
            {
                if (j < _currentViewDropList.Count)
                {
                    BookDropResult bookDropResult = _currentViewDropList[j];

                    if (bookDropResult.bookInstanceId == 161616)
                    {
                        var slot = _gachaSlotList[j];
                        slot.isCard = false;
                        slot.equipSlot.SetActiveSlot(true);
                        slot.cardSlot.SetActiveSlot(false);
                        slot.equipSlot.BookName.text = "Archipelago Check";
                        slot.equipSlot.index = j;
                        slot.equipSlot._book = null;
                        slot.equipSlot.Icon.sprite = UISpriteDataManager.instance?.GetStoryIcon("Chapter1").icon;
                        slot.equipSlot.IconGlow.sprite = UISpriteDataManager.instance?.GetStoryIcon("Chapter1").iconGlow;
                        Color uIColor = UIColorManager.Manager.GetUIColor(UIColor.Default);
                        Color equipRarityColor = UIColorManager.Manager.GetEquipRarityColor(Rarity.Unique);
                        typeof(UIGachaEquipSlot).GetMethod("SetColor", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(slot.equipSlot, new object[] { uIColor });
                        typeof(UIGachaEquipSlot).GetMethod("SetGlowColor", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(slot.equipSlot, new object[] { equipRarityColor });

                        slot.selectable.interactable = false;

                        continue;
                    }

                    if (bookDropResult.itemType == DropItemType.Card)
                    {
                        _gachaSlotList[j].SetCard(new DiceCardItemModel(ItemXmlDataList.instance.GetCardItem(bookDropResult.id)), j);
                    }
                    else
                    {
                        _gachaSlotList[j].SetEquip(Singleton<BookInventoryModel>.Instance.GetBookByInstanceId(bookDropResult.bookInstanceId), j);
                    }
                }
            }
        }

    }

    [HarmonyPatch(typeof(UIGachaSlot))]
    internal class FakeKeyPages
    {
        // Make fake AP Check ""Key Pages"" not Hoverable, show no info, as it's not real Key Pages
        [HarmonyPatch(nameof(UIGachaSlot.OnPointerEnter))]
        [HarmonyPrefix]
        static bool FakeKeyPageHover(UIGachaSlot __instance)
        {
            if (!__instance.isCard && __instance.equipSlot._book == null)
            {
                return false;
            }

            return true;
        }

        [HarmonyPatch(nameof(UIGachaSlot.OnPointerExit))]
        [HarmonyPrefix]
        static bool FakeKeyPageHoverExit(UIGachaSlot __instance)
        {
            if (!__instance.isCard && __instance.equipSlot._book == null)
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(DropBookXmlInfo))]
    internal class CustomBookNamesPatch
    {
        // Custom Books Names. For some reason they only show up if done that way
        [HarmonyPatch("Name", MethodType.Getter)]
        [HarmonyPrefix]
        static bool CustomBookName(DropBookXmlInfo __instance, ref string __result)
        {
            switch (__instance._id)
            {
                case 123462:
                    __result = "Paperback Booster Pack";
                    return false;
                case 123463:
                    __result = "Hardcover Booster Pack";
                    return false;
                case 123464:
                    __result = "Limited Booster Pack";
                    return false;
                case 123465:
                    __result = "Objet d'art Booster Pack";
                    return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(UIShowUsingBookInfoPanel))]
    internal class FakeBookDropsPatch
    {
        // Replace Books' drops with a fake drop with info on the drops
        [HarmonyPatch(nameof(UIShowUsingBookInfoPanel.ShowBookInfoData))]
        [HarmonyPrefix]
        static bool FakeBooksDrops(UIShowUsingBookInfoPanel __instance, DropBookXmlInfo dropBookInfo)
        {
            __instance.gameObject.SetActive(value: true);
            Traverse.Create(__instance).Field("xmlinfo").SetValue(dropBookInfo);
            __instance.SetActivePanel(show: true);
            var xmlinfo = Traverse.Create(__instance).Field<DropBookXmlInfo>("xmlinfo").Value;
            if (xmlinfo == null)
            {
                return false;
            }
            Traverse.Create(__instance).Field<UIBookSlot>("currentDropBookSlot").Value.SetData_DropBook(xmlinfo.id);
            Traverse.Create(__instance).Field<TextMeshProUGUI>("txt_bookName").Value.text = xmlinfo.Name;
            List<UIRewardBookData> list = new List<UIRewardBookData>();
            List<UIRewardCardData> list2 = new List<UIRewardCardData>();

            BookXmlInfo fakeInfo = new BookXmlInfo();
            fakeInfo._id = 727272;
            fakeInfo.workshopID = "Archipelago";
            switch (dropBookInfo.id.id)
            {
                case 123462:
                    fakeInfo.InnerName = "Random Paperback Key and Combat Pages x16";
                    fakeInfo.Rarity = Rarity.Common;
                    break;
                case 123463:
                    fakeInfo.InnerName = "Random Hardcover Key and Combat Pages x12";
                    fakeInfo.Rarity = Rarity.Uncommon;
                    break;
                case 123464:
                    fakeInfo.InnerName = "Random Limited Key and Combat Pages x8";
                    fakeInfo.Rarity = Rarity.Rare;
                    break;
                case 123465:
                    fakeInfo.InnerName = "Random Objet d'art Key and Combat Pages x4";
                    fakeInfo.Rarity = Rarity.Unique;
                    break;
                default:
                    fakeInfo.InnerName = "Archipelago Check";
                    fakeInfo.Rarity = Rarity.Unique;
                    break;
            }

            list.Add(new UIRewardBookData(fakeInfo, 0, 0));
            Traverse.Create(__instance).Field<UIRewardItemList>("rewardItemList").Value.SetItemsData(list, list2);
            __instance.SetColor(UIColorManager.Manager.GetUIColor(UIColor.Default));
            Traverse.Create(__instance).Field<Image>("img_BookIcon").Value.color = Color.white;
            Traverse.Create(__instance).Field<Image>("img_BookIcon").Value.sprite = xmlinfo.bookIcon;
            Traverse.Create(__instance).Field<Image>("img_BookIconGlow").Value.sprite = xmlinfo.bookIconGlow;
            Traverse.Create(__instance).Field<UICustomGraphicObject>("button_rewardResetButton").Value.interactable = false;

            return false;
        }
    }
}

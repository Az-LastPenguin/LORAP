using GameSave;
using HarmonyLib;
using LOR_DiceSystem;
using LORAP.CustomUI;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using static PassiveModel;
using static StageClearInfoListModel;
using static StageController;

namespace LORAP.Patches
{
    [HarmonyPatch(typeof(UIFloorQuestPanel))]
    internal class QuestToHintsPatch
    {
        // Change Quest info to hints
        [HarmonyPatch("SetData")]
        [HarmonyPrefix]
        static bool QuestToHints(UIFloorQuestPanel __instance, LibraryFloorModel floor)
        {
            var questSlotList = Traverse.Create(__instance).Field<UIFloorQuestSlot[]>("questSlotList").Value;

            var setText = (UIFloorQuestSlot slot, string text, bool complete) =>
            {
                slot.SetActiveSlot(on: true);
                Color color = (complete ? UIColorManager.Manager.GetUIColor(UIColor.Disabled) : UIColorManager.Manager.GetUIColor(UIColor.Default));
                slot.SetColor(color);

                var txt_QuestName = Traverse.Create(slot).Field<TextMeshProUGUI>("txt_QuestName").Value;
                txt_QuestName.text = text;
                txt_QuestName.ForceMeshUpdate();

                var img_BgFrame = Traverse.Create(slot).Field<Image>("img_BgFrame").Value;
                img_BgFrame.rectTransform.sizeDelta = new Vector2(txt_QuestName.preferredWidth + 10f, img_BgFrame.rectTransform.sizeDelta.y);

                txt_QuestName.enabled = false;
                txt_QuestName.enabled = true;

                float x = ((txt_QuestName.preferredWidth + 25f > 370f) ? 370f : (txt_QuestName.preferredWidth + 25f));
                img_BgFrame.rectTransform.sizeDelta = new Vector2(x, img_BgFrame.rectTransform.sizeDelta.y);

                Traverse.Create(slot).Field<TextMeshProUGUI>("txt_QuestProgress").Value.text = "";

                var img_Icon = Traverse.Create(slot).Field<Image>("img_Icon").Value;
                img_Icon.sprite = (complete ? UISpriteDataManager.instance._floorQuestStateIcon[1] : UISpriteDataManager.instance._floorQuestStateIcon[0]);
                img_Icon.enabled = true;
                img_Icon.color = (complete ? UIColorManager.Manager._floorQuestSlotIconColor[1] : UIColorManager.Manager._floorQuestSlotIconColor[0]);
            };

            setText(questSlotList[0], $"Abno Page: Unknown", false);
            setText(questSlotList[1], $"EGO: Unknown", false);
            setText(questSlotList[2], $"Librarian: Unknown", false);
            questSlotList[3].SetActiveSlot(on: false);
            if (floor.Sephirah == SephirahType.Keter)
                setText(questSlotList[3], $"Black Silence: Unknown", false);
            if (floor.Sephirah == SephirahType.Binah)
                setText(questSlotList[3], $"Binah: Unknown", false);
            questSlotList[4].SetActiveSlot(on: false);

            return false;
        }
    }

    [HarmonyPatch(typeof(ItemXmlDataList))]
    internal class CombatPageExclusivenessPatch
    {
        // Remove combat page exclusiveness
        [HarmonyPatch("InitCardInfo")]
        [HarmonyPrefix]
        static void RemoveCombatPageExclusiveness(ItemXmlDataList __instance, ref List<DiceCardXmlInfo> list)
        {
            List<DiceCardXmlInfo> res = new List<DiceCardXmlInfo>();

            foreach (var card in list)
            {
                //card.optionList.Remove(CardOption.Personal);
                card.optionList.Remove(CardOption.OnlyPage);
                res.Add(card);
            }

            list = res;
        }
    }

    [HarmonyPatch(typeof(LibraryModel))]
    internal class TutorialAndUnlocks
    {
        // So that tutorial doesn't softlock the game
        [HarmonyPatch(nameof(LibraryModel.IsClearRats))]
        [HarmonyPrefix]
        static bool RatsTutorial(LibraryModel __instance, ref bool __result)
        {
            __result = true;

            return false;
        }

        [HarmonyPatch(nameof(LibraryModel.IsClearYuns))]
        [HarmonyPrefix]
        static bool YunTutorial(LibraryModel __instance, ref bool __result)
        {
            __result = true;

            return false;
        }


        // Self explainatory
        [HarmonyPatch(nameof(LibraryModel.IsBinahLockedInLibrary))]
        [HarmonyPrefix]
        static bool IsBinahLockedInLibraryPrefix(LibraryModel __instance, ref bool __result)
        {
            __result = !APPlaythruManager.BinahUnlocked;

            return false;
        }

        [HarmonyPatch(nameof(LibraryModel.IsBlackSilenceLockedInLibrary))]
        [HarmonyPrefix]
        static bool IsBlackSilenceLockedInLibraryPrefix(LibraryModel __instance, ref bool __result)
        {
            __result = !APPlaythruManager.BlackSilenceUnlocked;

            return false;
        }

        // Self explainatory
        [HarmonyPatch(nameof(LibraryModel.IsBinahLockedInStage))]
        [HarmonyPrefix]
        static bool IsBinahLockedInStage(LibraryModel __instance, StageClassInfo stageInfo, ref bool __result)
        {
            __result = !APPlaythruManager.BinahUnlocked;

            return false;
        }

        [HarmonyPatch(nameof(LibraryModel.IsBlackSilenceLockedInStage))]
        [HarmonyPrefix]
        static bool IsBlackSilenceLockedInStage(LibraryModel __instance, StageClassInfo stageInfo, ref bool __result)
        {
            __result = !APPlaythruManager.BlackSilenceUnlocked;

            return false;
        }
    }

    [HarmonyPatch(typeof(LibraryFloorModel))]
    internal class Something
    {
        // Uhhh, i forgor :skull:
        [HarmonyPatch(nameof(LibraryFloorModel.UpdateOpenedCount), typeof(int))]
        [HarmonyPrefix]
        static bool UpdateOpenedCountPrefix(LibraryFloorModel __instance)
        {
            int res = 1;
            if (Traverse.Create(__instance).Field<int>("_opendUnitCount").Value != 0)
            {
                res = Traverse.Create(__instance).Field<int>("_opendUnitCount").Value;
            }
            Traverse.Create(__instance).Field<int>("_opendUnitCount").Value = res;

            return false;
        }
    }

    [HarmonyPatch(typeof(UI.UIController))]
    internal class APMessagesPos
    {
        // Change position of AP Messages
        [HarmonyPatch(nameof(UI.UIController.CallUIPhase), typeof(UIPhase))]
        [HarmonyPrefix]
        static void APMessagesPosition(UIController __instance, UIPhase phase)
        {
            switch (phase)
            {
                case UIPhase.Sepiroth:
                case UIPhase.Sephirah:
                case UIPhase.Librarian:
                case UIPhase.Librarian_CardList:
                case UIPhase.FloorFeedingBookFixed:
                case UIPhase.GachaResult:
                case UIPhase.Invitation:
                case UIPhase.Main_ItemList:
                    APLog.Show();
                    APLog.SetLogAtBottom(true);
                    break;
                case UIPhase.DUMMY:
                    APLog.Show();
                    APLog.SetLogAtBottom(false);
                    break;
                case UIPhase.Story:
                case UIPhase.BattleSetting:
                case UIPhase.BattleResult:
                    APLog.Hide();
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(UILibrarySliderPanel))]
    internal class APProgressPatch1
    {
        // Replace library level with the AP Progress
        [HarmonyPatch(nameof(UILibrarySliderPanel.SetData))]
        [HarmonyPrefix]
        static bool APProgressBar(UILibrarySliderPanel __instance)
        {
            LibraryModel instance = LibraryModel.Instance;
            int chapter = instance.GetChapter();
            Traverse.Create(__instance).Field<Image>("img_CityIcon").Value.sprite = UISpriteDataManager.instance._bookGradeFilterIcon[chapter - 1].icon;
            Traverse.Create(__instance).Field<Image>("img_CityIconGlow").Value.sprite = UISpriteDataManager.instance._bookGradeFilterIcon[chapter - 1].iconGlow;

            float num = LORAP.Instance.GetFoundLocationAmount();
            float num2 = LORAP.Instance.GetTotalLocationAmount();
            float sliderLength = Traverse.Create(__instance).Field<float>("sliderLength").Value;
            float x = sliderLength * (num / num2);

            var txt_leveltxt = Traverse.Create(__instance).Field<TextMeshProUGUI>("txt_leveltxt").Value;
            var img_SliderMaskGauge = Traverse.Create(__instance).Field<Image>("img_SliderMaskGauge").Value;

            if (num >= num2)
            {
                txt_leveltxt.text = "MAX";
                img_SliderMaskGauge.rectTransform.sizeDelta = new Vector2(sliderLength, img_SliderMaskGauge.rectTransform.sizeDelta.y);
                return false;
            }

            txt_leveltxt.text = "Lv" + num + "/Lv" + num2;
            img_SliderMaskGauge.rectTransform.sizeDelta = new Vector2(x, img_SliderMaskGauge.rectTransform.sizeDelta.y);

            return false;
        }
    }

    [HarmonyPatch(typeof(UITitlePanel))]
    internal class APProgressPatch2
    {
        // Replace library level with the AP Progress
        [HarmonyPatch("SetMainTitle")]
        [HarmonyPrefix]
        static bool APProgressText(UITitlePanel __instance, string key)
        {
            Traverse.Create(__instance).Field<TextMeshProUGUI>("mainTitleText").Value.GetComponent<TextMeshProMaterialSetter>().underlayColor = new Color(0f, 0.89453125f, 0.46484375f);
            Traverse.Create(__instance).Field<CanvasGroup>("mainTitle").Value.alpha = 1f;
            Traverse.Create(__instance).Field<CanvasGroup>("subTitle").Value.alpha = 0f;
            Traverse.Create(__instance).Field<TextMeshProUGUI>("mainTitleText").Value.text = "Archipelago Progress";
            Traverse.Create(__instance).Field<UILibrarySliderPanel>("sliderPanel").Value.SetData();

            return false;
        }
    }

    [HarmonyPatch(typeof(BookModel))]
    internal class PassiveCostPatch
    {
        // Custom max passive Cost
        [HarmonyPatch(nameof(BookModel.GetMaxPassiveCost))]
        [HarmonyPrefix]
        static bool CustomMaxPassiveCost(DropBookXmlInfo __instance, ref int __result)
        {
            __result = APPlaythruManager.MaxPassiveCost;

            return false;
        }
    }

    [HarmonyPatch(typeof(SaveManager))]
    internal class CustomSaveGame
    {
        // When game tries to save, instead save the game with custom save system
        [HarmonyPatch(nameof(SaveManager.SavePlayData))]
        [HarmonyPrefix]
        static bool SaveGame(SaveManager __instance)
        {
            APSaveManager.SaveGame();

            return false;
        }
    }

    [HarmonyPatch(typeof(UIEscPanel))]
    internal class ToTitleAndEndBattle
    {
        // When you go back to title AP disconnects. And you can also end battle.
        [HarmonyPatch(nameof(UIEscPanel.OnClickEvent))]
        [HarmonyPrefix]
        static bool EndBattleButton(UIEscPanel __instance, int idx)
        {
            if (idx == 1)
            {   
                if (StageController.Instance.Phase != StagePhase.ApplyLibrarianCardPhase && StageController.Instance.Phase != StagePhase.RoundStartPhase_System)
                {
                    UIAlarmPopup.instance.SetAlarmText("You can only end battle when in battle and selecting cards!");

                    return false;
                }
                UIAlarmPopup.instance.SetAlarmText(UIAlarmType.ReturnToTitleWarn_NoPenalty, UIAlarmButtonType.YesNo, EndBattle);

                return false;
            }
            else if (idx == 3)
            {
                UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Click);

                if (Singleton<StageController>.Instance.battleState == StageController.BattleState.None)
                {
                    
                    SingletonBehavior<UIBgScreenChangeAnim>.Instance.StartBg(UIScreenChangeType.ReturnTitle);
                }
                else
                {
                    UIAlarmPopup.instance.SetAlarmText(UIAlarmType.ReturnToTitleWarn_NoPenalty, UIAlarmButtonType.YesNo, __instance.ReturnToTitleAndBattleEnd);
                    UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Click);
                }

                return false;
            }

            return true;
        }

        // Change "Manual" to "End Battle"
        [HarmonyPatch(nameof(UIEscPanel.Open))]
        [HarmonyPostfix]
        static void EndBattleButton1(UIEscPanel __instance)
        {
            __instance.transform.Find("[Rect]Layout/[Button]CustomSelectableGraphic (1)").gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "End Battle";
        }

        private static void EndBattle(bool yes)
        {
            if (!yes) return;

            UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Click);

            LORAP.Instance.APDisconnect();

            SingletonBehavior<UIPopupWindowManager>.Instance.CloseUI(UIPopupType.Esc);

            StageController.Instance.GameOver(false);
            Singleton<SaveManager>.Instance.SavePlayData(1);
            GameSceneManager.Instance.ActivateUIController();
            UI.UIController.Instance.CallUIPhase(UIPhase.BattleResult);
            (UI.UIController.Instance.GetUIPanel(UIPanelType.BattleResult) as UIBattleResultPanel).SetData(new TestBattleResultData
            {
                rewardbookdatas = StageController.Instance._droppedbookdatas,
                iswin = true,
                loseinvitationbooks = new List<LorId>(),
                stagemodelInBattle = StageController.Instance.GetStageModel(),
                sephirahOrder = new List<SephirahType>(Traverse.Create(StageController.Instance).Field<List<SephirahType>>("_usedFloorList").Value)
            });
        }
    }

    [HarmonyPatch(typeof(PassiveModel))]
    internal class PassiveAttributionFix
    {
        [HarmonyPatch(nameof(PassiveModel.LoadFromSaveData))]
        [HarmonyPrefix]
        static bool WhyDoesItEvenBreakBruh(PassiveModel __instance, SaveData data)
        {
            SaveData data2 = data.GetData("passivecurrentid");
            LorId id = LorId.None;
            if (data2 != null)
            {
                id = new LorId(data2.GetInt("_id"));
            }
            SaveData data3 = data.GetData("passiveprevid");
            LorId id2 = LorId.None;
            if (data3 != null)
            {
                id2 = new LorId(data3.GetInt("_id"));
            }
            PassiveXmlInfo data4 = Singleton<PassiveXmlList>.Instance.GetData(id);
            __instance.originpassive = Singleton<PassiveXmlList>.Instance.GetData(id2);
            int @int = data.GetInt("receivebookinstanceid");
            int int2 = data.GetInt("givebookinstanceid");
            __instance.originData = new PassiveModelSavedData(data4, @int, int2);

            return false;
        }
    }

    [HarmonyPatch(typeof(UIFloorPanel))]
    internal class FloorOpenAndUpgradePatch
    {
        // Remove binah and black silence open messages and floor stories on open and etc.
        [HarmonyPatch(nameof(UIFloorPanel.CheckOpenFloor))]
        [HarmonyPrefix]
        static bool CheckOpenFloorPatch(UIFloorPanel __instance)
        {
            return false;
        }
    }
}

using HarmonyLib;
using LORAP.Playthru;
using LORAP.Utils;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.EventTrigger;

namespace LORAP.Patches
{
    internal class EmotionPatches
    {
        // Change amount of emotion coins needed for emotion level up
        [HarmonyPatch(typeof(BattleUnitEmotionDetail), nameof(BattleUnitEmotionDetail.GetNeedEmotionCoin))]
        [HarmonyPrefix]
        static bool NeedEmotionCoinPatch(BattleUnitEmotionDetail __instance, int level, ref int __result)
        {
            __result = level switch
            {
                0 => 3,
                1 => 3,
                2 => 3,
                3 => 5,
                4 => 7,
                5 => 9,
                6 => 9,
                7 => 9,
                8 => 10,
                9 => 10,
                10 => 10,
                11 => 11,
                12 => 11,
                13 => 12,
                14 => 12,
                15 => 13,
                _ => 13,
            };

            return false;
        }



        // Change amount of light gained per emotion level
        [HarmonyPatch(typeof(BattleUnitEmotionDetail), nameof(BattleUnitEmotionDetail.MaxPlayPointAdderByLevel))]
        [HarmonyPrefix]
        static bool LightPerLevel(BattleUnitEmotionDetail __instance, ref int __result)
        {
            __result = __instance.EmotionLevel switch
            {
                int n when (n <= 5) => __instance.EmotionLevel,
                int n when (n <= 15) => 5 + (__instance.EmotionLevel - 5) / 2,
                int n when (n <= 30) => 10 + (__instance.EmotionLevel - 15) / 3,
                _ => 0,
            };

            return false;
        }



        // Change amount of speed dies gained for emotion levels
        [HarmonyPatch(typeof(BattleUnitEmotionDetail), nameof(BattleUnitEmotionDetail.SpeedDiceNumAdder))]
        [HarmonyPrefix]
        static bool SpeedForEmotions(BattleUnitEmotionDetail __instance, ref int __result)
        {
            __result = 0;

            if (StageController.Instance.stageType == StageType.Creature && __instance._self.faction == Faction.Enemy)
            {
                return false;
            }

            __result = __instance.EmotionLevel / 4;
            return false;
        }



        // Copied and rewritten BattleUnitEmotionDetail.Reset with max emotion level change
        [HarmonyPatch(typeof(BattleUnitEmotionDetail), nameof(BattleUnitEmotionDetail.Reset))]
        [HarmonyPrefix]
        static bool FixedReset(BattleUnitEmotionDetail __instance)
        {
            __instance._forcelyLevelUpCount = 0;
            __instance._maximumCoinNumber = BattleUnitEmotionDetail.GetNeedEmotionCoin(__instance._emotionLevel + 1);
            __instance._maximumCoinNumberforEgo = BattleUnitEmotionDetail.GetNeedEmotionCoin(5);

            if (__instance._self.faction == Faction.Enemy)
            {
                __instance._maximumEmotionLevel = StageController.Instance.GetStageModel().ClassInfo.chapter;

                return false;
            }

            __instance._maximumEmotionLevel = Math.Min(PlaythruManager.MaxEmotionLevel, 30);

            return false;
        }



        // Set maximum emotion level (for team)
        [HarmonyPatch(typeof(EmotionBattleTeamModel), nameof(EmotionBattleTeamModel.Init))]
        [HarmonyPostfix]
        static void SetMaxEmotionLevelForTeam(EmotionBattleTeamModel __instance, List<UnitBattleDataModel> units, Faction faction)
        {
            __instance.emotionLevelMax = 30;
        }



        // Make battle unit's botttom emotion level display more than 5 (on init)
        [HarmonyPatch(typeof(BattleCharacterProfileEmotionUI), nameof(BattleCharacterProfileEmotionUI.Init))]
        [HarmonyPrefix]
        static bool UnitEmotionInit(BattleCharacterProfileEmotionUI __instance, int lv)
        {
            __instance.StopAllCoroutines();
            __instance.Anim = __instance.GetComponent<Animator>();
            __instance.Anim.SetTrigger("Default");

            string text = lv.ToRoman();

            __instance.txt_emotionLv_Next.text = text;
            __instance.txt_emotionLv_Prev.text = text;

            return false;
        }



        // Make battle unit's bottom emotion level display more than 5 (after update)
        [HarmonyPatch(typeof(BattleCharacterProfileEmotionUI), nameof(BattleCharacterProfileEmotionUI.UpdateEmotion))]
        [HarmonyPrefix]
        static bool UnitEmotionUpdateBottom(BattleCharacterProfileEmotionUI __instance, int emotionLV)
        {
            __instance.StopAllCoroutines();
            __instance.txt_emotionLv_Next.text = emotionLV.ToRoman();
            __instance.Anim.SetTrigger("LevelUp");

            return false;
        }



        // Make battle unit's top emotion level display more than 5
        [HarmonyPatch(typeof(BattleUnitInformationUI), nameof(BattleUnitInformationUI.SetEmotionLv))]
        [HarmonyPrefix]
        static bool UnitEmotionUpdateTop(BattleUnitInformationUI __instance, int level)
        {
            __instance.txt_LvRome.text = level.ToRoman();

            return false;
        }



        // Fix error from trying to change color of emotion level bars on levels 6+
        [HarmonyPatch(typeof(BattleEmotionInfoManagerUI), nameof(BattleEmotionInfoManagerUI.SetEmotionBarsByTeam))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> FixEmotionBarColor(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeMatcher = new CodeMatcher(instructions, generator);

            codeMatcher.Start().Advance(5).SetOpcodeAndAdvance(OpCodes.Blt_Un_S);

            return codeMatcher.Instructions();
        }



        // Fix top emotion level bar progress for levels 5+
        [HarmonyPatch(typeof(BattleEmotionInfoManagerUI), nameof(BattleEmotionInfoManagerUI.GetSrcToDst))]
        [HarmonyPrefix]
        static bool FixEmotionBarProgress(BattleEmotionInfoManagerUI __instance, int emotionLv, Faction faction, ref Vector3[] __result)
        {
            __result = new Vector3[2];

            Direction allyFormationDirection = StageController.Instance.AllyFormationDirection;

            List<Transform[]> leftTransform = new List<Transform[]>() 
            {
                __instance.left_lv1_fromTo,
                __instance.left_lv2_fromTo,
                __instance.left_lv3_fromTo,
                __instance.left_lv4_fromTo,
                __instance.left_lv5_fromTo,
            };

            List<Transform[]> rightTransform = new List<Transform[]>()
            {
                __instance.right_lv1_fromTo,
                __instance.right_lv2_fromTo,
                __instance.right_lv3_fromTo,
                __instance.right_lv4_fromTo,
                __instance.right_lv5_fromTo,
            };

            int selection = emotionLv;
            if (emotionLv > 4)
                selection = 4;

            // Funny XOR condition
            Transform[] selected = ((faction == Faction.Player) != (allyFormationDirection == Direction.RIGHT)) ? leftTransform[selection] : rightTransform[selection];
            Vector3 pos1;
            Vector3 pos2;
            if (emotionLv > 4)
                pos1 = selected[1].position;
            else
                pos1 = selected[0].position;
            pos2 = selected[1].position;

            __result = new Vector3[] { pos1, pos2 };

            return false;
        }

        // Change Emotion Level info when hovering over unit's emotion level (This is a refactor + change of OnPointerEnter)
        [HarmonyPatch(typeof(BattleUnitProfileInfoUI_EmotionLvTooltip), nameof(BattleUnitProfileInfoUI_EmotionLvTooltip.OnPointerEnter))]
        [HarmonyPrefix]
        static bool CustomEmotionLevelInfo(BattleUnitProfileInfoUI_EmotionLvTooltip __instance, PointerEventData eventData)
        {
            if (__instance._characterProfileUI == null)
                return false;

            BattleUnitModel unitModel = __instance._characterProfileUI.UnitModel;

            if (unitModel == null)
                return false;

            __instance._layout.gameObject.SetActive(true);

            foreach (var item in __instance._slotList)
            {
                item.gameObject.SetActive(false);
            }

            Faction faction = unitModel.faction;
            Direction allyFormationDirection = StageController.Instance.AllyFormationDirection;

            RectTransform component = __instance._layout.GetComponent<RectTransform>();
            component.pivot = ((faction == Faction.Player) != (allyFormationDirection == Direction.RIGHT)) ? new Vector2(1f, 0f) : Vector2.zero;
            Rect rect = component.rect;
            rect.position = Vector2.zero;

            // TooltipSlot.SetActivated()
            BattleUnitProfileInfoUI_EmotionLvTooltip_Slot slot = __instance._slotList[0];

            slot.gameObject.SetActive(true);

            slot._imgLinearDodge.enabled = true;
            slot._imgFrame._Saturation = 1f;
            slot._textUI.text = "Emotion Level Rewards";
            slot._textUI.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 40);
            slot.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(513, 400);

            TextMeshProUGUI text = slot.transform.Find("[Layout]RewardList").gameObject.GetComponentInChildren<TextMeshProUGUI>();
            text.gameObject.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(435, 280);
            text.gameObject.GetComponentInChildren<UITextDataLoader>().enabled = false;
            text.text = "Max Light + 1 at levels 1 - 5, 7, 9, 11, 13, 15, 18, 21, 24, 27 and 30;\n" +
                "Speed Dice +1 every 4 levels;\n" +
                "Level 5: After playing two or more Combat Pages in a Scene, draw an additional page next Scene.";
            text.color = slot._activatedColor;

            return false;
        }
    }
}

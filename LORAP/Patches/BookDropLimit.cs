using HarmonyLib;
using LORAP.Playthru;
using LORAP.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TMPro;
using UI;
using UnityEngine;

namespace LORAP.Patches
{
    [HarmonyPatch(typeof(BattleUnitModel))]
    internal class CustomBookDrops
    {
        static DropBookDataForAddedReward GetRandomBookFromUnit(UnitDataModel unit)
        {
            var drops = unit.DropTable.Select(d => d.Value).SelectMany(t => t.Ids).Where(id => !PlaythruManager.FoundBooks.Contains(id.id)).Distinct().ToList();

            if (drops.Count > 0)
            {
                DropBookDataForAddedReward drop = new DropBookDataForAddedReward(drops.ElementAt(new System.Random().Next(drops.Count)));

                // Also mark the book as found
                PlaythruManager.FoundBooks.Add(drop.id.id);

                return drop;
            }

            return null;
        }

        [HarmonyPatch(nameof(BattleUnitModel.OnDie))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> AnotherEnemyBookDropLimit(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Replace BattleUnitModel.OnDie mechanism of giving books on enemy death with a custom one. Gives only one book from the pool of every book that should drop from enemy
            CIWriter Writer = new CIWriter(instructions, generator);

            Writer.ToPattern(OpCodes.Ldarg_0, OpCodes.Call, OpCodes.Callvirt, OpCodes.Stloc_S);

            Writer.Remove(47);

            Label skip = Writer.AddLabel();

            Writer.Add(new CodeInstruction(OpCodes.Ldarg_0));
            Writer.Add(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(BattleUnitModel), nameof(BattleUnitModel.UnitData))));
            Writer.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(UnitBattleDataModel), nameof(UnitBattleDataModel.unitData))));
            Writer.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CustomBookDrops), nameof(CustomBookDrops.GetRandomBookFromUnit))));
            Writer.Add(new CodeInstruction(OpCodes.Stloc_S, 10));

            Writer.Add(new CodeInstruction(OpCodes.Ldloc_S, 10));
            Writer.Add(new CodeInstruction(OpCodes.Brfalse_S, skip));

            Writer.Add(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(StageController), nameof(StageController.Instance))));
            Writer.Add(new CodeInstruction(OpCodes.Ldloc_S, 10));
            Writer.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(StageController), nameof(StageController.OnEnemyDropBookForAdded))));

            Writer.Add(new CodeInstruction(OpCodes.Ldarg_0));
            Writer.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BattleUnitModel), nameof(BattleUnitModel.view))));
            Writer.Add(new CodeInstruction(OpCodes.Ldloc_S, 10));
            Writer.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(DropBookDataForAddedReward), nameof(DropBookDataForAddedReward.GetLorId))));
            Writer.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(BattleUnitView), nameof(BattleUnitView.OnEnemyDropBook))));

            return Writer.Instructions;
        }
    }

    [HarmonyPatch(typeof(BattleEmotionRewardInfoUI))]
    internal class BookDropInfoPatch
    {
        // Show only books enemies didn't yet drop
        [HarmonyPatch(nameof(BattleEmotionRewardInfoUI.SetData))]
        [HarmonyPrefix]
        static bool BookDropInfo(BattleEmotionRewardInfoUI __instance, List<UnitBattleDataModel> units, Faction faction)
        {
            // Code is partially copied. Would be too tedious to write a transpiler. And i'm lazy :)
            foreach (BattleEmotionRewardSlotUI slot in __instance.slots)
            {
                slot.gameObject.SetActive(false);
            }

            for (int i = 0; i < units.Count; i++)
            {
                BattleEmotionRewardSlotUI slot = __instance.slots[i];
                slot.gameObject.SetActive(true);

                UnitBattleDataModel unit = units[i];

                slot.txt_Name.text = unit.unitData.name;
                if (unit.emotionDetail.EmotionLevel == 0)
                    slot.img_emotionlevel.enabled = false;
                else
                    slot.img_emotionlevel.sprite = UISpriteDataManager.instance.EmotionLevelIcon[unit.emotionDetail.EmotionLevel];

                int j = 0;
                if (faction != Faction.Player)
                {
                    var drops = unit.unitData.DropTable.Select(d => d.Value).SelectMany(t => t.Ids).Where(id => !PlaythruManager.FoundBooks.Contains(id.id)).Distinct().ToList();

                    for (int k = 0; k < drops.Count; k++)
                    {
                        if (slot.rewardtexts.Count <= j)
                            break;

                        slot.rewardtexts[j].text = $"{Singleton<DropBookXmlList>.Instance.GetData(drops[k]).Name} - 1 Copy";
                        slot.rewardtexts[j].gameObject.SetActive(true);
                        slot.SetSizeByText(slot.rewardtexts[j]);
                        j++;
                    }
                }
                for (; j < slot.rewardtexts.Count; j++)
                {
                    slot.rewardtexts[j].gameObject.SetActive(false);
                }

                float height = 0f;
                foreach (TextMeshProUGUI rewardtext in slot.rewardtexts)
                {
                    if (rewardtext.isActiveAndEnabled)
                        height += rewardtext.rectTransform.sizeDelta.y;
                }
                height += 40f;
                Vector2 sizeDelta = slot.rect.sizeDelta;
                sizeDelta.y = height;
                slot.rect.sizeDelta = sizeDelta;
                Vector2 sizeDelta2 = slot.rect_frame.sizeDelta;
                sizeDelta2.y = height + 5f;
                slot.rect_frame.sizeDelta = sizeDelta2;
                Vector2 sizeDelta3 = slot.rect_bg.sizeDelta;
                sizeDelta3.y = height + 25f;
                slot.rect_bg.sizeDelta = sizeDelta3;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(EnemyTeamStageManager_TheCrying))]
    internal class CryingBooksPatches
    {
        // Unstable Books of The Crying Children drop differently, so i limit their drops in other patch
        [HarmonyPatch(nameof(EnemyTeamStageManager_TheCrying.OnStageClear))]
        [HarmonyPrefix]
        static bool CryingChildrenBooks(EnemyTeamStageManager_TheCrying __instance)
        {
            if (PlaythruManager.FoundBooks.Contains(240023))
                return false;

            LorId lorId = new LorId(240023);
            Singleton<StageController>.Instance.OnEnemyDropBookForAdded(new DropBookDataForAddedReward(lorId));
            PlaythruManager.FoundBooks.Add(240023);
            DropBookXmlInfo data = Singleton<DropBookXmlList>.Instance.GetData(lorId);
            if (data == null)
            {
                return false;
            }
            string text = TextDataModel.GetText("BattleUI_GetBook", data.Name);
            SingletonBehavior<BattleManagerUI>.Instance.ui_emotionInfoBar.DropBook(new List<string> { text });

            return false;
        }
    }

    [HarmonyPatch(typeof(UIRewardDropBookList))]
    internal class ResolveableBooksPatch
    {
        // Hide already found books from Reception info
        [HarmonyPatch(nameof(UIRewardDropBookList.SetData))]
        [HarmonyPrefix]
        static bool ResolveableRewardsPatch(UIRewardDropBookList __instance, ref List<LorId> bookids)
        {
            bookids = bookids.Where(id => !PlaythruManager.FoundBooks.Contains(id.id)).ToList();

            return true;
        }
    }

    [HarmonyPatch(typeof(StageController))]
    internal class LCDistortionBookRemove
    {
        // Remove giving book of distortion and book of LC, since they're not used anyway
        [HarmonyPatch(nameof(StageController.BonusRewardWithPopup))]
        [HarmonyPrefix]
        static bool ResolveableRewardsPatch(StageController __instance, LorId stageId)
        {
            return false;
        }
    }
}

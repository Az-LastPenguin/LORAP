using HarmonyLib;
using System.Collections.Generic;
using UI;

namespace LORAP.Patches
{
    [HarmonyPatch(typeof(StageController))]
    internal class BookDropLimit1
    {
        // Don't give the book if that book already dropped. 
        [HarmonyPatch(nameof(StageController.OnEnemyDropBookForAdded))]
        [HarmonyPrefix]
        static bool EnemyBookDropLimit(StageController __instance, DropBookDataForAddedReward data)
        {
            if (ItemLocationManager.BookIds.Contains(data.id.id) && APPlaythruManager.FoundBooks.Contains(data.id.id))
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(BattleUnitView))]
    internal class BookDropLimit2
    {
        // I made two patches so i dont need to do Transpilers for big method or other stuff
        [HarmonyPatch(nameof(BattleUnitView.OnEnemyDropBook))]
        [HarmonyPrefix]
        static bool AnotherEnemyBookDropLimit(BattleUnitView __instance, LorId id)
        {
            if (ItemLocationManager.BookIds.Contains(id.id) && APPlaythruManager.FoundBooks.Contains(id.id))
                return false;

            APPlaythruManager.FoundBooks.Add(id.id);

            return true;
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
            if (APPlaythruManager.FoundBooks.Contains(240023))
                return false;

            LorId lorId = new LorId(240023);
            Singleton<StageController>.Instance.OnEnemyDropBookForAdded(new DropBookDataForAddedReward(lorId));
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
            List<LorId> res = new List<LorId>();

            foreach (var id in bookids)
            {
                if (!ItemLocationManager.BookIds.Contains(id.id) || !APPlaythruManager.FoundBooks.Contains(id.id))
                    res.Add(id);
            }

            bookids = res;

            return true;
        }
    }
}

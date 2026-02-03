using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LORAP.CustomUI;
using TMPro;
using UI;
using UI.Title;
using UnityEngine;
using UnityEngine.UI;

namespace LORAP.Patches
{
    internal class TitlePatches
    {
        // UITitleController patches. This game is so ass i had to split the code into two patches. //
        [HarmonyPatch(typeof(UITitleController),  nameof(UITitleController.OnSelectButton))]
        [HarmonyPostfix]
        static void TitleButtonsPatch(UITitleController __instance, TitleActionType type)
        {
            // I... fuck it. I don't even know.
            if (__instance.TitleButtons != null && __instance.TitleButtons.Count() > 1 && __instance.TitleButtons[1] != null && __instance.TitleButtons[1].gameObject != null && __instance.TitleButtons[1].gameObject.GetComponentInChildren<TextMeshProUGUI>() != null)
                __instance.TitleButtons[1].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Archipelago Connect";
        }

        // Override OnSelectButton because the code is so ass patches break for some reason...
        [HarmonyPatch(typeof(UITitleController), nameof(UITitleController.OnSelectButton))]
        [HarmonyPrefix]
        static bool SelectButtonPatch(UITitleController __instance, TitleActionType type)
        {
            UISoundManager.instance.PlayEffectSound(UISoundType.Card_Over);
            __instance._currentSelectedActionType = type;

            if (__instance.isRuinTitle)
            {
                foreach (var Button in __instance.Ruin_TitleButtons)
                {
                    if (Button.type == type)
                        Button.SetState(ButtonState.Selected);
                    else if (Button.type == TitleActionType.New_Game || Button.type == TitleActionType.Credit)
                        Button.SetState(ButtonState.Disabled);
                    else
                        Button.SetState(ButtonState.Normal);
                }
            }
            else
            {
                foreach (var Button in __instance.TitleButtons)
                {
                    if (Button.type == type)
                        Button.SetState(ButtonState.Selected);
                    else if (Button.type == TitleActionType.New_Game || Button.type == TitleActionType.Credit)
                        Button.SetState(ButtonState.Disabled);
                    else
                        Button.SetState(ButtonState.Normal);
                }
            }

            if (!__instance.isRuinTitle)
                return false;


            __instance.ruin_selectedButtonTextAnim.ResetTrigger("Hide");
            __instance.ruin_selectedButtonTextAnim.SetTrigger("Reveal");

            switch (type)
            {
                case TitleActionType.Continue:
                    __instance.ruin_selectedButtonText.text = "Archipelago Connect";
                    break;
                case TitleActionType.Setting:
                    __instance.ruin_selectedButtonText.text = TextDataModel.GetText("ui_title_setting");
                    break;
                case TitleActionType.Exit:
                    __instance.ruin_selectedButtonText.text = TextDataModel.GetText("ui_title_exit");
                    break;
            }

            return false;
        }

        // To Load the save and connect to AP
        [HarmonyPatch(typeof(UITitleController), nameof(UITitleController.Continue))]
        [HarmonyPrefix]
        static bool ContinuePatch(UITitleController __instance)
        {
            APConnectWindow.Open();

            return false;
        }

        // Ruin Title
        [HarmonyPatch(typeof(UITitleController), nameof(UITitleController.CheckRuinTitle))]
        [HarmonyPrefix]
        static bool RuinTitlePatch(UITitleController __instance)
        {
            if (Gameplay.SaveManager.LoadLastSessionData().Progress < 1f)
                __instance.isRuinTitle = false;
            else
                __instance.isRuinTitle = true;

            return false;
        }



        // EntryScene patch. Display custom CG when loading into the game. //
        public static LatestDataModel GenerateRandomLatestData()
        {
            var AllCGs = new List<Tuple<int, int, int>>() { };

            foreach (var Chapter in StorySerializer.chapters)
            {
                int ChapterKey = Chapter.Key;

                for (int i = 0; i < Chapter.Value.groups.Count; i++)
                {
                    var Group = Chapter.Value.groups.ElementAt(i);
                    for (int j = 0; j < Group.episodes.Count; j++)
                    {
                        AllCGs.Add(new Tuple<int, int, int>(ChapterKey, i + 1, j + 1));
                        //Debug.Log($"CG: {ChapterKey} {i} {j}");
                    }
                }
            }

            var CG = AllCGs.ElementAt(new System.Random().Next(AllCGs.Count));

            var DataModel = new LatestDataModel();
            DataModel.LatestStorychapter = CG.Item1;
            DataModel.LatestStorygroup = CG.Item2;
            DataModel.LatestStoryepisode = CG.Item3;

            return DataModel;
        }

        [HarmonyPatch(typeof(EntryScene), nameof(EntryScene.SetCG))] // TODO: Remake // NOTE: This shit still crashes sometimes!
        [HarmonyPrefix]
        static bool SelectCGPatch(EntryScene __instance)
        {
            Debug.Log("[LORAP] Loading CG");
            // Generate random data and load effectfile from it
            LatestDataModel Data = GenerateRandomLatestData();
            Debug.Log($"[LORAP] Selected Random Chapter: {Data.LatestStorychapter}-{Data.LatestStorygroup}-{Data.LatestStoryepisode}");
            StorySerializer.LoadEffectFile(Data.LatestStorychapter, Data.LatestStorygroup, Data.LatestStoryepisode);

            // Gather all CGs from effectfile and filter them
            List<string> CGs = new List<string>();
            List<string> Exceptions = new List<string>() // Yes, i know, picking them by hand is unfun, i wish i could filter them automatically, but oh well
            {
                "None", "(4-4)Full_stop_office1-1", "(4-5)Full_stop_office1-2", "(4-7)Full_stop_office2-1", "(4-8)Full_stop_office2-2", "(4-11)Dawn_office1-2", "(4-12)Tomery1-1", "(4-13)Tomery1-2", "(4-16)Wedge_office1-1", "(4-16)Wedge_office1-2",
                "ch4_cutscene1_1", "ch4_cutscene1_2", "ch4_cutscene2_1", "ch4_cutscene2_2", "ch5_Index_1", "ch5_Index_2", "ch5_Index_3", "ch5_Puppeteer_1", "ch5_Puppeteer_2", "ch5_Shi Association_2", "ch5_Shi Association_3", "ch5_The Crying Children_1",
                "ch5_The Crying Children_2", "ch5-Sweepers_1", "ch5-Sweepers_2", "ch5-Sweepers_3", "ch6_Liu_sec1_2_1", "ch6_Liu_sec1_2_2", "ch6_Liu_sec2_2_1", "ch6_Liu_sec2_2_2", "ch6_Liu_sec2_2_3", "ch6_PupleTear_1", "ch6_Rcop_ep1_1", "ch6_Rcop_ep1_2",
                "ch6_Rcop_ep1_3", "ch6_Rcop_ep2_2", "ch6_Rcop_ep2_3", "ch6_Rcop_ep2_4", "ch6_RedMist_1", "ch6_RedMist_2", "ch6_RedMist_3", "ch6_The Index_1", "ch6_The Index_2", "ch6_The Index_3", "ch6_The Index_5", "ch6_The Index_6", "ch6_The Index_7",
                "ch6_thumb_2_1", "ch6_thumb_2_2", "ch6_thumb_2_3", "ch7_Blue_3_1", "ch7_End_2", "ch7_Hana_ep1_2", "ch7_Hana_ep1_3", "ch7_Hana_ep2_1", "ch7_Hana_ep2_2", "ch7_he Blue Reverberation_ep2_1_1", "ch7_he Blue Reverberation_ep2_1_2",
                "ch7_he Blue Reverberation_ep2_1_3", "ch8_Binah_ep5_1", "ch8_Binah_ep5_2", "ch8_Binah_ep5_4", "ch8_Binah_ep5_5", "ch8_Binah_ep5_7", "ch8_Chesed_ep5_1", "ch8_Chesed_ep5_2", "ch8_Chesed_ep5_4", "ch8_Chesed_ep5_5", "ch8_Chesed_ep5_6",
                "ch8_Hod_ep5_1", "ch8_Hod_ep5_2", "ch8_Hod_ep5_3", "ch8_Hod_ep5_4", "ch8_Hod_ep5_5", "ch8_Hod_ep5_7", "ch8_Hod_ep5_8", "ch8_Hod_ep5_9", "ch8_Hod_ep5_10", "ch8_Hod_ep5_11", "ch8_Hod_ep5_12", "ch8_Hod_ep5_14", "ch8_Hod_ep5_15",
                "ch8_Hod_ep5_16", "ch8_Hod_ep5_17", "ch8_Hod_ep5_18", "ch8_Keter_0", "ch8_Malkuth_ep5_1", "ch8_Malkuth_ep5_2", "ch8_Malkuth_ep5_3", "ch8_Netzach_1", "ch8_Netzach_2", "ch8_Netzach_3", "ch8_Yesod_ep5_1", "Gebura_ep3_1",
                "Gebura_ep3_2", "Gebura_ep5_1", "Gebura_ep5_2", "Gebura_ep5_3", "Hod_ep3-1", "Hod_ep3-2", "Hod_ep3-3", "Yesod_ep1-1", "Yesod_ep1-2", "Yesod_ep1-3", "Yesod1", "Yesod2", "Yesod3", "Yesod4", "Yesod5", "Yesod6", "Yesod7",
                "검은화면", "검은화면_반투명", "검은화면_투명", "공백화면", "롤안 2", "케테르1", "케테르2", "켙0", "필립에고화"
            };

            foreach (var d in StorySerializer.effectDefinition.dlgEffectList)
            {
                if (Exceptions.Contains(d.bg.src))
                    continue;

                CGs.Add(d.bg.src);
            }
            Debug.Log($"[LORAP] Found CGs: {CGs.Count}");
            Sprite sprite;
            if (CGs.Count == 0)
            {
                StorySerializer.LoadEffectFile(1, 1, 1);
                sprite = (Sprite)Resources.Load("StoryResource/BgSprites/" + StorySerializer.effectDefinition.cg.src, typeof(Sprite));

                if (sprite != null)
                    __instance.CGImage.sprite = sprite;

                return false;
            }

            sprite = (Sprite)Resources.Load("StoryResource/BgSprites/" + CGs.ElementAt(new System.Random().Next(CGs.Count)), typeof(Sprite));

            if (sprite != null)
                __instance.CGImage.sprite = sprite;

            return false;
        }
    }
}

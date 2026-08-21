using LORAP.Archipelago;
using LORAP.Playthru;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace LORAP.CustomUI.Components
{
    internal class ChapterSeparator : MonoBehaviour
    {
        internal int Chapter;

        private GameObject ProgressBar;
        private Image ProgressBarFrontImage;
        private TextMeshProUGUI InfoText;

        public void Awake()
        {
            Transform iconHolder = transform.Find("Holder/Chapter/Icon");
            iconHolder.Find("Icon").GetComponent<Image>().sprite = UISpriteDataManager.instance._bookGradeFilterIcon[Chapter - 1].icon;
            iconHolder.Find("Back").GetComponent<Image>().sprite = UISpriteDataManager.instance._bookGradeFilterIcon[Chapter - 1].iconGlow;
            transform.Find("Holder/Chapter/Name").GetComponent<TextMeshProUGUI>().text = TextDataModel.GetText($"ui_maintitle_citystate_{Chapter}");

            ProgressBar = transform.Find("Holder/Progress").gameObject;
            ProgressBarFrontImage = ProgressBar.transform.Find("Front").GetComponent<Image>();
            InfoText = transform.Find("Holder/Text").GetComponent<TextMeshProUGUI>();
        }

        internal void UpdateSeparator()
        {
            // If the chapter is unlocked OR progression is set to Battle Graph, show completion
            if (SettingsManager.RunProgressionMode == ProgressionMode.BattleGraph || SlotDataManager.IsSphereClearEnough(Chapter - 1))
            {
                ProgressBar.SetActive(false);

                List<BattleNode> sphereNodes = SlotDataManager.GetLogicalSphereNodes(Chapter);
                int cleared = sphereNodes.Count(n => PlaythruManager.IsStageComplete(n.Id));
                
                InfoText.text = $"Completion: {cleared}/{sphereNodes.Count} - {((float)cleared / sphereNodes.Count).ToString("P1")}";
            }
            else if (SlotDataManager.IsSphereClearEnough(Chapter - 2)) // Else, if prev chapter is unlocked, show how many stages have to be cleared in order to open this chapter
            {
                ProgressBar.SetActive(true);

                List<BattleNode> sphereNodes = SlotDataManager.GetLogicalSphereNodes(Chapter - 1);
                int percentage = Math.Max(0, Math.Min(100, SettingsManager.ChapterClearPercentage.GetValue()));
                int required = (int)Math.Ceiling(sphereNodes.Count * percentage / 100.0);
                int cleared = sphereNodes.Count(n => PlaythruManager.IsStageComplete(n.Id));
                float progress = (float)cleared / required;

                InfoText.text = $"Unlock Progress: {cleared}/{required} - {(progress).ToString("P1")}";

                ProgressBarFrontImage.fillAmount = progress;
            }
            else
            {
                ProgressBar.SetActive(false);

                InfoText.text = $"Can't access this chapter yet!";
            }
        }
    }
}

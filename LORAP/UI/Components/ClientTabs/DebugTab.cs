using LORAP.Archipelago;
using LORAP.Gameplay;
using LORAP.Playthru;
using TMPro;
using UnityEngine;

namespace LORAP.CustomUI.Components.ClientTabs
{
    internal class DebugTab : MonoBehaviour, IClientTab
    {
        private GameObject Tab;

        private TextMeshProUGUI ClientSeed;
        private TextMeshProUGUI APSeed;
        private TextMeshProUGUI KeterStage;
        private TextMeshProUGUI MaxEmotion;
        private TextMeshProUGUI BoostersOpened;
        private TextMeshProUGUI BoEOpened;
        private TextMeshProUGUI BoEAvailable;
        private TextMeshProUGUI ModVersion;
        private TextMeshProUGUI ItemsReceived;

        public void Awake()
        {
            // Get all the stuff
            Tab = gameObject;

            ClientSeed = transform.Find("ClientSeed").GetComponent<TextMeshProUGUI>();
            APSeed = transform.Find("APSeed").GetComponent<TextMeshProUGUI>();
            KeterStage = transform.Find("KeterStage").GetComponent<TextMeshProUGUI>();
            MaxEmotion = transform.Find("MaxEmotion").GetComponent<TextMeshProUGUI>();
            BoostersOpened = transform.Find("BoostersOpened").GetComponent<TextMeshProUGUI>();
            BoEOpened = transform.Find("BoEOpened").GetComponent<TextMeshProUGUI>();
            BoEAvailable = transform.Find("BoEAvailable").GetComponent<TextMeshProUGUI>();
            ModVersion = transform.Find("ModVersion").GetComponent<TextMeshProUGUI>();
            ItemsReceived = transform.Find("Items Received").GetComponent<TextMeshProUGUI>();
        }

        public void UpdateTab()
        {
            ClientSeed.text = $"Client Seed: {SlotDataManager.ClientSeed}";
            APSeed.text = $"AP Seed: {SessionManager.RoomSeed}";
            KeterStage.text = $"Keter Stage: {PlaythruManager.KeterRealizationStage}";
            MaxEmotion.text = $"Max Emotion: {PlaythruManager.MaxEmotionLevel}";
            BoostersOpened.text = $"Booster Packs Opened: {BookDropManager.BoosterPacksOpened}";
            BoEOpened.text = $"Books of Everything Opened: {BookDropManager.BoosterPacksOpened}";
            BoEAvailable.text = $"Books of Everything Available: {PlaythruManager.GetUnlockedBoEBundleLimit()}";
            ModVersion.text = $"Mod Version: {LORAP.ModVersion}";
            ItemsReceived.text = $"Items Received: {ItemManager.ItemsReceived}";
        }

        public void ResetTab()
        {

        }
    }
}

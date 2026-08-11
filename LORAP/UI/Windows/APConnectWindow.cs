using LORAP.Archipelago;
using LORAP.Playthru;
using UnityEngine;
using System;
using TMPro;

namespace LORAP.CustomUI 
{
    internal class APConnectWindow : SingletonBehavior<APConnectWindow>
    {
        private GameObject Panel;

        private Animator Animator;

        private TMP_InputField IPInput;
        private TMP_InputField SlotInput;
        private TMP_InputField PassInput;

        private TextMeshProUGUI InfoText;


        internal string IP => IPInput?.text ?? "";
        internal string Slot => SlotInput?.text ?? "";
        internal string Password => PassInput?.text ?? "";

        internal void Awake()
        {
            // Get all the stuff
            Panel = transform.Find("Window").gameObject;

            Animator = Panel.GetComponent<Animator>();

            Transform Inputs = Panel.transform.Find("Window/Container/Layout/CenterPanel/Inputs");
            IPInput = Inputs.Find("IPPortInput").GetComponent<TMP_InputField>();
            SlotInput = Inputs.Find("SlotInput").GetComponent<TMP_InputField>();
            PassInput = Inputs.Find("PasswordInput").GetComponent<TMP_InputField>();

            InfoText = Panel.transform.Find("Window/Container/Layout/CenterPanel/Texts/InfoText").GetComponent<TextMeshProUGUI>();

            // Add button events
            Transform Buttons = Panel.transform.Find("Window/Container/ButtonLayout");
            Buttons.Find("Connect").gameObject.AddComponent<BasicButton>().MouseClickEvent.AddListener((_) => TryStartGame());
            Buttons.Find("Cancel").gameObject.AddComponent<BasicButton>().MouseClickEvent.AddListener((_) => Close());

            // Start inactive
            Panel.SetActive(false);
        }

        internal void Open()
        {
            SetLastSessionData();

            Animator.SetTrigger("ConnectWindowReveal");

            Panel.SetActive(true);
        }

        internal void Close()
        {
            Panel.SetActive(false);
        }

        private void SetLastSessionData()
        {
            var LastData = SessionManager.GetLastSessionData();

            IPInput.text = LastData?.IP ?? "";
            SlotInput.text = LastData?.SlotName ?? "";

            InfoText.text = $"Last run progress: {LastData?.Progress.ToString("P1") ?? "Unknown"}";
        }

        private void TryStartGame()
        {
            try
            {
                // Try connecting to the slot
                SessionManager.TryConnect(IP, Slot, Password);

                // If we successfully connected, start the game
                PlaythruManager.StartGame();
            }
            catch (Exception e)
            {
                // Make the error message
                string origin = $"{e.TargetSite.DeclaringType.FullName}.{e.TargetSite.Name}";
                string exText = $"[{origin}]\n{e.Message}";

                // Show the error message
                InfoText.text = exText;

                // Abort the mission
                SessionManager.EndSession();

                // Log the exception
                Debug.LogException(e);
                return;
            }
        }
    }
}

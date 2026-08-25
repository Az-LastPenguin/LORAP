using LORAP.Archipelago;
using LORAP.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LORAP.CustomUI
{
    internal class APChatWindow : SingletonBehavior<APChatWindow>, IDragHandler
    {
        private GameObject MessagePrefab = AssetBundleHelper.GetAsset("APChatMessage");

        private GameObject Panel;

        private RectTransform Window;

        private Transform MessageHolder;

        internal void Awake()
        {
            // Get all the stuff
            Panel = transform.Find("Window").gameObject;
            Window = Panel.GetComponent<RectTransform>();

            MessageHolder = Panel.transform.Find("Main/ScrollRect/Viewport/Content");

            // Add button events
            Panel.transform.Find("Header/Clear").gameObject.AddComponent<BasicButton>().MouseClickEvent.AddListener((_) => ClearMessages());

            // Listen for MessageEvent to show messages
            SessionManager.MessageEvent += AddMessage;

            // Start inactive
            Panel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                if (Panel.activeSelf)
                    Close();
                else
                    Open();
            }
        }

        internal void Open()
        {
            Panel.SetActive(true);
        }

        internal void Close()
        {
            Panel.SetActive(false);
        }

        internal void AddMessage(string message)
        {
            Instantiate(MessagePrefab, MessageHolder).GetComponent<TextMeshProUGUI>().text = message;
        }

        internal void ClearMessages()
        {
            for (int i = MessageHolder.childCount - 1; i >= 0; i--)
            {
                Destroy(MessageHolder.GetChild(i).gameObject);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 newPos;
            if (Input.GetKey(KeyCode.LeftControl)) // If Ctrl is being held, we resize the window
            {
                // 600x250 min; 1000x500 max
                Window.sizeDelta = new Vector2(Mathf.Clamp(Window.sizeDelta.x + eventData.delta.x, 600, 1000), Mathf.Clamp(Window.sizeDelta.y + eventData.delta.y, 250, 500));

                newPos = Window.position;
            }
            else // If not, we set the new position based on mouse delta
            {
                newPos = Window.position + (Vector3)eventData.delta;
            }

            Rect safe = Screen.safeArea;
            safe.xMin += Window.rect.width * Window.lossyScale.x * 0.5f;
            safe.xMax -= Window.rect.width * Window.lossyScale.x * 0.5f;
            safe.yMin += Window.rect.height * Window.lossyScale.y * 0.5f;
            safe.yMax -= Window.rect.height * Window.lossyScale.y * 0.5f;

            newPos.x = Mathf.Clamp(newPos.x, safe.xMin, safe.xMax);
            newPos.y = Mathf.Clamp(newPos.y, safe.yMin, safe.yMax);

            Window.position = newPos;
        }
    }
}

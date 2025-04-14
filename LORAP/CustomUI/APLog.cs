using LORAP.Utils;
using System.Linq;
using TMPro;
using UnityEngine;

namespace LORAP.CustomUI
{
    internal static class APLog
    {
        private static GameObject Panel;

        private static GameObject LogPrefab = PrefabHelper.GetPrefab("archipelagolog", "Log");

        internal static int LogCount { get; private set; }

        internal static bool isAtBottom { get; private set; } = true;

        private static void Init()
        {
            Panel = GameObject.Instantiate(PrefabHelper.GetPrefab("archipelagolog", "APActionsLog"));

            Panel.transform.Find("LogHolder").localPosition = new Vector3(-950, 0, 0);

            LogPrefab.GetComponent<TextMeshProUGUI>().font = UIHelper.Font2;

            SetLogAtBottom(true);
            Panel.SetActive(false);
        }

        public static void Show()
        {
            if (Panel == null)
                Init();

            Panel.SetActive(true);
        }

        public static void Hide()
        {
            if (Panel == null)
                Init();

            Panel.SetActive(false);
        }

        public static void AddLog(string message)
        {
            if (Panel == null)
                Init();

            var Holder = Panel.transform.Find("LogHolder");

            var Log = Object.Instantiate(LogPrefab, Holder);
            Log.name = $"Log{LogCount}";
            Log.GetComponent<TextMeshProUGUI>().text = message;
            var LogHeight = Log.GetComponent<TextMeshProUGUI>().preferredHeight + 10;
            Log.transform.localPosition = isAtBottom ? new Vector3(0, -530 + LogHeight, 0) : new Vector3(0, 440 - LogHeight, 0);
            Log.GetComponent<TextMeshProUGUI>().alignment = isAtBottom ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.Center;

            foreach (var log in Holder.gameObject.GetComponentsInChildren<Transform>().Where(t => t != Holder & t != Log.transform))
            {
                log.position += isAtBottom ? new Vector3(0, LogHeight, 0) : new Vector3(0, -LogHeight, 0);
            }

            LogCount++;

            Timing.After(3, () =>
            {
                Log.GetComponent<TextMeshProUGUI>().CrossFadeAlpha(0f, 3f, false);

                Timing.After(3, () => GameObject.Destroy(Log));
            });
        }

        public static void SetLogAtBottom(bool atBottom)
        {
            isAtBottom = atBottom;

            var Holder = Panel.transform.Find("LogHolder");

            var List = Holder.gameObject.GetComponentsInChildren<RectTransform>().Where(t => t != Holder);

            if (List != null && List.Count() > 0)
                return;

            if (isAtBottom)
                List = List.Reverse();

            var curHeight = isAtBottom ? -530f : 440f;
            for (int i = 0; i < List.Count(); i++)
            {
                var Log = List.ElementAt(i);
                //Debug.Log(Log);
                if (!Log.gameObject.scene.IsValid())
                    continue;

                var LogHeight = Log.GetComponent<TextMeshProUGUI>().preferredHeight + 10;
                curHeight += isAtBottom ? LogHeight : -LogHeight;
                Log.localPosition = isAtBottom ? new Vector3(0, curHeight, 0) : new Vector3(0, curHeight, 0);
                //Debug.Log("C");
                Log.gameObject.GetComponent<TextMeshProUGUI>().alignment = isAtBottom ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.Center;
                //Debug.Log("D");
            }
        }
    }
}

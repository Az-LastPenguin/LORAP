using BepInEx.Logging;
using BTAI;
using System.Collections;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LORAP.CustomUI
{
    internal static class APLog
    {
        private static GameObject Panel;

        private static GameObject LogPrefab = PrefabHelper.GetPrefab("archipelagolog", "Log");

        private static int LogCount;

        private static bool isAtBottom = true;

        private static void Init()
        {
            Panel = Object.Instantiate(PrefabHelper.GetPrefab("archipelagolog", "APActionsLog"));
            //LogPrefab = PrefabHelper.GetPrefab("archipelagolog", "Log");

            Panel.transform.Find("LogHolder").localPosition = new Vector3(-950, 0, 0);

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

            foreach (var log in Holder.gameObject.GetComponentsInChildren<Transform>().Where(t => t != Holder))
            {
                log.position += isAtBottom ? new Vector3(0, 40, 0) : new Vector3(0, -40, 0);
            }

            var Log = Object.Instantiate(LogPrefab, Holder);
            Log.name = $"Log{LogCount}";
            Log.GetComponent<TextMeshProUGUI>().text = message;
            Log.transform.localPosition = isAtBottom ? new Vector3(0, -490, 0) : new Vector3(0, 400, 0);
            Log.GetComponent<TextMeshProUGUI>().alignment = isAtBottom ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.Center;

            LogCount++;

            LORAP.Instance.StartCoroutine(RemoveInSeconds(3, Log));
        }

        private static IEnumerator RemoveInSeconds(int sec, GameObject obj)
        {
            yield return new WaitForSeconds(3);

            obj.GetComponent<TextMeshProUGUI>().CrossFadeAlpha(0f, 3f, false);

            yield return new WaitForSeconds(sec);

            Object.Destroy(obj);
        }

        public static void SetLogAtBottom(bool atBottom)
        {
            isAtBottom = atBottom;

            var Holder = Panel.transform.Find("LogHolder");

            var List = Holder.gameObject.GetComponentsInChildren<RectTransform>().Where(t => t != Holder);
            if (isAtBottom)
                List.Reverse();

            var i = 0;
            foreach (var log in List)
            {
                log.localPosition = isAtBottom ? new Vector3(0, -490 + 40 * i, 0) : new Vector3(0, 400 - 40 * i, 0);
                log.gameObject.GetComponent<TextMeshProUGUI>().alignment = isAtBottom ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.Center;
                i++;
            }
        }
    }
}

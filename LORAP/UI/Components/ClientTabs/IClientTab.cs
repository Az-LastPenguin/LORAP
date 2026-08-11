using UnityEngine;

namespace LORAP.CustomUI.Components.ClientTabs
{
    internal interface IClientTab
    {
        GameObject gameObject { get; }

        void UpdateTab();

        void ResetTab();
    }
}

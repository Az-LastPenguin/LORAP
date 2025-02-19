using System;
using System.Collections;
using HarmonyLib;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using System.Reflection.Emit;
using System.Linq;
using System.Diagnostics;

namespace LORAP.Utils
{
    // Class to help accessing some UI elements/assets of the game to use in custom UI
    internal static class UIHelper
    {
        internal static TMP_FontAsset Font1 => UIAlarmPopup.instance.transform.Find("[Rect]Normal/[Text]AlarmText").gameObject.GetComponent<TextMeshProUGUI>().font;

        internal static TMP_FontAsset Font2 => UIControlManager.Instance.GetTitlePanel().sliderPanel.txt_leveltxt.font;

        internal static TMP_FontAsset Font3 => UIPopupWindowManager.Instance.popupPanels[(int)UIPopupType.Option].transform.Find("[Text]Title_TextMesh").gameObject.GetComponent<TextMeshProUGUI>().font;
        internal static Material Font3Material => UIPopupWindowManager.Instance.popupPanels[(int)UIPopupType.Option].transform.Find("[Text]Title_TextMesh").gameObject.GetComponent<TextMeshProUGUI>().fontMaterial;
    }

    // Custom class made to access Coroutines without needing to create a new GameObject or search for one
    // Also has some utility functions for timing of things
    internal class Timing : MonoBehaviour
    {
        internal static void Setup(GameObject newInstance)
        {
            instance = newInstance.GetComponent<Timing>();
        }

        private static Timing instance;

        public static Coroutine Coroutine(IEnumerator enumerator)
        {
            return instance.StartCoroutine(enumerator);
        }

        public static Coroutine After(float time, Action func)
        {
            IEnumerator Coroutine()
            {
                yield return new WaitForSeconds(time);

                func.Invoke();
            };

            return instance.StartCoroutine(Coroutine());
        }
    }


    // Custom class made by me to make Transpilers creation easier
    // Basically a bunch of macros
    internal class CIWriter
    {
        public List<CodeInstruction> Instructions { get; private set; }
        public ILGenerator Generator { get; private set; }
        public int Pointer { get; private set; }
        public CodeInstruction Current => Instructions[Pointer] ?? null;
        public List<Label> Labels => Current.labels;
        public int Length => Instructions.Count;

        public CIWriter(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            Instructions = instructions.ToList();
            Generator = generator;
            Pointer = 0;
        }

        public bool Next()
        {
            if (Pointer + 1 > Length)
                return false;

            Pointer++;

            return true;
        }

        public bool Prev()
        {
            if (Pointer - 1 < 0)
                return false;

            Pointer--;

            return true;
        }

        public bool ToPattern(params OpCode[] opCodes)
        {
            List<OpCode> codes = opCodes.ToList();

            for (int i = 0; i < Length; i++)
            {
                for (int j = 0; j < codes.Count; j++)
                {
                    if (Instructions[i + j].opcode != codes[j])
                        goto next;
                }

                Pointer = i;
                return true;

                next:
                continue;
            }

            return false; 
        }

        public bool To(int Pos)
        {
            if (Pos < 0 || Pos > Length)
                return false;

            Pointer = Pos;

            return true;
        }

        public CodeInstruction At(int Pos)
        {
            if (Pos < 0 || Pos >= Length)
                return null;

            return Instructions[Pos];
        }

        // Made specifically to repalce instrcution and retain it's labels, to not waste time copying them and placing onto new instruction. Laziness is my middlename
        public void Nop()
        {
            Instructions[Pointer].opcode = OpCodes.Nop;
            Instructions[Pointer].operand = null;
            Next();
        }

        public void Remove(int Amount = 1)
        {
            Instructions.RemoveRange(Pointer, Amount);
        }

        public void Add(CodeInstruction instruction)
        {
            Instructions.Insert(Pointer, instruction);
            Next();
        }

        public void Insert(CodeInstruction instruction)
        {
            Instructions.Insert(Pointer, instruction);
        }

        public void AddLabel(Label label)
        {
            Current.WithLabels(label);
        }

        public Label AddLabel()
        {
            Label label = Generator.DefineLabel();
            Current.WithLabels(label);
            return label;
        }

        public Label NewLabel()
        {
            return Generator.DefineLabel();
        }

        public void Dump()
        {
            UnityEngine.Debug.Log("------------------------------");
            UnityEngine.Debug.Log($"Dump of Transpiler patch '{new StackFrame(1, true).GetMethod().Name}':\n");
            Instructions.ForEach(i => UnityEngine.Debug.Log($"{i.ToString()}"));
            UnityEngine.Debug.Log("\n------------------------------");
        }
    }
}

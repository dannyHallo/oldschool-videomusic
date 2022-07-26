using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TerminalAccess : MonoBehaviour
{
    public TextMeshProUGUI codeUI;
    public Image caret;
    string activeLine = "";
    public string codeToBeProcessed = "";

    string code = "";
    float fontSize;
    int charIndex = 0;
    Vector2Int caretIndex;
    int[] keyValues;
    List<KeyCode> currentPressingKeycodes;
    bool userCanPushTheButton = true;
    bool holdingCheck = false;
    bool userHasPushedForAWhile = false;
    static readonly KeyCode[] specialKeycodes = { KeyCode.Return, KeyCode.Backspace };
    const string legalChars = "abcdefghijklmnopqrstuvwxyz 0.123456789+-/*=<>()[]{},!@#$%^&?|`'~_";

    private void Awake()
    {
        OnAwake();
    }

    protected virtual void OnAwake()
    {
        charIndex = code.Length;
        currentPressingKeycodes = new List<KeyCode>();
        keyValues = (int[])System.Enum.GetValues(typeof(KeyCode));

        fontSize = codeUI.fontSize;
        caret.GetComponent<RectTransform>().localScale = new Vector3(fontSize * 0.875f, fontSize, 1);
    }

    private void UpdateActiveLine()
    {
        string[] splitedCode = code.Split('\n');
        if (splitedCode != null && splitedCode.Length != 0)
            activeLine = splitedCode[splitedCode.Length - 1].ToLower();
    }

    protected void Writeln(string str)
    {
        code += str + "\n";
        charIndex += str.Length + 1;
        caretIndex.x = 0;
        caretIndex.y++;
    }

    protected void InputHandler()
    {
        TextInputHandler();
        SpecialInputHandler();
        UpdateTerminal();
        UpdateActiveLine();
    }

    private void TextInputHandler()
    {
        string input = Input.inputString;
        foreach (char c in input)
        {
            if (legalChars.Contains(c.ToString().ToLower()))
            {
                // Append
                if (string.IsNullOrEmpty(code) || charIndex == code.Length)
                {
                    code += c;
                }
                // Insert
                else
                {
                    code = code.Insert(charIndex, c.ToString());
                }
                charIndex++;
                caretIndex.x++;
            }
        }
    }

    private void SpecialInputHandler()
    {
        currentPressingKeycodes.Clear();
        bool specialKeyIsPushed = false;

        for (int i = 0; i < keyValues.Length; i++)
        {
            KeyCode currentlyCheckingKeycode = (KeyCode)keyValues[i];
            if (Input.GetKey(currentlyCheckingKeycode))
            {
                if (!currentPressingKeycodes.Contains(currentlyCheckingKeycode))
                    currentPressingKeycodes.Add(currentlyCheckingKeycode);
            }
        }

        foreach (KeyCode specialKeycode in specialKeycodes)
        {
            if (currentPressingKeycodes.Contains(specialKeycode))
            {
                specialKeyIsPushed = true;
                if (!userCanPushTheButton)
                    return;
                ToggleButtonCooldown();
                break;
            }
        }

        if (!specialKeyIsPushed)
        {
            userHasPushedForAWhile = false;
            userCanPushTheButton = true;
            return;
        }

        if (!userCanPushTheButton)
        {
            return;
        }

        HoldingWaiting();

        if (Input.GetKey(KeyCode.Return))
        {
            codeToBeProcessed = activeLine;
            code = code.Insert(charIndex, "\n");
            charIndex++;
            caretIndex.x = 0;
            caretIndex.y++;
        }

        if (Input.GetKey(KeyCode.Backspace))
        {
            if (caretIndex.x > 0)
            {
                code = code.Remove(charIndex - 1, 1);
                charIndex--;
                caretIndex.x--;
            }
        }

        userCanPushTheButton = false;
    }

    private void UpdateTerminal()
    {
        codeUI.text = code;
        caret.GetComponent<RectTransform>().anchoredPosition = new Vector2(fontSize * caretIndex.x, -fontSize * caretIndex.y);
    }



    protected void ToggleButtonCooldown()
    {
        if (!userCanPushTheButton)
            return;

        StopCoroutine(ResetButtonPushEnable());
        StartCoroutine(ResetButtonPushEnable());
    }

    protected void HoldingWaiting()
    {
        if (holdingCheck)
            return;
        holdingCheck = true;
        StopCoroutine(WaitForContinuousInput());
        StartCoroutine(WaitForContinuousInput());
    }



    IEnumerator ResetButtonPushEnable()
    {
        if (userHasPushedForAWhile)
        {
            yield return new WaitForSeconds(0.05f);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }
        userCanPushTheButton = true;
    }

    IEnumerator WaitForContinuousInput()
    {
        yield return new WaitForSeconds(0.5f);
        userHasPushedForAWhile = true;
        holdingCheck = false;
    }
}

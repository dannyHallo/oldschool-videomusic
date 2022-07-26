using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;

public class TerminalInterpreter : TerminalAccess
{
    public CRTEffect cRTEffect;

    protected override void OnAwake()
    {
        base.OnAwake();
        Writeln("OS: Win10");
        Writeln("Audio Device: AKG K712");
    }

    private void Update()
    {
        InputHandler();
        if (InterpreteCode())
        {
            codeToBeProcessed = "";
        }
    }

    bool InterpreteCode()
    {
        if (codeToBeProcessed.Length == 0)
            return false;

        string[] splitedCode = codeToBeProcessed.Split(' ');

        if (splitedCode.Length != 3)
        {
            Writeln("Command invalid!");
            return true;
        }

        float value = 0;
        try
        {
            value = float.Parse(splitedCode[2]);
        }
        catch (FormatException)
        {
            Writeln("Value is invalid!");
            return true;
        }
        catch (OverflowException)
        {
            Writeln("Value is invalid!");
            return true;
        }

        int errorType = cRTEffect.SetProperty(splitedCode[0], splitedCode[1], value);
        switch (errorType)
        {
            case -1:
                Writeln("Command not found");
                return true;
            case -2:
                Writeln("Value name not found");
                return true;
            case -3:
                Writeln("Value out of bound");
                return true;
        }
        return true;
    }
}

﻿using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System;
using System.IO;
using LCPrinter;
using UnityEngine.UI;

public class LCExampleScript : MonoBehaviour {

    public Texture2D texture2D;
    public string printerName = "";
    public int copies = 1;

    public InputField inputField;

    public void printSmileButton()
    {
        Print.PrintTexture(texture2D.EncodeToPNG(), copies, printerName);
    }

    public void printByPathButton()
    {
        Print.PrintTextureByPath(inputField.text.Trim(), copies, printerName);
    }
}

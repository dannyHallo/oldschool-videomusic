using Assets.WasapiAudio.Scripts.Unity;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Original script by Darren O'Neale (coderDarren)
// https://github.com/coderDarren/RenaissanceCoders_UnityScripting

[RequireComponent(typeof(Image))]
public class AudioSyncColor : AudioVisualizationEffect
{
    public Color[] colorSelection;
    public Color restColor;
    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    private Color RandomColor()
    {
        if (colorSelection == null || colorSelection.Length == 0) return Color.white;
        return colorSelection[UnityEngine.Random.Range(0, colorSelection.Length)];
    }

    private void Update()
    {
        image.color = Color.Lerp(image.color, restColor, 2f * Time.deltaTime);
    }
}


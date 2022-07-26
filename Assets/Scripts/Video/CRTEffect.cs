using System;
using UnityEngine;

// Original script by Jasper Flick (Catlike Coding)
// https://catlikecoding.com/unity/tutorials/advanced-rendering/fxaa/
// Also inspired by Sebastian Lague

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
[CreateAssetMenu(menuName = "PostProcessing/CRT")]
public class CRTEffect : PostProcessingEffect
{
    [Header("Shader")]
    public Shader crtShader;

    [Header("set weight")]
    [Range(0, 1f)] public float whiteNoiseMultiplier = 1.0f;
    [Range(0, 1f)] public float scanlineMultiplier = 1.0f;
    [Range(0, 1f)] public float horzFuzzMultiplier = 0.8f;

    [Header("set amp")]
    public float colorDrift = 0.5f;

    [Header("set freq")]
    [Range(0, 1.0f)] public float scrollLikelihood = 0.2f;
    [Range(0, 1f)] public float horzFuzzFraquency = 0.2f;

    private int scanlineNum = 100;
    private float horzFuzzDensity = 1.0f;

    bool valueChanged = true;

    [NonSerialized]
    Material crtMaterial;

    public override void Render(RenderTexture source, RenderTexture destination)
    {
        if (crtMaterial == null)
        {
            crtMaterial = new Material(crtShader);
            crtMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
        if (valueChanged)
            UpdateValuesToShader();

        // crtMaterial.SetFloat();
        Graphics.Blit(source, destination, crtMaterial, 0);
    }

    private void OnValidate()
    {
        valueChanged = true;
    }

    void UpdateValuesToShader()
    {
        // set_weight
        crtMaterial.SetFloat("whiteNoiseMultiplier", whiteNoiseMultiplier * 5f);
        crtMaterial.SetFloat("scanlineMultiplier", scanlineMultiplier * 10f);
        crtMaterial.SetFloat("horzFuzzMultiplier", horzFuzzMultiplier);

        // set_amp
        crtMaterial.SetFloat("rgbOffsetOptX", colorDrift);

        // set_freq
        crtMaterial.SetFloat("horzFuzzFraquency", horzFuzzFraquency * 2f);
        crtMaterial.SetFloat("scrollLikelihood", 1 - 2 * scrollLikelihood);

        // Default (unchangable)
        crtMaterial.SetFloat("scanlineNum", (float)scanlineNum);
        crtMaterial.SetFloat("horzFuzzDensity", horzFuzzDensity);

        valueChanged = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="commandType"></param>
    /// <param name="varName"></param>
    /// <param name="value"></param>
    /// <returns>
    /// 0: success
    /// -1: command invalid
    /// -2: value name invalid
    /// -3: value out of bound
    /// </returns>
    public int SetProperty(string commandType, string varName, float value)
    {
        if (value < 0 || value > 1.0f)
        {
            Debug.Log("Value out of bound! (0, 1)");
            return -3;
        }

        switch (commandType)
        {
            case "setweight":
                switch (varName)
                {
                    case "whitenoise":
                        whiteNoiseMultiplier = value;
                        break;
                    case "scanline":
                        scanlineMultiplier = value;
                        break;
                    case "fuzz":
                        horzFuzzMultiplier = value;
                        break;
                    default:
                        return -2;
                }
                break;

            case "setamp":
                switch (varName)
                {
                    case "coldrift":
                        colorDrift = value;
                        break;
                    default:
                        return -2;
                }
                break;

            case "setfreq":
                switch (varName)
                {
                    case "fuzz":
                        horzFuzzFraquency = value;
                        break;
                    case "scroll":
                        scrollLikelihood = value;
                        break;
                    default:
                        return -2;
                }
                break;
            default:
                return -1;
        }
        valueChanged = true;
        return 0;
    }
}
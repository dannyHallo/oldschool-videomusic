using Assets.WasapiAudio.Scripts.Unity;
using System.Collections;
using UnityEngine;

public class AudioSyncScale : AudioVisualizationEffect
{
    public float scale;
    public float power;
    [Range(0, 63)] public int inspectColumnsMin;
    [Range(0, 63)] public int inspectColumnsMax;

    private void Update()
    {
        float[] spectrumData = GetSpectrumData();
        float audioScale = GetAudioScale(spectrumData);
        transform.localScale = new Vector3(audioScale, audioScale, audioScale);

        // print(MaxIndex(spectrumData));
    }

    float GetAudioScale(float[] spectrumData)
    {
        float audioScale = 0;
        for (int j = inspectColumnsMin; j < inspectColumnsMax + 1; j++)
        {
            audioScale += spectrumData[j];
        }
        audioScale /= 1 + inspectColumnsMax - inspectColumnsMin;
        audioScale = Mathf.Pow(audioScale * scale, power);
        return audioScale;
    }

    float MaxValue(float[] array)
    {
        float max = array[0];
        int index = 0;
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i] > max)
            {
                max = array[i];
                index = i;
            }
        }
        return max;
    }

    int MaxIndex(float[] array)
    {
        float max = array[0];
        int index = 0;
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i] > max)
            {
                max = array[i];
                index = i;
            }
        }
        return index;
    }
}
using System;
using UnityEngine;

[Serializable]
public class LightLerper
{
    public float brightness = 0.0f;
    public float lerpSpeed = 3f;

    public void Lerp(bool set = false)
    {
        targetLight.intensity = Utils.FunkyLerp(targetLight.intensity, minBrightness, maxBrightness, brightness, lerpSpeed, set);
    }

    [Header("Config")]
    [SerializeField] private Light targetLight;
    [SerializeField] private float minBrightness = 0.0f;
    [SerializeField] private float maxBrightness = 0.0f;
}

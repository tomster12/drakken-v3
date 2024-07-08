using System;
using UnityEngine;

[Serializable]
public class OutlineLerper
{
    public float amount = 0.0f;
    public float lerpSpeed = 3f;

    public void Lerp(bool set = false)
    {
        Color targetColor = outlineColor;
        targetColor.a *= amount;
        if (!set) targetOutline.OutlineColor = Color.Lerp(targetOutline.OutlineColor, targetColor, Time.deltaTime * lerpSpeed);
        else targetOutline.OutlineColor = targetColor;
    }

    [Header("Config")]
    [SerializeField] private Outline targetOutline;
    [SerializeField] private Color outlineColor;
}

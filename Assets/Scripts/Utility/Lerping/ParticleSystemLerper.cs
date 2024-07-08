using System;
using UnityEngine;

[Serializable]
public class ParticleSystemLerper
{
    public float amount = 0.0f;
    public float lerpSpeed = 3f;

    public void Lerp(bool set = false)
    {
        ParticleSystem.MainModule mainModule = targetParticleSystem.main;
        mainModule.startLifetime = Utils.FunkyLerp(mainModule.startLifetime.constant, minLifetime, maxLifetime, amount, lerpSpeed, set);
        mainModule.startSize = Utils.FunkyLerp(mainModule.startSize.constant, minSize, maxSize, amount, lerpSpeed, set);
    }

    [Header("Config")]
    [SerializeField] private ParticleSystem targetParticleSystem;
    [SerializeField] private float minLifetime = 0.0f;
    [SerializeField] private float maxLifetime = 0.0f;
    [SerializeField] private float minSize = 1.0f;
    [SerializeField] private float maxSize = 1.0f;
}

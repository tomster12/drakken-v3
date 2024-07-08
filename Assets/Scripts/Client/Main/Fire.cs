using UnityEngine;

public class Fire : MonoBehaviour
{
    public float brightness = 0.0f;
    public float lerpSpeed = 3.0f;

    public void LerpValues(bool set = false)
    {
        foreach (LightLerper lerper in lightLerpers)
        {
            lerper.brightness = brightness;
            lerper.lerpSpeed = lerpSpeed;
            lerper.Lerp(set);
        }
        foreach (ParticleSystemLerper lerper in fireLerpers)
        {
            lerper.amount = brightness;
            lerper.lerpSpeed = lerpSpeed;
            lerper.Lerp(set);
        }
    }

    [Header("References")]
    [SerializeField] private LightLerper[] lightLerpers;
    [SerializeField] private ParticleSystemLerper[] fireLerpers;

    private void Update()
    {
        LerpValues();
    }

    [ContextMenu("Set Values")]
    private void SetValues() => LerpValues(true);
}

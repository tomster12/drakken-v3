
using UnityEngine;


public class Fire : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light light1;
    [SerializeField] private Light light2;
    [SerializeField] private Renderer lightRenderer;
    
    [Header("Config")]
    [SerializeField] private Color emissiveStart;
    [SerializeField] private Color emissiveEnd;
    [SerializeField] private float lerpSpeed = 3f;
    [SerializeField] private float l1IntensityStart = 50.0f;
    [SerializeField] private float l1IntensityEnd = 800.0f;
    [SerializeField] private float l2IntensityStart = 0.0f;
    [SerializeField] private float l2IntensityEnd = 800.0f;

    [Range(0.0f, 1.0f)]
    [SerializeField] public float brightness = 0.5f;



    private void Update()
    {
        // Lerp intensity based on brightness
        float targetIntensity1 = brightness * (l1IntensityEnd - l1IntensityStart) + l1IntensityStart;
        float targetIntensity2 = brightness * (l2IntensityEnd - l2IntensityStart) + l2IntensityStart;
        light1.intensity = Mathf.Lerp(light1.intensity, targetIntensity1, Time.deltaTime * lerpSpeed);
        light2.intensity = Mathf.Lerp(light2.intensity, targetIntensity2, Time.deltaTime * lerpSpeed);

        // Lerp color based on brightness
        Color targetColor = Color.Lerp(emissiveStart, emissiveEnd, brightness);
        lightRenderer.sharedMaterial.SetColor("_EmissionColor", Color.Lerp(lightRenderer.sharedMaterial.GetColor("_EmissionColor"), targetColor, Time.deltaTime * lerpSpeed));
    }


    public void SetValues()
    {
        // Lerp intensity based on brightness
        float targetIntensity1 = brightness * (l1IntensityEnd - l1IntensityStart) + l1IntensityStart;
        float targetIntensity2 = brightness * (l2IntensityEnd - l2IntensityStart) + l2IntensityStart;
        light1.intensity = targetIntensity1;
        light2.intensity = targetIntensity2;

        // Lerp color based on brightness
        Color targetColor = Color.Lerp(emissiveStart, emissiveEnd, brightness);
        lightRenderer.material.SetColor("_EmissionColor", targetColor);
    }
}

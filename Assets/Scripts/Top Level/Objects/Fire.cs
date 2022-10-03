
using UnityEngine;


public class Fire : MonoBehaviour
{
    // Declare variables
    [SerializeField] private Light fireLight;
    [SerializeField] private Renderer lightRenderer;
    [SerializeField] private Color emissiveStart;
    [SerializeField] private Color emissiveEnd;
    [SerializeField] private float lerpSpeed = 3f;
    private float brightness = 0.5f;


    private void Update()
    {
        // Lerp intensity based on brightness
        float targetIntensity = brightness * 650 + 50;
        fireLight.intensity = Mathf.Lerp(fireLight.intensity, targetIntensity, Time.deltaTime * lerpSpeed);

        // Lerp color based on brightness
        Color targetColor = Color.Lerp(emissiveStart, emissiveEnd, brightness);
        lightRenderer.material.SetColor("_EmissionColor", Color.Lerp(lightRenderer.material.GetColor("_EmissionColor"), targetColor, Time.deltaTime * lerpSpeed));
    }


    public void setBrightness(float brightness_) => brightness = brightness_;
}

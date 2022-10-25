
using UnityEngine;


public class Fire : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light light1;
    [SerializeField] private Light light2;
    [SerializeField] private ParticleSystem redFire;
    [SerializeField] private ParticleSystem orangeFire;

    [Header("Config")]
    [SerializeField] private float lerpSpeed = 3f;
    [SerializeField] private float l1IntensityStart = 50.0f;
    [SerializeField] private float l1IntensityEnd = 800.0f;
    [SerializeField] private float l2IntensityStart = 0.0f;
    [SerializeField] private float l2IntensityEnd = 800.0f;
    [SerializeField] private float redFireLifetimeStart = 0.0f;
    [SerializeField] private float redFireLifetimeEnd = 5.25f;
    [SerializeField] private float redFireSizeStart = 0.0f;
    [SerializeField] private float redFireSizeEnd = 1.0f;
    [SerializeField] private float orangeFireLifetimeStart = 0.0f;
    [SerializeField] private float orangeFireLifetimeEnd = 3.5f;
    [SerializeField] private float orangeFireSizeStart = 0.0f;
    [SerializeField] private float orangeFireSizeEnd = 0.65f;

    [Range(0.0f, 1.0f)]
    [SerializeField] public float brightness = 0.5f;



    private void Update()
    {
        // Lerp intensity based on brightness
        float targetIntensity1 = brightness * (l1IntensityEnd - l1IntensityStart) + l1IntensityStart;
        float targetIntensity2 = brightness * (l2IntensityEnd - l2IntensityStart) + l2IntensityStart;
        light1.intensity = Mathf.Lerp(light1.intensity, targetIntensity1, Time.deltaTime * lerpSpeed);
        light2.intensity = Mathf.Lerp(light2.intensity, targetIntensity2, Time.deltaTime * lerpSpeed);

        // Lerp fire based on brightness
        float targetRedLifetime = brightness * (redFireLifetimeEnd - redFireLifetimeStart) + redFireLifetimeStart;
        float targetRedSize = brightness * (redFireSizeEnd - redFireSizeStart) + redFireSizeStart;
        float targetOrangeLifetime = brightness * (orangeFireLifetimeEnd - orangeFireLifetimeStart) + orangeFireLifetimeStart;
        float targetOrangeSize = brightness * (orangeFireSizeEnd - orangeFireSizeStart) + orangeFireSizeStart;
        redFire.startLifetime = Mathf.Lerp(redFire.startLifetime, targetRedLifetime, Time.deltaTime * lerpSpeed);
        redFire.startSize = Mathf.Lerp(redFire.startSize, targetRedSize, Time.deltaTime * lerpSpeed);
        orangeFire.startLifetime = Mathf.Lerp(redFire.startLifetime, targetOrangeLifetime, Time.deltaTime * lerpSpeed);
        orangeFire.startSize = Mathf.Lerp(redFire.startSize, targetOrangeSize, Time.deltaTime * lerpSpeed);
    }


    public void SetValues()
    {
        // Lerp intensity based on brightness
        float targetIntensity1 = brightness * (l1IntensityEnd - l1IntensityStart) + l1IntensityStart;
        float targetIntensity2 = brightness * (l2IntensityEnd - l2IntensityStart) + l2IntensityStart;
        light1.intensity = targetIntensity1;
        light2.intensity = targetIntensity2;

        // Lerp fire based on brightness
        float targetRedLifetime = brightness * (redFireLifetimeEnd - redFireLifetimeStart) + redFireLifetimeStart;
        float targetRedSize = brightness * (redFireSizeEnd - redFireSizeStart) + redFireSizeStart;
        float targetOrangeLifetime = brightness * (orangeFireLifetimeEnd - orangeFireLifetimeStart) + orangeFireLifetimeStart;
        float targetOrangeSize = brightness * (orangeFireSizeEnd - orangeFireSizeStart) + orangeFireSizeStart;
        redFire.startLifetime = targetRedLifetime;
        redFire.startSize = targetRedSize;
        orangeFire.startLifetime = targetOrangeLifetime;
        orangeFire.startSize = targetOrangeSize;
    }
}


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


    private void Update() => LerpValues();


    [ContextMenu("Set Values")]
    public void SetValues() => LerpValues(true);

    public void LerpValues(bool set=false)
    {
        light1.intensity = FunkyLerp(light1.intensity, l1IntensityStart, l1IntensityEnd, brightness, lerpSpeed, set);
        light2.intensity = FunkyLerp(light2.intensity, l2IntensityStart, l2IntensityEnd, brightness, lerpSpeed, set);
        redFire.startLifetime = FunkyLerp(redFire.startLifetime, redFireLifetimeStart, redFireLifetimeEnd, brightness, lerpSpeed, set);
        redFire.startSize = FunkyLerp(redFire.startSize, redFireSizeStart, redFireSizeEnd, brightness, lerpSpeed, set);
        orangeFire.startLifetime = FunkyLerp(orangeFire.startLifetime, orangeFireLifetimeStart, orangeFireLifetimeEnd, brightness, lerpSpeed, set);
        orangeFire.startSize = FunkyLerp(orangeFire.startSize, orangeFireSizeStart, orangeFireSizeEnd, brightness, lerpSpeed, set);
    }


    private float FunkyLerp(float x, float a, float b, float t, float v, bool set = false)
    {
        float nx = t * (b - a) + a;
        if (t == 0) nx = 0;
        if (set) return nx;
        return Mathf.Lerp(x, nx, Time.deltaTime * v);
    }
}

using UnityEngine;

public class Fire : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    [SerializeField] public float brightness = 0.5f;

    public void LerpValues(bool set = false)
    {
        light1.intensity = Utils.FunkyLerp(light1.intensity, l1IntensityStart, l1IntensityEnd, brightness, lerpSpeed, set);
        light2.intensity = Utils.FunkyLerp(light2.intensity, l2IntensityStart, l2IntensityEnd, brightness, lerpSpeed, set);
        redFireModule.startLifetime = Utils.FunkyLerp(redFireModule.startLifetime.constant, redFireLifetimeStart, redFireLifetimeEnd, brightness, lerpSpeed, set);
        redFireModule.startSize = Utils.FunkyLerp(redFireModule.startSize.constant, redFireSizeStart, redFireSizeEnd, brightness, lerpSpeed, set);
        orangeFireModule.startLifetime = Utils.FunkyLerp(orangeFireModule.startLifetime.constant, orangeFireLifetimeStart, orangeFireLifetimeEnd, brightness, lerpSpeed, set);
        orangeFireModule.startSize = Utils.FunkyLerp(orangeFireModule.startSize.constant, orangeFireSizeStart, orangeFireSizeEnd, brightness, lerpSpeed, set);
    }

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

    private ParticleSystem.MainModule redFireModule;
    private ParticleSystem.MainModule orangeFireModule;

    private void Awake()
    {
        redFireModule = redFire.main;
        orangeFireModule = orangeFire.main;
    }

    private void Update()
    {
        LerpValues();
    }
}

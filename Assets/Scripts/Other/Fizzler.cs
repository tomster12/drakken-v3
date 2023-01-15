
using UnityEngine;


public class Fizzler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer mainRenderer;

    [Header("Config")]
    [SerializeField] private float fizzleDuration = 2.5f;

    private float fizzleStartTime = 0.0f;

    public bool isFizzling { get; private set; } = false;
    public bool hasFizzled { get; private set; } = false;
    public float fizzlePct { get; private set; } = 0.0f;


    private void Awake()
    {
        // Instantiate seperate materials
        for (int i = 0; i < mainRenderer.materials.Length; i++)
        {
            mainRenderer.materials[i] = Instantiate(mainRenderer.materials[i]);
        }
    }


    public void CallUpdate()
    {
        if (!isFizzling) return;

        // Update variables
        fizzlePct = Mathf.Min((Time.time - fizzleStartTime) / fizzleDuration, 1.0f);
        foreach (Material mat in mainRenderer.materials) mat.SetFloat("_Percent", fizzlePct);
        if (fizzlePct >= 1.0f)
        {
            hasFizzled = true;
            isFizzling = false;
        }
    }

    public void StartFizzle()
    {
        if (isFizzling || hasFizzled) return;

        // Start fizzle
        fizzleStartTime = Time.time;
        isFizzling = true;
        hasFizzled = false;
        fizzlePct = 0.0f;
        CallUpdate();
    }

    public void Reset()
    {
        if (!isFizzling && !hasFizzled) return;

        // Reset fizzle variables
        isFizzling = false;
        hasFizzled = false;
        fizzlePct = 0.0f;
        foreach (Material mat in mainRenderer.materials) mat.SetFloat("_Percent", 0.0f);
    }
}

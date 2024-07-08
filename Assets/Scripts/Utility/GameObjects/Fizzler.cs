using UnityEngine;

public class Fizzler : MonoBehaviour
{
    public bool isFizzling { get; private set; } = false;
    public bool hasFizzled { get; private set; } = false;
    public float fizzlePct { get; private set; } = 0.0f;

    public void CallUpdate()
    {
        if (!isFizzling) return;

        fizzlePct = Mathf.Min((Time.time - fizzleStartTime) / fizzleDuration, 1.0f);
        foreach (Material mat in mainRenderer.materials) mat.SetFloat("_Percent", fizzlePct);

        if (fizzlePct >= 1.0f)
        {
            hasFizzled = true;
            isFizzling = false;
        }
    }

    [ContextMenu("Start Fizzle")]
    public void StartFizzle()
    {
        if (isFizzling || hasFizzled) return;

        // Start fizzle
        isFizzling = true;
        hasFizzled = false;
        fizzlePct = 0.0f;
        fizzleStartTime = Time.time;
        CallUpdate();
    }

    [ContextMenu("Reset")]
    public void Reset()
    {
        if (!isFizzling && !hasFizzled) return;

        // Reset fizzle variables
        isFizzling = false;
        hasFizzled = false;
        fizzlePct = 0.0f;
        foreach (Material mat in mainRenderer.materials) mat.SetFloat("_Percent", 0.0f);
    }

    [Header("References")]
    [SerializeField] private Renderer mainRenderer;

    [Header("Config")]
    [SerializeField] private float fizzleDuration = 2.5f;
    [SerializeField] private bool toUpdate;

    private float fizzleStartTime = 0.0f;

    private void Awake()
    {
        // Instantiate seperate materials
        for (int i = 0; i < mainRenderer.materials.Length; i++)
        {
            mainRenderer.materials[i] = Instantiate(mainRenderer.materials[i]);
        }
    }

    private void Update()
    {
        if (toUpdate) CallUpdate();
    }
}

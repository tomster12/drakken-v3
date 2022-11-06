
using UnityEngine;


public class VisualToken : MonoBehaviour
{
    // Declare variables
    [Header("References")]
    [SerializeField] private HoverChild hoverChecker;
    [SerializeField] private Renderer mainRenderer;
    [SerializeField] private GameObject emissiveObject;
    [SerializeField] public Lerper _lerper;
    [SerializeField] private ClassData _optionClass;
    public ClassData optionClass => _optionClass;
    public Lerper lerper => _lerper;

    [Header("Config")]
    [SerializeField] private float fizzleDuration = 2.5f;

    private float fizzleStartTime = 0.0f;

    public bool isActive { get; private set; } = false;
    public bool isHovered { get; private set; } = false;
    public bool isFizzling { get; private set; } = false;
    public float fizzlePct { get; private set; } = 0.0f;

    public bool toGlow = false;


    private void Awake()
    {
        // Seperate materials for each visual token
        for (int i = 0; i < mainRenderer.materials.Length; i++)
        {
            mainRenderer.materials[i] = Instantiate(mainRenderer.materials[i]);
        }
    }


    public void Update()
    {
        if (!isActive) return;

        // Update variables
        isHovered = hoverChecker.GetHovered();
        emissiveObject.SetActive(toGlow);
        lerper.CallUpdate();
        UpdateFizzling();
    }

    private void UpdateFizzling()
    {
        if (!isFizzling) return;

        // Update variables
        fizzlePct = Mathf.Min((Time.time - fizzleStartTime) / fizzleDuration,  1.0f);
        foreach (Material mat in mainRenderer.materials)
        {
            mat.SetFloat("_Percent", fizzlePct);
        }
        if (fizzlePct == 1.0f)
        {
            isFizzling = false;
            SetActive(false);
        }
    }


    public void SetActive(bool isActive_)
    {
        if (isActive == isActive_ || isFizzling) return;

        // Update variables
        isActive = isActive_;
        gameObject.SetActive(isActive);
        Update();
    }


    public void StartFizzle()
    {
        // Start fizzle
        fizzleStartTime = Time.time;
        isFizzling = true;
        fizzlePct = 0.0f;
    }
}

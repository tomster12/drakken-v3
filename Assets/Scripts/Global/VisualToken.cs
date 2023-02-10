
using UnityEngine;


public class VisualToken : MonoBehaviour
{
    // Declare variables
    [Header("References")]
    [SerializeField] private HoverChild hoverChecker;
    [SerializeField] private Renderer mainRenderer;
    [SerializeField] private Material emissiveOnMaterial;
    [SerializeField] public Lerper _lerper;
    [SerializeField] public Fizzler _fizzler;
    [SerializeField] private ClassData _optionClass;
    public ClassData optionClass => _optionClass;
    public Lerper lerper => _lerper;
    public Fizzler fizzler => _fizzler;

    private Material emissiveOffMaterial;
    public bool isActive { get; private set; } = false;
    public bool isHovered { get; private set; } = false;
    public bool toGlow = false;

    private void Awake()
    {
        // Instantiate seperate materials
        emissiveOnMaterial = Instantiate(emissiveOnMaterial);
        emissiveOffMaterial = Instantiate(mainRenderer.materials[0]);
    }


    public void Update()
    {
        if (!isActive) return;

        // Update materials
        Material[] materials = mainRenderer.materials;
        if (toGlow) materials[0] = emissiveOnMaterial;
        else materials[0] = emissiveOffMaterial;
        mainRenderer.materials = materials;

        // Update variables
        isHovered = hoverChecker.GetHovered();
        fizzler.CallUpdate();
        lerper.Lerp();
    }


    public void SetActive(bool isActive_)
    {
        if (isActive == isActive_) return;

        // Update variables
        isActive = isActive_;
        fizzler.Reset();
        gameObject.SetActive(isActive);
        mainRenderer.gameObject.SetActive(isActive);
        if (isActive) Update();
        if (!isActive) hoverChecker.SetOverride(false);
    }
}

using UnityEngine;

public class VisualToken : MonoBehaviour
{
    [SerializeField] public PointLerper lerper;
    [SerializeField] public Fizzler fizzler;

    public PointLerper Lerper => lerper;
    public Fizzler Fizzler => fizzler;
    public ClassData OptionClass { get; private set; }
    public bool IsActive { get; private set; } = false;
    public bool IsHovered { get; private set; } = false;
    public bool IsGlowing { get; private set; } = false;

    public void SetActive(bool isActive_)
    {
        if (IsActive == isActive_) return;

        // Update variables
        IsActive = isActive_;
        Fizzler.Reset();
        gameObject.SetActive(IsActive);
        mainRenderer.gameObject.SetActive(IsActive);
        if (IsActive) Update();
        if (!IsActive) hoverChecker.SetOverride(false);
    }

    public void SetGlowing(bool isGlowing)
    {
        IsGlowing = isGlowing;
        UpdateMaterials();
    }

    [Header("References")]
    [SerializeField] private HoverChild hoverChecker;
    [SerializeField] private Renderer mainRenderer;
    [SerializeField] private Material emissiveOnMaterial;

    private Material emissiveOffMaterial;

    private void Update()
    {
        if (!IsActive) return;

        IsHovered = hoverChecker.GetHovered();
        Fizzler.CallUpdate();
        Lerper.Lerp();
    }

    private void Awake()
    {
        emissiveOnMaterial = Instantiate(emissiveOnMaterial);
        emissiveOffMaterial = Instantiate(mainRenderer.materials[0]);
    }

    private void UpdateMaterials()
    {
        Material[] materials = mainRenderer.materials;
        if (IsGlowing) materials[0] = emissiveOnMaterial;
        else materials[0] = emissiveOffMaterial;
        mainRenderer.materials = materials;
    }
}

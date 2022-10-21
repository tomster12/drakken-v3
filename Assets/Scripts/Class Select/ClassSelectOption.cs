
using UnityEngine;


public class ClassSelectOption : MonoBehaviour
{
    // Declare variables
    [Header("References")]
    [SerializeField] private Collider hoverCollider;
    [SerializeField] private HoverChild hoverChecker;
    [SerializeField] private ClassData _optionClass;
    public ClassData optionClass => _optionClass;
    [SerializeField] private GameObject emissiveObject;

    [Header("Config")]
    [SerializeField] private float selectedOffset = 0.6f;
    [SerializeField] private float playOffset = 1f;
    [SerializeField] private float hoverOffsetLerp = 4.5f;

    private bool isActive;
    private bool isHovered;
    private bool isClickable;
    private bool isSelected;
    private Vector3 hoverOffset;
    public Vector3 targetPosition;


    private void Update()
    {
        if (!isActive) return;

        // Update variables
        isHovered = hoverChecker.GetHovered();
        emissiveObject.SetActive(isHovered || isSelected);

        // Update position
        Vector3 targetOffset = Vector3.zero;
        if (isClickable && isHovered && isSelected) targetOffset = Vector3.up * playOffset;
        else if (isClickable && (isHovered || isSelected)) targetOffset = Vector3.up * selectedOffset;
        hoverOffset = Vector3.Lerp(hoverOffset, targetOffset, Time.deltaTime * hoverOffsetLerp);
        transform.position = targetPosition + hoverOffset;
    }


    public void SetActive(bool isActive_)
    {
        if (isActive == isActive_) return;

        // Update variables
        isActive = isActive_;
        gameObject.SetActive(isActive);
        isClickable = false;
        isSelected = false;
        hoverOffset = Vector3.zero;
    }


    public void SetSelected(bool isSelected_) { isSelected = isSelected_; isClickable = false; }
    public void SetClickable(bool isClickable_)  { isClickable = isClickable_; hoverCollider.enabled = isClickable; }
    public bool GetClicked() => (hoverChecker.GetHovered() && Input.GetMouseButtonDown(0) && isClickable);
}

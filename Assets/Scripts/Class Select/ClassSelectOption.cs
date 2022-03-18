
using UnityEngine;


public class ClassSelectOption : MonoBehaviour
{
    // Declare variables
    [Header("References")]
    [SerializeField] private Collider hoverCollider;
    [SerializeField] private HoverChild hoverChecker;
    [SerializeField] private ClassData optionClass;
    [SerializeField] private GameObject emissiveObject;

    [Header("Config")]
    [SerializeField] private float selectedOffset = 0.6f;
    [SerializeField] private float playOffset = 1f;
    [SerializeField] private float hoverOffsetLerp = 4.5f;

    private bool isActive;
    private bool isClickable;
    private bool isSelected;
    private Vector3 targetPosition;
    private Vector3 hoverOffset;


    private void Update()
    {
        // Run updates
        UpdatePosition();
        emissiveObject.SetActive(GetHovered() && isSelected);
    }


    private void UpdatePosition()
    {
        // Update position
        Vector3 targetOffset = Vector3.zero;
        if (GetHovered() && isSelected) targetOffset = Vector3.up * playOffset;
        else if ((GetHovered() || isSelected) && isClickable) targetOffset = Vector3.up * selectedOffset;
        hoverOffset = Vector3.Lerp(hoverOffset, targetOffset, Time.deltaTime * hoverOffsetLerp);
        transform.position = targetPosition + hoverOffset;
    }


    public void SetTargetPosition(Vector3 targetPosition_)
    {
        // Set target position
        targetPosition = targetPosition_;
        UpdatePosition();
    }

    public void SetActive(bool isActive_)
    {
        // Reset variables
        if (isActive && !isActive_)
        {
            isActive = false;
            isClickable = false;
            isSelected = false;
            hoverOffset = Vector3.zero;
            gameObject.SetActive(false);
            hoverChecker.SetOverride(false);
        }

        else if (!isActive && isActive_)
        {
            isClickable = false;
            isSelected = false;
            hoverOffset = Vector3.zero;
            gameObject.SetActive(true);
        }

        // Update isActive
        isActive = isActive_;
    }


    public void SetSelected(bool isSelected_) { isSelected = isSelected_; isClickable = false; }
    public void SetClickable(bool isClickable_)  { isClickable = isClickable_; hoverCollider.enabled = isClickable; }
    public bool GetClicked() => (hoverChecker.GetHovered() && Input.GetMouseButtonDown(0) && isClickable);
    public bool GetHovered() => isClickable && hoverChecker.GetHovered();
    public ClassData GetClass() => optionClass;
}

using UnityEngine;

public class PhysicalButton : MonoBehaviour
{
    protected virtual void Click()
    { }

    [Header("Config")]
    [SerializeField] private float hoverY = 0.4f;
    [SerializeField] private float heldY = -0.15f;
    [SerializeField] private float normLerp = 12f;
    [SerializeField] private float heldLerp = 40f;

    private float initialY;
    private bool isHovered;
    private bool isHeld;

    private void Awake()
    {
        // Initialize variables
        initialY = transform.position.y;
        isHovered = false;
        isHeld = false;
    }

    private void Update()
    {
        // Update isHeld
        if (Input.GetMouseButtonDown(0) && isHovered) isHeld = true;
        if (!Input.GetMouseButton(0))
        {
            if (isHeld && isHovered)
            {
                Click();
                isHovered = false;
            }
            isHeld = false;
        }

        // Lerp based on current state
        Vector3 currentPosition = transform.position;
        if (isHeld) currentPosition.y = Mathf.Lerp(transform.position.y, heldY, heldLerp * Time.deltaTime);
        else if (isHovered) currentPosition.y = Mathf.Lerp(transform.position.y, hoverY, normLerp * Time.deltaTime);
        else currentPosition.y = Mathf.Lerp(transform.position.y, initialY, normLerp * Time.deltaTime);
        transform.position = currentPosition;
    }

    // Handle hovering
    private void OnMouseOver() => isHovered = true;

    private void OnMouseExit() => isHovered = false;
}

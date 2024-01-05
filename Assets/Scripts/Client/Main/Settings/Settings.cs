using UnityEngine;

public class Settings : MonoBehaviour
{
    public enum State
    { STOPPED, SETUP, SELECTING, SELECTED, FINISHED };

    public bool isActive { get; private set; } = false;
    public bool hasExited { get; private set; } = false;

    public void SetActive(bool isActive_)
    {
        if (isActive == isActive_) return;

        // Update variables
        isActive = isActive_;
        hasExited = false;
        transform.position = follow.position;
        transform.rotation = follow.rotation;
        exitToken.transform.position = start.position;
        exitToken.transform.rotation = target.rotation;
        if (gameObject.activeSelf != isActive_) gameObject.SetActive(isActive);
        exitToken.SetActive(isActive);
        exitToken.Fizzler.Reset();
    }

    [Header("References")]
    [SerializeField] private VisualToken exitToken;
    [SerializeField] private Transform follow;
    [SerializeField] private Transform start;
    [SerializeField] private Transform target;

    [Header("Config")]
    [SerializeField] private float movementLerpSpeed = 5.5f;
    [SerializeField] private float rotationLerpSpeed = 3.0f;
    [SerializeField] private float hoverOffset = 1.0f;
    [SerializeField] private float clickOffset = 2.2f;

    private void Awake()
    {
        // Set token lerper and target values
        exitToken.Lerper.SetTarget(target.position, target.rotation);
        exitToken.Lerper.positionlerpSpeed = movementLerpSpeed;
        exitToken.Lerper.rotationLerpSpeed = rotationLerpSpeed;
    }

    private void Update()
    {
        if (!isActive) return;
        transform.position = follow.position;

        // Handle fizzling
        hasExited = exitToken.Fizzler.hasFizzled;
        if (exitToken.Fizzler.isFizzling)
        {
            exitToken.Lerper.SetTargetPosition(target.position + Vector3.up * clickOffset);
            exitToken.SetGlowing(true);
        }

        // Handle hovering
        else if (exitToken.IsHovered)
        {
            exitToken.Lerper.SetTargetPosition(target.position + Vector3.up * hoverOffset);
            exitToken.SetGlowing(true);
            if (Input.GetMouseButtonDown(0)) exitToken.Fizzler.StartFizzle();
        }

        // Reset otherwise
        else
        {
            exitToken.Lerper.SetTargetPosition(target.position);
            exitToken.SetGlowing(false);
        }
    }
}

using UnityEngine;

public class GameToken : MonoBehaviour
{
    public void LoadToken(TokenData tokenData_)
    {
        // TODO:
        //  - Probably take in isOpponent
        //  - Update generator with isOpponent
        //  - Also take in GameBoard reference
        //  - Potentially flip it, so that token calls function on
        //      GameBoard instance to find position rather than calls
        //      of SetTargetPosition()

        // Initialize variables
        tokenData = tokenData_;
        generator.SetToken(tokenData_);
    }

    public void SetTargetPosition(Vector3 targetPosition_)
    {
        // Set target position
        targetPosition = targetPosition_;
    }

    public void SetOpponent(bool isOpponent_) => isOpponent = isOpponent_;

    public void SetSelected(bool isSelected_) => isSelected = isSelected_;

    public bool GetClicked() => Input.GetMouseButtonDown(0) && GetHovered();

    public bool GetHovered() => hoverChild.GetHovered();

    public TokenData GetTokenData() => tokenData;

    [Header("References")]
    [SerializeField] private TokenGenerator generator;
    [SerializeField] private HoverChild hoverChild;

    [Header("Config")]
    [SerializeField] private float hoverOffsetAmount = 0.45f;
    [SerializeField] private float selectedOffsetAmount = 0.75f;
    [SerializeField] private float hoverOffsetLerp = 6f;

    private bool isOpponent;
    private TokenData tokenData;
    private Vector3 targetPosition;
    private Vector3 hoverOffset;
    private bool isSelected;

    private void Update()
    {
        // Update position
        Vector3 targetOffset = Vector3.zero;
        if (isSelected) targetOffset = Vector3.up * selectedOffsetAmount;
        else if (GetHovered()) targetOffset = Vector3.up * hoverOffsetAmount;
        hoverOffset = Vector3.Lerp(hoverOffset, targetOffset, Time.deltaTime * hoverOffsetLerp);
        transform.position = targetPosition + hoverOffset;
    }
}

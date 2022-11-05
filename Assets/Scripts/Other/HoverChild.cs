
using UnityEngine;


public class HoverChild : MonoBehaviour
{
    // Declare variables
    [SerializeField] private bool isHovered = false;

    public bool GetHovered() => isHovered;

    public void SetOverride(bool isHovered_) => isHovered = isHovered_;

    private void OnMouseOver() => isHovered = true;
    private void OnMouseExit() => isHovered = false;
}

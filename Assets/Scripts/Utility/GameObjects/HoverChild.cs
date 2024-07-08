using UnityEngine;

public class HoverChild : MonoBehaviour
{
    public bool GetHovered() => isHovered;

    public void SetOverride(bool isHovered_) => isHovered = isHovered_;

    [SerializeField] private bool isHovered = false;

    private void OnMouseOver() => isHovered = true;

    private void OnMouseExit() => isHovered = false;
}

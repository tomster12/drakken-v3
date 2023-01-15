
using System;
using UnityEngine;


public class Settings : MonoBehaviour
{
    public enum State { STOPPED, SETUP, SELECTING, SELECTED, FINISHED };

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

    public bool isActive { get; private set; } = false;
    public bool hasExited { get; private set; } = false;


    private void Awake()
    {
        // Set token lerper and target values
        exitToken.lerper.SetTarget(target.position, target.rotation);
        exitToken.lerper.positionlerpSpeed = movementLerpSpeed;
        exitToken.lerper.rotationLerpSpeed = rotationLerpSpeed;
    }


    private void Update()
    {
        if (!isActive) return;
        transform.position = follow.position;

        // Handle fizzling
        hasExited = exitToken.fizzler.hasFizzled;
        if (exitToken.fizzler.isFizzling)
        {
            exitToken.lerper.SetTargetPosition(target.position + Vector3.up * clickOffset);
            exitToken.toGlow = true;
        }

        // Handle hovering
        else if (exitToken.isHovered)
        {
            exitToken.lerper.SetTargetPosition(target.position + Vector3.up * hoverOffset);
            exitToken.toGlow = true;
            if (Input.GetMouseButtonDown(0)) exitToken.fizzler.StartFizzle();
        }

        // Reset otherwise
        else
        {
            exitToken.lerper.SetTargetPosition(target.position);
            exitToken.toGlow = false;
        }
    }

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
        exitToken.fizzler.Reset();
    }
}

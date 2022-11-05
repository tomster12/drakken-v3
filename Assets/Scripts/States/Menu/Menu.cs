
using System;
using UnityEngine;


public class Menu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VisualToken[] options;
    [SerializeField] private Transform[] transforms;
    [SerializeField] private Transform startTransform;

    [Header("Config")]
    [SerializeField] private float setupIntervalDuration = 1.0f;
    [SerializeField] private float setupOutwardDuration = 0.5f;
    [SerializeField] private float setupRotationLerpSpeed = 4.5f;
    [SerializeField] private float selectingPositionLerpSpeed = 10f;
    [SerializeField] private float selectingRotationLerpSpeed = 10f;
    [SerializeField] private float selectedPositionLerpSpeed = 1.5f;
    [SerializeField] private float optionHoverDuration = 3f;
    [SerializeField] private float optionHoverMagnitude = 0.4f;
    [SerializeField] private float optionHoverOffset = 0.2f;
    [SerializeField] private float optionHoverHeight = 1.4f;
    [SerializeField] private float optionSelectedHeight = 2.5f;
    private float timeStart;
    private float setupStart;

    public int hoveredIndex { get; private set; } = -1;
    public int selectedIndex { get; private set; } = -1;
    public bool isActive { get; private set; } = false;
    public bool isSetup { get; private set; } = false;
    public bool isFizzling { get; private set; } = false;

    public Action<int> onSelect;
    public Action onFizzled;


    private void Reset()
    {
        // Update all variables
        timeStart = Time.time;
        setupStart = Time.time;
        hoveredIndex = -1;
        selectedIndex = -1;
        isSetup = false;
        isFizzling = false;
        StartSetup();
    }

    private void StartSetup()
    {
        // Update variables
        for (int i = 0; i < options.Length; i++)
        {
            options[i].transform.position = startTransform.position;
            options[i].transform.rotation = startTransform.rotation;
            options[i].lerper.toLerpPosition = false;
            options[i].lerper.targetRotation = transforms[i].rotation;
            options[i].lerper.toLerpRotation = true;
            options[i].lerper.rotationLerpSpeed = setupRotationLerpSpeed;
            options[i].toGlow = false;
            options[i].SetActive(false);
        }
    }

    private void StartSelecting()
    {
        // Update variables
        setupStart = Time.time;
        isSetup = true;
        for (int i = 0; i < options.Length; i++)
        {
            options[i].lerper.toLerpPosition = true;
            options[i].lerper.rotationLerpSpeed = selectingPositionLerpSpeed;
            options[i].lerper.positionlerpSpeed = selectingRotationLerpSpeed;
        }
    }


    private void Update()
    {
        if (!isActive) return;
        UpdateSetup();
        UpdateSelecting();
        UpdateFizzling();
    }

    private void UpdateSetup()
    {
        if (isSetup) return;
        float timeDiff = Time.time - timeStart;

        // Update all options
        for (int i = 0; i < options.Length; i++)
        {
            // Once started
            float startTime = i * setupIntervalDuration;
            if (timeDiff >= startTime)
            {
                // Update positions
                float outwardPct = Mathf.Min(Mathf.Max((timeDiff - startTime) / setupOutwardDuration, 0.0f), 1f);
                outwardPct = Easing.easeOutExpo(outwardPct);
                options[i].lerper.targetPosition = Vector3.Lerp(startTransform.position, transforms[i].position, outwardPct);
                options[i].SetActive(true);
            }
        }

        if (timeDiff > (options.Length - 1) * setupIntervalDuration + setupOutwardDuration) StartSelecting();
    }

    private void UpdateSelecting()
    {
        if (!isSetup) return;
        float timeDiff = Time.time - setupStart;

        // Update all options
        hoveredIndex = -1;
        for (int i = 0; i < options.Length; i++)
        {
            // Update positions
            options[i].lerper.targetPosition = transforms[i].position;
            options[i].lerper.targetRotation = transforms[i].rotation;

            // Handle hovering
            if (options[i].isHovered)
            {
                hoveredIndex = i;
                options[i].lerper.targetPosition += Vector3.up * optionHoverHeight;
                options[i].toGlow = true;
                if (Input.GetMouseButtonDown(0)) Select(i);
            }
            else
            {
                options[i].lerper.targetPosition += new Vector3(0.0f, Mathf.Sin((timeDiff / optionHoverDuration - i * optionHoverOffset) * Mathf.PI * 2f) * optionHoverMagnitude, 0.0f);
                options[i].toGlow = false;
            }
        }
    }

    private void UpdateFizzling()
    {
        if (!isSetup || !isFizzling) return;

        // Update all options
        bool allFizzled = true;
        for (int i = 0; i < options.Length; i++)
        {
            // Update positions
            options[i].lerper.targetPosition = transforms[i].position;
            options[i].lerper.targetRotation = transforms[i].rotation;
            options[i].lerper.positionlerpSpeed = selectedPositionLerpSpeed;
            if (selectedIndex == i) options[i].lerper.targetPosition += Vector3.up * (optionHoverHeight + optionSelectedHeight);
            else options[i].lerper.targetPosition += Vector3.up * (optionHoverHeight - optionSelectedHeight);

            // Check whether fizzled
            allFizzled &= options[i].fizzlePct == 1.0f;
        }

        // Handle finished fizzling
        if (allFizzled)
        {
            isFizzling = false;
            SetActive(false);
            if (onFizzled != null) onFizzled();
        }
    }


    public void SetActive(bool isActive_)
    {
        if (isActive == isActive_) return;
        if (isFizzling) return;

        // Update variables and reset
        isActive = isActive_;
        for (int i = 0; i < options.Length; i++) options[i].SetActive(false);
        if (gameObject.activeSelf != isActive_) gameObject.SetActive(isActive);
        if (isActive) Reset();
    }

    private void Select(int index)
    {
        // Update variables
        isFizzling = true;
        selectedIndex = index;
        foreach (VisualToken option in options) option.StartFizzle();
        if (onSelect != null) onSelect(index);
    }
}

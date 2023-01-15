
using System;
using UnityEngine;


public class Menu : MonoBehaviour
{
    public enum State { STOPPED, SETUP, SELECTING, SELECTED, FINISHED };

    [Header("References")]
    [SerializeField] private VisualToken[] options;
    [SerializeField] private Transform[] transforms;
    [SerializeField] private Transform startTransform;

    [Header("Config")]
    [SerializeField] private float setupIntervalDuration = 1.0f;
    [SerializeField] private float setupOutwardDuration = 0.5f;
    [SerializeField] private float setupPositionLerpSpeed = 10f;
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

    public int hoveredIndex { get; private set; } = -1;
    public int selectedIndex { get; private set; } = -1;
    public bool isActive { get; private set; } = false;
    public State state { get; private set; } = State.STOPPED;

    public Action<int> onClick;
    public Action<int> onSelect;


    private void Reset()
    {
        // Update all variables
        timeStart = Time.time;
        hoveredIndex = -1;
        selectedIndex = -1;
        SetStateSetup();
    }


    private void SetStateSetup()
    {
        // Update tokens
        for (int i = 0; i < options.Length; i++)
        {
            transform.position = startTransform.position;
            transform.rotation = startTransform.rotation;
            options[i].transform.position = startTransform.position;
            options[i].transform.rotation = startTransform.rotation;
            options[i].lerper.positionlerpSpeed = setupPositionLerpSpeed;
            options[i].lerper.rotationLerpSpeed = setupRotationLerpSpeed;
            options[i].lerper.SetTargetRotation(transforms[i].rotation);
            options[i].toGlow = false;
            options[i].SetActive(false);
        }

        // Update state
        state = State.SETUP;
    }

    private void SetStateSelecting()
    {
        // Update variables
        for (int i = 0; i < options.Length; i++)
        {
            options[i].lerper.positionlerpSpeed = selectingRotationLerpSpeed;
            options[i].lerper.rotationLerpSpeed = selectingPositionLerpSpeed;
        }

        // Update state
        state = State.SELECTING;
    }

    private void SetStateSelected()
    {
        // Start each option fizzling
        foreach (VisualToken option in options) option.fizzler.StartFizzle();

        // Update state
        state = State.SELECTED;
    }

    private void SetStateFinished()
    {
        state = State.FINISHED;
        SetActive(false);
        if (onSelect != null) onSelect(selectedIndex);
    }


    private void Update()
    {
        if (!isActive) return;
        UpdateSetup();
        UpdateSelecting();
        UpdateSelected();
    }

    private void UpdateSetup()
    {
        if (state != State.SETUP) return;
        float timeDiff = Time.time - timeStart;
        transform.position = startTransform.position;
        transform.rotation = startTransform.rotation;

        // Update all options
        for (int i = 0; i < options.Length; i++)
        {
            // Once started
            float startTime = i * setupIntervalDuration;
            if (timeDiff >= startTime)
            {
                Vector3 target = transforms[i].position;

                // Handle hovering
                if (options[i].isHovered)
                {
                    hoveredIndex = i;
                    target += Vector3.up * optionHoverHeight;
                    options[i].toGlow = true;
                    if (Input.GetMouseButtonDown(0)) Select(i);
                }

                // Otherwise oscillate
                else
                {
                    target += new Vector3(0.0f, Mathf.Sin((timeDiff / optionHoverDuration - i * optionHoverOffset) * Mathf.PI * 2f) * optionHoverMagnitude, 0.0f);
                    options[i].toGlow = false;
                }

                // Update positions
                float outwardPct = Mathf.Min(Mathf.Max((timeDiff - startTime) / setupOutwardDuration, 0.0f), 1f);
                outwardPct = Easing.easeOutExpo(outwardPct);
                options[i].lerper.SetTargetPosition(Vector3.Lerp(startTransform.position, target, outwardPct));
                options[i].SetActive(true);
            }
        }

        // Detect when finished
        if (timeDiff > (options.Length - 1) * setupIntervalDuration + setupOutwardDuration * 0.9f) SetStateSelecting();
    }

    private void UpdateSelecting()
    {
        if (state != State.SELECTING) return;
        float timeDiff = Time.time - timeStart;
        transform.position = startTransform.position;
        transform.rotation = startTransform.rotation;

        // Update all options
        hoveredIndex = -1;
        for (int i = 0; i < options.Length; i++)
        {
            Vector3 targetPosition = transforms[i].position;

            // Hovered so move up
            if (options[i].isHovered)
            {
                hoveredIndex = i;
                targetPosition += Vector3.up * optionHoverHeight;
                options[i].toGlow = true;
                if (Input.GetMouseButtonDown(0)) Select(i);
            }

            // Otherwise oscillate
            else
            {
                targetPosition += new Vector3(0.0f, Mathf.Sin((timeDiff / optionHoverDuration - i * optionHoverOffset) * Mathf.PI * 2f) * optionHoverMagnitude, 0.0f);
                options[i].toGlow = false;
            }

            // Set targets
            options[i].lerper.SetTargetPosition(targetPosition);
            options[i].lerper.SetTargetRotation(transforms[i].rotation);
        }
    }

    private void UpdateSelected()
    {
        if (state != State.SELECTED) return;

        // Update all options
        bool allFizzled = true;
        for (int i = 0; i < options.Length; i++)
        {
            // Update positions
            Vector3 targetPosition = transforms[i].position;
            Quaternion targetRotation = transforms[i].rotation;
            options[i].toGlow = selectedIndex == i;
            if (selectedIndex == i) targetPosition += Vector3.up * (optionHoverHeight + optionSelectedHeight);
            else targetPosition += Vector3.up * (optionHoverHeight - optionSelectedHeight);
            options[i].lerper.SetTargetPosition(targetPosition);
            options[i].lerper.SetTargetRotation(targetRotation);

            // Check whether fizzled
            allFizzled &= !options[i].isActive || (options[i].fizzler.fizzlePct == 1.0f);
        }

        // Handle finished fizzling
        if (allFizzled) SetStateFinished();
    }


    public void SetActive(bool isActive_)
    {
        if (isActive == isActive_) return;
        if (state == State.SELECTED) return;

        // Update variables and reset
        isActive = isActive_;
        if (gameObject.activeSelf != isActive_) gameObject.SetActive(isActive);
        for (int i = 0; i < options.Length; i++) options[i].SetActive(false);
        if (isActive) Reset();
    }

    private void Select(int index)
    {
        // Update variables
        for (int i = 0; i < options.Length; i++)
        {
            options[i].lerper.positionlerpSpeed = selectingRotationLerpSpeed;
            options[i].lerper.rotationLerpSpeed = selectingPositionLerpSpeed;
        }
        state = State.SELECTED;
        selectedIndex = index;
        SetStateSelected();
        if (onClick != null) onClick(index);
    }
}

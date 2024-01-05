using System;
using UnityEngine;

public class ClassSelect : MonoBehaviour
{
    public Action<ClassData> onSelectClass;
    public int selectedIndex { get; private set; } = -1;
    public bool isActive { get; private set; } = false;
    public bool isSetup { get; private set; } = false;
    public bool hasInteracted { get; private set; } = false;

    public VisualToken GetSelectedOption() => (selectedIndex == -1) ? null : options[selectedIndex];

    public void SetActive(bool isActive_)
    {
        if (isActive_ == isActive) return;
        isActive = isActive_;
        for (int i = 0; i < options.Length; i++) options[i].SetActive(false);
        if (gameObject.activeSelf != isActive_) gameObject.SetActive(isActive);
        if (isActive) Reset();
    }

    [Header("References")]
    [SerializeField] private VisualToken[] options;
    [SerializeField] private Transform startPosition;

    [Header("Config")]
    [SerializeField] private float circleRadius = 5f;
    [SerializeField] private float circleOffset = 4f;
    [SerializeField] private float setupIntervalDuration = 0.3f;
    [SerializeField] private float setupOutwardDuration = 0.65f;
    [SerializeField] private float oscillateFreq = 3.0f;
    [SerializeField] private float oscillateMag = 0.45f;
    [SerializeField] private float idleScrollVel = 1.5f;
    [SerializeField] private float selectedScrollVel = 5f;
    [SerializeField] private float playHoverHeight = 0.5f;
    [SerializeField] private float selectedHoverHeight = 0.3f;
    [SerializeField] private Vector3 generationOffset = Vector3.down * 0.5f;

    private float timeStart;
    private float timeDiff;
    private float currentScroll;
    private float currentScrollVel;

    private void Awake() => SetActive(false);

    private void Reset()
    {
        // Update all variables
        timeStart = Time.time;
        timeDiff = 0.0f;
        currentScroll = 0f;
        currentScrollVel = 0f;
        selectedIndex = -1;
        isSetup = false;
        hasInteracted = false;

        // Begin setup
        transform.position = startPosition.position;
        transform.rotation = startPosition.rotation;
        for (int i = 0; i < options.Length; i++)
        {
            options[i].SetActive(false);
            options[i].Lerper.SetTarget(startPosition.position, startPosition.rotation, true, true);
            options[i].SetGlowing(false);
        }
    }

    private void Update()
    {
        if (!isActive) return;
        timeDiff = Time.time - timeStart;
        UpdateSetup();
        UpdateSelecting();
    }

    private void UpdateSetup()
    {
        if (!isActive || isSetup) return;
        transform.position = startPosition.position;
        transform.rotation = startPosition.rotation;

        // Calculate setup scroll for fancy swil
        // - new token every 0.2s
        // - token reaches end in 0.5s
        // - after 0.5s, scroll should be 0 relative to current token
        // - at 0.5s index 0 should be scroll 0 so overall scroll 0
        // - at 0.7s index 1 should be scroll 0 so overall scroll -1
        //  -at 0.9s index 2 should be scroll 0 so overall scroll -2
        float setupScroll = -(timeDiff - setupOutwardDuration) / setupIntervalDuration;

        // Setup option positions
        for (int i = 0; i < options.Length; i++)
        {
            float startTime = i * setupIntervalDuration;
            if (timeDiff >= startTime)
            {
                float outwardPct = Mathf.Min(Mathf.Max((timeDiff - startTime) / setupOutwardDuration, 0.0f), 1f);
                options[i].Lerper.SetTargetPosition(GetOptionSetupPosition(i, outwardPct, setupScroll), true);
                options[i].Lerper.SetTargetRotation(GetOptionSetupRotation(i, outwardPct, setupScroll), true);
                options[i].SetActive(true);
            }
        }

        // Begin selecting
        if (timeDiff > ((options.Length - 1) * setupIntervalDuration + setupOutwardDuration))
        {
            isSetup = true;
            currentScrollVel -= 1f / setupIntervalDuration;
            for (int i = 0; i < options.Length; i++)
            {
                options[i].Lerper.positionlerpSpeed = 15.0f;
                options[i].Lerper.rotationLerpSpeed = 15.0f;
            }
        }
    }

    private void UpdateSelecting()
    {
        if (!isActive || !isSetup) return;
        transform.position = startPosition.position;
        transform.rotation = startPosition.rotation;

        // Update each token
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) selectedIndex = -1;
        for (int i = 0; i < options.Length; i++)
        {
            Vector3 position = GetOptionSelectingPosition(i, currentScroll, selectedIndex != i);
            options[i].SetGlowing(options[i].IsHovered || selectedIndex == i);
            if (options[i].IsHovered)
            {
                if (selectedIndex == i)
                {
                    position += Vector3.up * playHoverHeight;
                    if (Input.GetMouseButtonDown(0) && onSelectClass != null) onSelectClass(options[i].OptionClass);
                }
                else
                {
                    position += Vector3.up * selectedHoverHeight;
                    if (Input.GetMouseButtonDown(0))
                    {
                        selectedIndex = i;
                        hasInteracted |= selectedIndex != -1;
                    }
                }
            }
            else if (selectedIndex == i) position += Vector3.up * selectedHoverHeight;
            options[i].Lerper.SetTargetPosition(position);
            options[i].Lerper.SetTargetRotation(GetOptionSelectingRotation(i, currentScroll));
        }

        // Scroll towards selected index
        if (selectedIndex != -1)
        {
            float targetScroll = -selectedIndex + options.Length * 0.25f;
            float diff = targetScroll - currentScroll;
            if (Mathf.Abs(diff) > options.Length / 2f) diff -= Mathf.Sign(diff) * options.Length;
            currentScrollVel = diff * selectedScrollVel;
        }

        // Update scroll variables
        else if (!hasInteracted && currentScrollVel > -idleScrollVel) currentScrollVel = -idleScrollVel;
        currentScroll += currentScrollVel * Time.deltaTime;
        currentScroll = (currentScroll + options.Length) % options.Length;
        currentScrollVel *= 0.99f;
    }

    private Vector3 GetOptionSetupPosition(float index, float pct, float scroll = 0f)
    {
        // Return lerped base options position
        return Vector3.Lerp(transform.position, GetOptionSelectingPosition(index, scroll), pct);
    }

    private Quaternion GetOptionSetupRotation(float index, float pct, float scroll = 0f)
    {
        // Return base options rotation
        return Quaternion.Lerp(Quaternion.identity, GetOptionSelectingRotation(index, scroll), pct);
    }

    private Vector3 GetOptionSelectingPosition(float index, float scroll, bool toOscillate = true)
    {
        // Return end token position with given scroll
        float angle = (-(float)(index + scroll) / options.Length - 0.25f) * Mathf.PI * 2f;
        Vector3 start = transform.position + generationOffset;
        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * circleRadius,
            toOscillate
                ? (Mathf.Sin(angle * oscillateFreq + Time.time) * oscillateMag + circleOffset)
                : (oscillateMag * 0.5f + circleOffset),
            Mathf.Sin(angle) * circleRadius);
        return start + offset;
    }

    private Quaternion GetOptionSelectingRotation(float index, float scroll = 0f)
    {
        // Generate angle pointing outwards
        float angle = ((float)(index + scroll) / options.Length + 0.25f) * 360f - 90f;
        return Quaternion.Euler(-25f, angle, 0f);
    }
}

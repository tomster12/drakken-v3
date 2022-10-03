
using UnityEngine;


public class ClassSelect : MonoBehaviour
{
    // Declare variables
    [SerializeField] private ClassSelectOption[] options;
    [SerializeField] private float circleRadius = 5f;
    [SerializeField] private float circleOffset = 4f;
    [SerializeField] private float setupPieceOffset = 0.3f;
    [SerializeField] private float setupPieceDuration = 0.65f;
    [SerializeField] private float idleScrollVel = 1.5f;
    [SerializeField] private float selectedScrollVel = 3f;
    [SerializeField] private Vector3 generationOffset = Vector3.down * 0.5f;

    private float setupTimer = 0f;
    private float currentScroll = 0f;
    private float currentScrollVel = 0f;
    public int selectedIndex { get; private set; } = -1;
    public bool isActive { get; private set; } = false;
    public bool isSetup { get; private set; } = false;
    public bool hasInteracted { get; private set; } = false;


    private void Awake() => SetActive(false);


    private void Update()
    {
        UpdateOptions();
        SetupOptions();
    }


    private void SetupOptions()
    {
        if (!isActive || isSetup) return;

        // Setup option positions
        for (int i = 0; i < options.Length; i++)
        {
            options[i].SetClickable(false);
            bool isStarted = setupTimer >= i * setupPieceOffset;
            options[i].SetActive(isStarted);
            if (isStarted)
            {
                float pct = (setupTimer - i * setupPieceOffset) / setupPieceDuration;
                pct = Mathf.Min(Mathf.Max(pct, 0f), 1f);
                float scroll = -(i - (setupTimer - setupPieceDuration) / setupPieceOffset);
                options[i].SetTargetPosition(GetOptionSetupPosition(0f, pct, scroll));
                options[i].transform.rotation = GetOptionSetupRotation(0f, pct, scroll);
            }
        }

        // Increase timer
        setupTimer += Time.deltaTime;

        // Check if is setup
        if (setupTimer > ((options.Length - 1) * setupPieceOffset + setupPieceDuration))
        {
            isSetup = true;
            currentScrollVel += 1f / setupPieceOffset;
            return;
        }
    }


    private void UpdateOptions()
    {
        if (!isActive || !isSetup) return;

        // Centre selected option
        else if (selectedIndex != -1)
        {
            float targetScroll = -selectedIndex + options.Length * 0.25f;
            float diff = targetScroll - currentScroll;
            if (Mathf.Abs(diff) > options.Length / 2f) diff -= Mathf.Sign(diff) * options.Length;
            currentScrollVel = diff * selectedScrollVel;
        }

        // Spin while not selected
        else if (!hasInteracted && currentScrollVel < idleScrollVel) currentScrollVel = idleScrollVel;

        // Update current scroll
        currentScroll += currentScrollVel * Time.deltaTime;
        currentScroll = currentScroll % options.Length;
        currentScrollVel *= 0.99f;

        // Check each token
        int newSelectedIndex = -1;
        for (int i = 0; i < options.Length; i++)
        {
            // Update option position
            options[i].SetActive(true);
            options[i].SetClickable(true);
            options[i].SetTargetPosition(GetOptionCurrentPosition(i, currentScroll));
            options[i].transform.rotation = GetOptionCurrentRotation(i, currentScroll);

            // Check if clicked
            if (options[i].GetClicked()) newSelectedIndex = i;
        }

        // Update selected token
        hasInteracted |= selectedIndex != -1;
        if (Input.GetMouseButtonDown(0) && newSelectedIndex != selectedIndex)
        {
            if (selectedIndex != -1) options[selectedIndex].SetSelected(false);
            if (newSelectedIndex != -1) options[newSelectedIndex].SetSelected(true);
            selectedIndex = newSelectedIndex;
        }
    }


    #region Token Positions

    private Vector3 GetOptionCurrentPosition(float index, float scroll)
    {
        // Return end token position with given scroll
        float angle = -((float)(index + scroll) / options.Length) * Mathf.PI * 2f;
        Vector3 start = transform.position + generationOffset;
        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * circleRadius,
            Mathf.Sin(angle * 3f + Time.time) * 0.2f + circleOffset,
            Mathf.Sin(angle) * circleRadius);
        return start + offset;
    }

    private Quaternion GetOptionCurrentRotation(float index, float scroll=0f)
    {
        // Generate angle pointing outwards
        float angle = ((float)(index + scroll) / options.Length) * 360f - 90f;
        return Quaternion.Euler(-25f, angle, 0f);
    }

    private Vector3 GetOptionSetupPosition(float index, float pct, float scroll=0f)
    {
        // Return lerped base options position
        return Vector3.Lerp(transform.position, GetOptionCurrentPosition(index, scroll), pct);
    }

    private Quaternion GetOptionSetupRotation(float index, float pct, float scroll=0f)
    {
        // Return base options rotation
        return Quaternion.Lerp(Quaternion.identity, GetOptionCurrentRotation(index, scroll), pct);
    }

    #endregion


    public void SetActive(bool isActive_)
    {
        if (isActive_ == isActive) return;

        // Update isActive
        setupTimer = 0f;
        currentScroll = 0f;
        selectedIndex = -1;
        isSetup = false;
        hasInteracted = false;
        foreach (ClassSelectOption option in options) option.SetActive(false);
        isActive = isActive_;
    }


    public ClassSelectOption GetSelectedOption() => (selectedIndex == -1) ? null : options[selectedIndex];
}

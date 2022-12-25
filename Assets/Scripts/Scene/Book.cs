
using System;
using UnityEngine;
using TMPro;


public class Book : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Outline outline;
    [SerializeField] private Animator animator;
    [SerializeField] private Light[] lights;
    [SerializeField] private PlaceLerper _placeLerper;
    public PlaceLerper placeLerper => _placeLerper;

    [Header("Content References")]
    [SerializeField] private GameObject contentUI;
    [SerializeField] private TextMeshProUGUI contentTitleText;
    [SerializeField] private TextMeshProUGUI contentDescriptionText;

    [Header("Config")]
    [SerializeField] private float glowIntensity = 50.0f;
    [SerializeField] private float glowLerpSpeed = 3.0f;
    [SerializeField] private Color outlineColor = new Color(200.0f, 100.0f, 100.0f, 255.0f);
    [SerializeField] private float outlineLerpSpeed = 3.0f;

    private float currentPct;

    public float targetPct = 1.0f;
    public float glowAmount = 0.0f;
    public float outlineAmount = 0.0f;
    public float openLerpSpeed = 1.0f;
    public float openThreshold = 0.02f;
    public bool toOpen;
    public bool isHovered { get; private set; }
    public bool isOpen { get; private set; }
    public float normalizedPct => currentPct / targetPct;


    private void Update()
    {
        // Update UI
        SetContentTitle("");
        SetContentDescription("");
        contentUI.SetActive(isOpen);

        // Update states
        currentPct = Mathf.Lerp(currentPct, toOpen ? targetPct : 0.0f, Time.deltaTime * openLerpSpeed);
        animator.SetFloat("normalizedTime", currentPct);
        isOpen = toOpen && (1 - normalizedPct) < openThreshold;

        // Lerp towards target
        placeLerper.CallUpdate();

        // Update all lights
        foreach (Light light in lights)
        {
            light.intensity = Mathf.Lerp(light.intensity, glowAmount * glowIntensity, Time.deltaTime * glowLerpSpeed);
        }

        // Update outline
        Color targetColor = outlineColor;
        targetColor.a *= outlineAmount;
        outline.OutlineColor = Color.Lerp(outline.OutlineColor, targetColor, Time.deltaTime * outlineLerpSpeed);
    }


    public void SetContentTitle(String text_) => contentTitleText.text = text_;
    public void SetContentDescription(String text_) => contentDescriptionText.text = text_;


    private void OnMouseOver() => isHovered = true;
    private void OnMouseExit() => isHovered = false;
}

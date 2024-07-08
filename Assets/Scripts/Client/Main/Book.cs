using System;
using UnityEngine;
using TMPro;

public class Book : MonoBehaviour
{
    public bool toOpen = false;
    public float openAmount = 1.0f;
    public float openLerpSpeed = 1.0f;
    public float openThreshold = 0.02f;
    public float glowAmount = 0.0f;
    public float glowLerpSpeed = 3.0f;
    public float outlineAmount = 0.0f;
    public float outlineLerpSpeed = 3.0f;

    public Waypointer Waypointer => waypointer;
    public bool IsHovered { get; private set; }
    public bool IsOpen { get; private set; }
    public float CurrentOpenPercent => currentOpenAmount / openAmount;

    public void LerpValues(bool set = false)
    {
        // Lerp book open amount
        if (!set) currentOpenAmount = Mathf.Lerp(currentOpenAmount, toOpen ? openAmount : 0.0f, Time.deltaTime * openLerpSpeed);
        else currentOpenAmount = toOpen ? openAmount : 0.0f;
        animator.SetFloat("normalizedTime", currentOpenAmount);
        IsOpen = toOpen && (1 - CurrentOpenPercent) < openThreshold;

        // Lerp lerpers
        foreach (LightLerper lerper in lightLerpers)
        {
            lerper.brightness = glowAmount;
            lerper.lerpSpeed = glowLerpSpeed;
            lerper.Lerp(set);
        }
        outlineLerper.amount = outlineAmount;
        outlineLerper.lerpSpeed = outlineLerpSpeed;
        outlineLerper.Lerp(set);
    }

    public void SetContentTitle(String text_) => contentTitleText.text = text_;

    public void SetContentDescription(String text_) => contentDescriptionText.text = text_;

    [Header("References")]
    [SerializeField] private Outline outline;
    [SerializeField] private Animator animator;
    [SerializeField] private LightLerper[] lightLerpers;
    [SerializeField] private OutlineLerper outlineLerper;
    [SerializeField] private Waypointer waypointer;

    [Header("Content References")]
    [SerializeField] private GameObject contentUI;
    [SerializeField] private TextMeshProUGUI contentTitleText;
    [SerializeField] private TextMeshProUGUI contentDescriptionText;

    private float currentOpenAmount;

    private void Awake()
    {
        Waypointer.Init(transform);
        SetContentTitle("");
        SetContentDescription("");
    }

    private void Update()
    {
        contentUI.SetActive(IsOpen);
        Waypointer.Lerp();
        LerpValues();
    }

    private void OnMouseOver() => IsHovered = true;

    private void OnMouseExit() => IsHovered = false;
}

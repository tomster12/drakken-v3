
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Book : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Outline outline;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform[] placesList;
    [SerializeField] private Light[] lights;

    [Header("Content References")]
    [SerializeField] private GameObject contentUI;
    [SerializeField] private TextMeshProUGUI contentTitleText;
    [SerializeField] private TextMeshProUGUI contentDescriptionText;

    [Header("Config")]
    [SerializeField] private float glowIntensity = 50.0f;
    [SerializeField] private float glowLerpSpeed = 3.0f;
    [SerializeField] private Color outlineColor = new Color(200.0f, 100.0f, 100.0f, 255.0f);
    [SerializeField] private float outlineLerpSpeed = 3.0f;

    private Dictionary<string, Transform> places;
    private Transform currentPlace;
    private float currentPct;

    public float movementLerpSpeed;
    public Vector3 targetPosOffset;
    public Quaternion targetRotOffset;
    public float glowAmount = 0.0f;
    public float outlineAmount = 0.0f;
    public float inPositionThreshold = 0.1f;
    public float openLerpSpeed = 1.0f;
    public float openThreshold = 0.02f;
    public bool toOpen;
    public float targetPct = 1.0f;

    public bool isHovered { get; private set; }
    public bool isOpen { get; private set; }
    public bool inPosition => ((currentPlace.position + targetPosOffset) - transform.position).magnitude < inPositionThreshold;
    public float normalizedPct => currentPct / targetPct;



    private void Awake()
    {
        // Put places into a hashmap
        places = new Dictionary<string, Transform>();
        foreach (Transform place in placesList) places[place.gameObject.name] = place;
    }


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
        if (currentPlace != null)
        {
            transform.position = Vector3.Lerp(transform.position, currentPlace.position + targetPosOffset, Time.deltaTime * movementLerpSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, currentPlace.rotation * targetRotOffset, Time.deltaTime * movementLerpSpeed);
        }

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


    public void SetPlace(string placeName, bool setPos=false)
    {
        // Dont set if already there or doesnt exist
        if (currentPlace != null && currentPlace.gameObject.name == placeName && !setPos) return;
        if (!places.ContainsKey(placeName)) return;

        // Set to camera view
        currentPlace = places[placeName];
        targetPosOffset = Vector3.zero;
        targetRotOffset = Quaternion.identity;
        if (setPos)
        {
            transform.position = currentPlace.position;
            transform.rotation = currentPlace.rotation;
        }
    }

    public void SetContentTitle(String text_) => contentTitleText.text = text_;
    public void SetContentDescription(String text_) => contentDescriptionText.text = text_;


    private void OnMouseOver() => isHovered = true;
    private void OnMouseExit() => isHovered = false;
}

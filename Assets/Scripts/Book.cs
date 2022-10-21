
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Book : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform[] placesList;
    [SerializeField] private Animator animator;

    [Header("Content References")]
    [SerializeField] private GameObject contentUI;
    [SerializeField] private TextMeshProUGUI contentTitleText;
    [SerializeField] private TextMeshProUGUI contentDescriptionText;

    private Dictionary<string, Transform> places = new Dictionary<string, Transform>();
    private Transform currentPlace;
    public float movementLerpSpeed;
    public Vector3 targetPosOffset;
    public Quaternion targetRotOffset;
    public bool isHovered { get; private set; }
    public bool toOpen;
    public bool isOpen { get; private set; }
    public bool inPosition { get; private set; }


    private void Awake()
    {
        // Put places into a hashmap
        foreach (Transform place in placesList) places[place.gameObject.name] = place;
    }


    private void Update()
    {
        // Update UI
        SetContentTitle("");
        SetContentDescription("");
        contentUI.SetActive(isOpen);

        // Update states
        animator.SetBool("isOpen", toOpen);
        isOpen = toOpen && animator.GetCurrentAnimatorStateInfo(0).IsName("Open");

        // Lerp towards target
        if (currentPlace != null)
        {
            transform.position = Vector3.Lerp(transform.position, currentPlace.position + targetPosOffset, Time.deltaTime * movementLerpSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, currentPlace.rotation * targetRotOffset, Time.deltaTime * movementLerpSpeed);
            inPosition = ((currentPlace.position + targetPosOffset) - transform.position).magnitude < 0.5f;
        }
    }


    public void SetPlace(string placeName, bool setPos=false)
    {
        // Dont set if already there or doesnt exist
        if (currentPlace != null && currentPlace.gameObject.name == placeName) return;
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

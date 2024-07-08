using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class ClientStateTitle : ClientState
{
    public override void Set()
    {
        cameraController.Waypointer.positionLerpSpeed = camLerpSpeed;
        book.toOpen = false;
        book.outlineAmount = 0.0f;
        book.glowAmount = 0.0f;
        book.glowLerpSpeed = bookGlowLerpSpeed;
        book.openLerpSpeed = bookOpenLerpSpeed;
        fire.brightness = 0.0f;
        fire.lerpSpeed = fireBrightnessLerpSpeed;
        isHovered = false;
        isOpening = false;

        if (firstTime)
        {
            cameraController.Waypointer.SetPlace(camPlace, true, true);
            book.Waypointer.SetPlace(bookPlace, true, true);
            fire.LerpValues(true);
            book.LerpValues(true);
            isTitleFaded = false;
            firstTime = false;
        }
        else
        {
            cameraController.Waypointer.SetPlace(camPlace);
            book.Waypointer.SetPlace(bookPlace);
        }
    }

    public override void Update()
    {
        if (isOpening) return;

        isHovered = (Input.mousePosition.y / Screen.height) < 0.2f;

        if (!isHovered)
        {
            book.outlineAmount = 0.0f;
            book.glowAmount = 0.0f;
            fire.brightness = fireBrightnessNeutral;
        }
        else
        {
            book.outlineAmount = bookOutlineHovered;
            book.glowAmount = bookGlowHovered;
            fire.brightness = fireBrightnessHovered;

            if (Input.GetMouseButtonDown(0)) GotoNext();
        }
    }

    [Header("References")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Book book;
    [SerializeField] private Fire fire;
    [SerializeField] private Fizzler titleFizzler;

    [Header("Config")]
    [SerializeField] private string camPlace = "Default";
    [SerializeField] private float camLerpSpeed = 2.0f;
    [SerializeField] private string bookPlace = "Default";
    [SerializeField] private float bookOpenLerpSpeed = 3.0f;
    [SerializeField] private float bookGlowLerpSpeed = 3.0f;
    [SerializeField] private float bookOutlineHovered = 1.0f;
    [SerializeField] private float bookGlowHovered = 0.7f;
    [SerializeField] private float bookGlowOpening = 1.5f;
    [SerializeField] private float fireBrightnessLerpSpeed = 3.0f;
    [SerializeField] private float fireBrightnessNeutral = 0.01f;
    [SerializeField] private float fireBrightnessHovered = 0.09f;
    [SerializeField] private float fireBrightnessOpening = 0.13f;

    private bool isTitleFaded = false;
    private bool isHovered = false;
    private bool isOpening = false;
    private bool firstTime = true;

    private void GotoNext() => client.StartCoroutine(GotoNextIE());

    private IEnumerator GotoNextIE()
    {
        if (isOpening) throw new Exception("Cannot GotoNext already isOpening");

        isOpening = true;
        book.glowAmount = bookGlowOpening;
        fire.brightness = fireBrightnessOpening;
        if (!isTitleFaded)
        {
            titleFizzler.StartFizzle();
            yield return new WaitForSeconds(0.4f);
            isTitleFaded = true;
        }

        client.SetState(Client.StateType.MENU);
    }
}

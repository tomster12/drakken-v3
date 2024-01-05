using System;
using System.Collections;
using UnityEngine;

// TODO: Cleanup
[Serializable]
public class ClientStateTitle : ClientState
{
    public ClientStateTitle(Client app) : base(app)
    {
    }

    public void SetToBase()
    {
        app.CameraController.Waypointer.SetPlace(camPlace, true);
        app.Book.Waypointer.SetPlace(bookPlace, true);
        ResetValues();
        app.Fire.LerpValues(true);
        app.Book.LerpValues(true);
        app.Book.LerpValues(true); // TODO: Is this needed?

        // Start fade - assume title existing
        isTitleFaded = false;
    }

    public override void Set()
    {
        app.CameraController.Waypointer.SetPlace(camPlace);
        app.Book.Waypointer.SetPlace(bookPlace);
        ResetValues();
    }

    public override void Update()
    {
        if (isOpening) return;

        // Check if hovered
        isHovered = (Input.mousePosition.y / Screen.height) < 0.2f;

        // Handle neutral
        if (!isHovered)
        {
            app.Book.outlineAmount = 0.0f;
            app.Book.glowAmount = 0.0f;
            app.Fire.brightness = neutralFireBrightness;
        }

        // Handle book hovered
        else
        {
            app.Book.outlineAmount = hoveredBookOutlineAmount;
            app.Book.glowAmount = hoveredBookGlowAmount;
            app.Fire.brightness = hoveredFireBrightness;

            // Transition on mouse click
            if (Input.GetMouseButtonDown(0)) GotoNext();
        }
    }

    private const string camPlace = "Default";
    private const string bookPlace = "Default";
    private const float camLerpSpeed = 2.0f;
    private const float bookOpenLerpSpeed = 3.0f;
    private const float bookGlowLerpSpeed = 3.0f;
    private const float neutralFireBrightness = 0.01f;
    private const float hoveredFireBrightness = 0.09f;
    private const float hoveredBookOutlineAmount = 1.0f;
    private const float hoveredBookGlowAmount = 0.7f;
    private const float openingBookGlowAmount = 1.5f;
    private const float openingFireBrightness = 0.13f;

    private bool isTitleFaded;
    private bool isHovered;
    private bool isOpening;

    private void ResetValues()
    {
        // Reset all component values to base level
        app.Book.glowLerpSpeed = bookGlowLerpSpeed;
        app.CameraController.Waypointer.positionlerpSpeed = camLerpSpeed;
        app.Book.openLerpSpeed = bookOpenLerpSpeed;
        app.Book.outlineAmount = 0.0f;
        app.Book.glowAmount = 0.0f;
        app.Book.toOpen = false;
        app.Fire.brightness = 0.0f;
        isHovered = false;
        isOpening = false;
    }

    private void GotoNext() => app.StartCoroutine(GotoNextIE());

    private IEnumerator GotoNextIE()
    {
        // Set values, fade title, wait, then transition
        isOpening = true;
        app.Book.glowAmount = openingBookGlowAmount;
        app.Fire.brightness = openingFireBrightness;
        if (!isTitleFaded)
        {
            FadeTitle();
            yield return new WaitForSeconds(0.8f);
        }
        app.SetAppState(Client.StateType.MENU);
    }

    private void FadeTitle()
    {
        if (isTitleFaded) return;

        // Fade title text - should only happen once
        isTitleFaded = true;
        //titleFizzler.StartFizzle(); TODO
    }
}

using System;
using UnityEngine;

[Serializable]
public class ClientStateMenu : ClientState
{
    public override void Set()
    {
        cameraController.Waypointer.SetPlace(camPlace);
        cameraController.Waypointer.positionLerpSpeed = 0.8f;
        book.Waypointer.SetPlace(bookPlace);
        book.toOpen = true;
        book.outlineAmount = 0.0f;
        book.glowAmount = bookGlowAmount;
        book.openLerpSpeed = bookOpenLerpSpeed;
        fire.brightness = fireBrightness;
    }

    public override void Unset()
    {
        book.toOpen = false;
    }

    public override void Update()
    {
        if (Input.GetMouseButtonDown(0)) client.SetState(Client.StateType.MATCHMAKING);
        if (Input.GetMouseButtonDown(1)) client.SetState(Client.StateType.TITLE);
    }

    [Header("References")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Book book;
    [SerializeField] private Fire fire;

    [Header("Config")]
    [SerializeField] private string camPlace = "Default";
    [SerializeField] private string bookPlace = "Default";
    [SerializeField] private float bookGlowAmount = 1.0f;
    [SerializeField] private float bookOpenLerpSpeed = 0.8f;
    [SerializeField] private float fireBrightness = 1.0f;
}

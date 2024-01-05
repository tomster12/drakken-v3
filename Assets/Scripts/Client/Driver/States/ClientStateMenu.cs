using System;
using UnityEngine;

[Serializable]
public class ClientStateMenu : ClientState
{
    public ClientStateMenu(Client app) : base(app)
    {
    }

    public override void Set()
    {
        app.Book.Waypointer.SetPlace("default");
        app.CameraController.Waypointer.SetPlace("default");
        app.CameraController.Waypointer.positionlerpSpeed = 0.8f;

        app.Book.toOpen = true;
        app.Book.outlineAmount = 0.0f;
        app.Book.glowAmount = 200.0f;
        app.Book.openLerpSpeed = 0.8f;
        app.Fire.brightness = 200.0f;
    }

    public override void Unset()
    {
        app.Book.toOpen = false;
    }

    public override void Update()
    {
        if (Input.GetMouseButtonDown(0)) app.SetAppState(Client.StateType.TITLE);
    }
}

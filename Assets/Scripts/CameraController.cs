
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    // Declare variables
    public static CameraController instance { get; private set; }

    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform[] viewsList;
    [SerializeField] private PlaceLerper _placeLerper;
    public PlaceLerper placeLerper => _placeLerper;

    [Header("Config")]
    [SerializeField] private float wibbleAmount = 3.5f;
    [SerializeField] private float viewLerp = 3.5f;


    private void Awake()
    {
        // Singleton handling
        if (instance != null) return;
        instance = this;

        // Set camera variables
        cam.depthTextureMode = DepthTextureMode.Depth;

        // Set to default view
        placeLerper.SetPlace("Default", true);
    }


    private void Update()
    {
        // Wibble current offset rotation
        placeLerper.SetOffsetRotation(GetWibbledOffset(placeLerper.currentPlace.rotation), false);
        placeLerper.CallUpdate();
    }


    private Quaternion GetWibbledOffset(Quaternion rot)
    {
        // Move camera towards mouse slightly
        float xCurrent = (rot.eulerAngles.x < 180f) ? rot.eulerAngles.x : (rot.eulerAngles.x - 360f);
        float yCurrent = (rot.eulerAngles.y < 180f) ? rot.eulerAngles.y : (rot.eulerAngles.y - 360f);
        float xPct = Mathf.Min(Mathf.Max(2f * (Input.mousePosition.y / Screen.height - 0.5f), -1f), 1f);
        float yPct = Mathf.Min(Mathf.Max(2f * (Input.mousePosition.x / Screen.width - 0.5f), -1f), 1f);
        float xWibbled = xCurrent - wibbleAmount * xPct;
        float yWibbled = yCurrent + wibbleAmount * yPct;
        Quaternion target = Quaternion.Euler(xWibbled, yWibbled, rot.eulerAngles.z);
        Quaternion newRot = Quaternion.Inverse(rot) * target;
        return newRot;
    }
}

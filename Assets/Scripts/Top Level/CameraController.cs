
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    // Declare variables
    public static CameraController instance { get; private set; }

    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform[] viewsList;

    [Header("Config")]
    [SerializeField] private float wibbleAmount = 3.5f;
    [SerializeField] private float viewLerp = 3.5f;

    private Dictionary<string, Transform> views = new Dictionary<string, Transform>();
    private Transform currentView;


    private void Awake()
    {
        // Singleton handling
        if (instance != null) return;
        instance = this;

        // Set camera variables
        cam.depthTextureMode = DepthTextureMode.Depth;

        // Put views into a hashmap
        foreach (Transform view in viewsList) views[view.gameObject.name] = view;

        // Set to default view
        SetView("Default", true);
    }


    private void Update()
    {
        // Run update functions
        UpdatePosition();
    }


    private void UpdatePosition()
    {
        // Move camera to current view
        if (currentView != null)
        {
            Vector3 newPos = Vector3.Lerp(transform.position, currentView.position, Time.deltaTime * viewLerp);
            Quaternion wibbledTarget = GetWibbledRotation(currentView.rotation);
            Quaternion newRot = Quaternion.Lerp(transform.rotation, wibbledTarget, Time.deltaTime * viewLerp);
            transform.position = newPos;
            transform.rotation = newRot;
        }
    }


    private Quaternion GetWibbledRotation(Quaternion rot)
    {
        // Move camera towards mouse slightly
        float xCurrent = (rot.eulerAngles.x < 180f) ? rot.eulerAngles.x : (rot.eulerAngles.x - 360f);
        float yCurrent = (rot.eulerAngles.y < 180f) ? rot.eulerAngles.y : (rot.eulerAngles.y - 360f);
        float xPct = Mathf.Min(Mathf.Max(2f * (Input.mousePosition.y / Screen.height - 0.5f), -1f), 1f);
        float yPct = Mathf.Min(Mathf.Max(2f * (Input.mousePosition.x / Screen.width - 0.5f), -1f), 1f);
        float xWibbled = xCurrent - wibbleAmount * xPct;
        float yWibbled = yCurrent + wibbleAmount * yPct;
        return Quaternion.Euler(xWibbled, yWibbled, rot.eulerAngles.z);
    }


    public void SetView(string viewName, bool setPos=false)
    {
        // Dont set if already or doesnt exist
        if (currentView != null && currentView.gameObject.name == viewName) return;
        if (!views.ContainsKey(viewName)) return;

        // Set to camera view
        currentView = views[viewName];
        if (setPos)
        {
            transform.position = currentView.position;
            transform.rotation = currentView.rotation;
        }
    }
}

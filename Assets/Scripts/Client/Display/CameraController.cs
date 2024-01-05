using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }
    public Waypointer Waypointer => waypointer;

    public Quaternion GetMousePannedRotation(Quaternion rot)
    {
        float xCurrent = (rot.eulerAngles.x < 180f) ? rot.eulerAngles.x : (rot.eulerAngles.x - 360f);
        float yCurrent = (rot.eulerAngles.y < 180f) ? rot.eulerAngles.y : (rot.eulerAngles.y - 360f);
        float xPct = Mathf.Min(Mathf.Max(2f * (Input.mousePosition.y / Screen.height - 0.5f), -1f), 1f);
        float yPct = Mathf.Min(Mathf.Max(2f * (Input.mousePosition.x / Screen.width - 0.5f), -1f), 1f);
        float xPanned = xCurrent - mousePanAmount * xPct;
        float yPanned = yCurrent + mousePanAmount * yPct;
        Quaternion target = Quaternion.Euler(xPanned, yPanned, rot.eulerAngles.z);
        Quaternion newRot = Quaternion.Inverse(rot) * target;
        return newRot;
    }

    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Waypointer waypointer;

    [Header("Config")]
    [SerializeField] private float mousePanAmount = 3.5f;

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;

        cam.depthTextureMode = DepthTextureMode.Depth;
    }

    private void Start()
    {
        Waypointer.Init(transform);
        Waypointer.SetPlace("Default", true);
    }

    private void Update()
    {
        Waypointer.SetOffsetRotation(GetMousePannedRotation(Waypointer.CurrentWaypoint.rotation), false);
        Waypointer.Lerp();
    }
}

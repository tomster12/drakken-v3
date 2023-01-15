
using System.Collections.Generic;
using UnityEngine;


public class PlaceLerper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform[] placeList;
    [SerializeField] private bool log;

    private Dictionary<string, Transform> places = new Dictionary<string, Transform>();

    public float inPositionThreshold = 0.2f;
    public float isRotatedThreshold = 0.2f;
    public float positionlerpSpeed = 3.0f;
    public float rotationLerpSpeed = 3.0f;
    private bool toLerpPosition;
    private bool toLerpRotation;
    public Vector3 targetOffsetPosition { get; private set; } = Vector3.zero;
    public Quaternion targetOffsetRotation { get; private set; } = Quaternion.identity;
    public Transform currentPlace { get; private set; }
    public bool inPosition => (currentPlace.position + targetOffsetPosition - transform.position).magnitude < inPositionThreshold;
    public bool isRotated => Quaternion.Angle(currentPlace.rotation * targetOffsetRotation, transform.rotation) < isRotatedThreshold;
    public Vector3 targetPosition => currentPlace.position + targetOffsetPosition;
    public Quaternion targetRotation => currentPlace.rotation * targetOffsetRotation;


    private void Awake()
    {
        foreach (Transform place in placeList) places[place.gameObject.name] = place;
    }


    public void CallUpdate()
    {
        if (toLerpPosition) transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionlerpSpeed);
        else transform.position = targetPosition;
        if (toLerpRotation) transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * positionlerpSpeed);
        else transform.rotation = targetRotation;
    }


    public void SetPlace(string placeName, bool setPos = false, bool setRot = false)
    {
        // Dont set if already or doesnt exist
        if (currentPlace != null && currentPlace.gameObject.name == placeName && !setPos) return;
        if (!places.ContainsKey(placeName)) return;

        // Set to place
        currentPlace = places[placeName];
        toLerpPosition = !setPos;
        toLerpRotation = !setRot;
        if (setPos) transform.position = targetPosition;
        if (setRot) transform.rotation = targetRotation;
    }


    public void SetOffset(Vector3 position, Quaternion rotation, bool setPos = false, bool setRot = false)
    {
        SetOffsetPosition(position, setPos);
        SetOffsetRotation(rotation, setRot);
    }

    public void SetOffsetPosition(Vector3 position, bool set = false)
    {
        targetOffsetPosition = position;
        toLerpPosition = !set;
        if (set) transform.position = targetPosition;
    }

    public void SetOffsetRotation(Quaternion rotation, bool set = false)
    {
        targetOffsetRotation = rotation;
        toLerpRotation = !set;
        if (set) transform.rotation = targetRotation;
    }
}

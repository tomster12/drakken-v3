using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Waypointer
{
    public float inPositionThreshold = 0.2f;
    public float isRotatedThreshold = 0.2f;
    public float positionlerpSpeed = 3.0f;
    public float rotationLerpSpeed = 3.0f;

    public Vector3 TargetOffsetPosition { get; private set; } = Vector3.zero;
    public Quaternion TargetOffsetRotation { get; private set; } = Quaternion.identity;
    public Transform CurrentWaypoint { get; private set; }
    public Transform ControlledTransform { get; private set; }
    public bool ReachedPosition => (CurrentWaypoint.position + TargetOffsetPosition - ControlledTransform.position).magnitude < inPositionThreshold;
    public bool ReachedRotation => Quaternion.Angle(CurrentWaypoint.rotation * TargetOffsetRotation, ControlledTransform.rotation) < isRotatedThreshold;
    public Vector3 TargetPosition => CurrentWaypoint.position + TargetOffsetPosition;
    public Quaternion TargetRotation => CurrentWaypoint.rotation * TargetOffsetRotation;

    public void Init(Transform controlledTransform)
    {
        ControlledTransform = controlledTransform;
        foreach (Transform place in waypoints)
        {
            places[place.gameObject.name] = place;
        }
    }

    public void Lerp(bool set = false)
    {
        if (toLerpPosition && !set) ControlledTransform.position = Vector3.Lerp(ControlledTransform.position, TargetPosition, Time.deltaTime * positionlerpSpeed);
        else ControlledTransform.position = TargetPosition;
        if (toLerpRotation && !set) ControlledTransform.rotation = Quaternion.Lerp(ControlledTransform.rotation, TargetRotation, Time.deltaTime * positionlerpSpeed);
        else ControlledTransform.rotation = TargetRotation;
    }

    public void SetOffset(Vector3 position, Quaternion rotation, bool setPos = false, bool setRot = false)
    {
        SetOffsetPosition(position, setPos);
        SetOffsetRotation(rotation, setRot);
    }

    public void SetOffsetPosition(Vector3 position, bool set = false)
    {
        TargetOffsetPosition = position;
        toLerpPosition = !set;
        if (set) ControlledTransform.position = TargetPosition;
    }

    public void SetOffsetRotation(Quaternion rotation, bool set = false)
    {
        TargetOffsetRotation = rotation;
        toLerpRotation = !set;
        if (set) ControlledTransform.rotation = TargetRotation;
    }

    public void SetPlace(string placeName, bool setPos = false, bool setRot = false)
    {
        // Dont set if already or doesnt exist
        if (CurrentWaypoint != null && CurrentWaypoint.gameObject.name == placeName && !setPos) return;
        if (!places.ContainsKey(placeName)) return;

        // Set to place
        CurrentWaypoint = places[placeName];
        toLerpPosition = !setPos;
        toLerpRotation = !setRot;
        if (setPos) ControlledTransform.position = TargetPosition;
        if (setRot) ControlledTransform.rotation = TargetRotation;
    }

    [SerializeField] private Transform[] waypoints;
    [SerializeField] private bool log;

    private Dictionary<string, Transform> places = new Dictionary<string, Transform>();
    private bool toLerpPosition;
    private bool toLerpRotation;
}

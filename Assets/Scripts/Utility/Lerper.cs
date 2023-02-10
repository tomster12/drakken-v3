
using UnityEngine;


public class Lerper : MonoBehaviour
{
    public float inPositionThreshold = 0.2f;
    public float isRotatedThreshold = 0.2f;
    public float positionlerpSpeed = 3.0f;
    public float rotationLerpSpeed = 3.0f;
    private bool toLerpPosition;
    private bool toLerpRotation;
    public Vector3 targetPosition { get; private set; }
    public Quaternion targetRotation { get; private set; }
    public bool inPosition => (targetPosition - transform.position).magnitude < inPositionThreshold;
    public bool isRotated => Quaternion.Angle(targetRotation, transform.rotation) < isRotatedThreshold;


    public void Lerp(bool set=false)
    {
        if (toLerpPosition && !set) transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionlerpSpeed);
        else transform.position = targetPosition;
        if (toLerpRotation && !set) transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
        else transform.rotation = targetRotation;
    }


    public void SetTarget(Vector3 position, Quaternion rotation, bool setPos = false, bool setRot = false)
    {
        SetTargetPosition(position, setPos);
        SetTargetRotation(rotation, setRot);
    }

    public void SetTargetPosition(Vector3 position, bool set=false)
    {
        targetPosition = position;
        toLerpPosition = !set;
        if (set) transform.position = targetPosition;
    }

    public void SetTargetRotation(Quaternion rotation, bool set=false)
    {
        targetRotation = rotation;
        toLerpRotation = !set;
        if (set) transform.rotation = targetRotation;
    }
}

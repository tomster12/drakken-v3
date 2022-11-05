
using UnityEngine;


public class Lerper : MonoBehaviour
{
    public Vector3 targetPosition;
    public Quaternion targetRotation;
    public float positionlerpSpeed = 3.0f;
    public float rotationLerpSpeed = 3.0f;
    public bool toLerpPosition;
    public bool toLerpRotation;


    public void CallUpdate()
    {
        if (toLerpPosition) transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionlerpSpeed);
        else transform.position = targetPosition;
        if (toLerpRotation) transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
        else transform.rotation = targetRotation;
    }
}

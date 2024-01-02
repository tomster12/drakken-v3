using UnityEngine;

[CreateAssetMenu(fileName = "Class Data", menuName = "Scriptables/ClassData")]
public class ClassData : ScriptableObject
{
    public string className;
    [TextArea] public string classDescription;
}

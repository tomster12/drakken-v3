using UnityEngine;

[CreateAssetMenu(fileName = "TokenData", menuName = "Scriptables/TokenData")]
public class TokenData : ScriptableObject
{
    public bool isFull;
    public Mesh tokenMesh;
    public string tokenName;
}

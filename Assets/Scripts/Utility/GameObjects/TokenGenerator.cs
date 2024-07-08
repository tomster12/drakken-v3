using UnityEngine;

[ExecuteInEditMode]
public class TokenGenerator : MonoBehaviour
{
    public void SetToken(TokenData token_)
    {
        // Initialize variables
        token = token_;
        UpdateMesh();
    }

    
    [Header("References")]
    [SerializeField] private TokenData token;
    [SerializeField] private MeshFilter filter;

    private void Update() => UpdateMesh();

    private void UpdateMesh()
    {
        // Keep mesh correct
        if (filter != null
            && token != null
            && token.tokenMesh != null
            && filter.sharedMesh != token.tokenMesh)
        {
            filter.sharedMesh = token.tokenMesh;
        }
    }
}

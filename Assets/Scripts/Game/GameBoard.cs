
using System.Collections.Generic;
using UnityEngine;


public class GameBoard : MonoBehaviour
{
    // Declare variables
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject tokenPfb;

    private bool isOpponent;
    private List<GameToken> currentTokens;
    private Bounds boardBounds;
    private float tokenWidth = 2f;
    private GameToken selectedFullToken;
    private List<GameToken> selectedPartialTokens;


    private void Awake()
    {
        // Initialize variables
        boardBounds = transform.GetChild(0).GetComponent<MeshRenderer>().bounds;
    }


    public void GenerateTokens(int count, bool reset = true)
    {
        // Reset tokens if needed
        if (reset) ResetTokens();

        // Create new tokens
        for (int i = 0; i < count; i++)
        {
            GameObject tokenObj = Instantiate(tokenPfb);
            GameToken token = tokenObj.GetComponent<GameToken>();
            token.LoadToken(gameManager.GenerateRandomToken());
            tokenObj.transform.localScale = Vector3.one * tokenWidth;
            tokenObj.transform.parent = transform;
            currentTokens.Add(token);
        }
    }


    private void Update()
    {
        // Update token positions
        UpdateTokens();
    }


    private void UpdateTokens()
    {
        if (currentTokens != null)
        {
            for (int i = 0; i < currentTokens.Count; i++)
            {
                // Update each tokens positions
                Vector3 newPos = GetTokenPosition(i);
                currentTokens[i].SetTargetPosition(newPos);

                // Select tokens
                if (currentTokens[i].GetClicked())
                {
                    TokenData data = currentTokens[i].GetTokenData();
                    if (data.isFull)
                    {
                        if (selectedFullToken != null) selectedFullToken.SetSelected(false);
                        selectedFullToken = currentTokens[i];
                        currentTokens[i].SetSelected(true);
                    }
                }
            }
        }
    }


    public Vector3 GetTokenPosition(int index)
    {
        // Return token target position
        return new Vector3(
            transform.position.x - boardBounds.extents.x + 0.5f * tokenWidth + index * (tokenWidth * 1.2f),
            transform.position.y + boardBounds.extents.y * 2f,
            transform.position.z
        );
    }


    public void ResetTokens()
    {
        // Reset current tokens
        if (currentTokens == null) currentTokens = new List<GameToken>();
        foreach (GameToken t in currentTokens) Destroy(t.gameObject);
        currentTokens.Clear();
    }
}

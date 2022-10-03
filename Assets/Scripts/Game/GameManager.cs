
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // Declare variables
    public static GameManager instance;

    [Header("References")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Fire fire;

    [SerializeField] private Book.BookStateIngame ingameBook;
    [SerializeField] private TokenData[] potentialTokens;
    [SerializeField] private Transform diceRollTransform;
    [SerializeField] private HoverChild cornerViewHover;
    [SerializeField] private GameBoard localBoard;
    [SerializeField] GameObject dicePfb;

    private Match match;
    private bool isLoaded;
    private ClassData currentClass;
    private List<GameDice> currentDice;
    private int diceValueTotal;


    private void Awake()
    {
        // Singleton handling
        if (instance != null) return;
        instance = this;
    }


    public void StartGame(ClassData currentClass_, Book.BookStateIngame ingameBook_)
    {
        // Update variables
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        currentClass = currentClass_;
        ingameBook = ingameBook_;

        // Ready up
        Match.instance.ReadyUp(LoadGame);
    }

    public void StopGame()
    {
        // Update variables
        if (gameObject.activeSelf) gameObject.SetActive(false);
        currentClass = null;
        ingameBook = null;
    }

    public void LoadGame()
    {
        // Roll dice, generate tokens
        RollDice(4, true);
        localBoard.GenerateTokens(5, true);
        isLoaded = true;
    }

    public void UnloadGame()
    {
        // Unload variables
        ResetDice();
        localBoard.ResetTokens();
        isLoaded = false;
    }


    private void Update()
    {
        if (!isLoaded) return;

        // Update dice value total
        diceValueTotal = 0;
        foreach (GameDice dice in currentDice) diceValueTotal += dice.getValue();

        // Update Book
        if (ingameBook != null)
        {
            ingameBook.SetContentTitle(currentClass.className);
            ingameBook.SetContentDescription(currentClass.classDescription);
            ingameBook.SetCornerHovered(cornerViewHover.GetHovered());
            if (ingameBook.IsOpen()) cameraController.SetView("Corner Book Close");
            else cameraController.SetView("Default");
        }

        // Update fire
        fire.setBrightness(0.2f);
    }


    public TokenData GenerateRandomToken()
    {
        // Generate a random token
        int index = UnityEngine.Random.Range(0, potentialTokens.Length);
        return potentialTokens[index];
    }


    private void RollDice(int count, bool reset = true) => StartCoroutine(RollDiceIE(count, reset));

    private IEnumerator RollDiceIE(int count, bool reset)
    {
        // Reset dice if needed
        if (reset) ResetDice();

        // Create new dice
        for (int i = 0; i < count; i++)
        {
            // Wait inbetween each dice
            yield return new WaitForSeconds(0.15f);

            // Generate dice object
            GameObject diceObj = Instantiate(dicePfb);
            Rigidbody rb = diceObj.GetComponent<Rigidbody>();
            GameDice dice = diceObj.GetComponent<GameDice>();
            diceObj.transform.position = diceRollTransform.position;
            diceObj.transform.rotation = Quaternion.LookRotation(UnityEngine.Random.onUnitSphere, Vector3.up);
            rb.AddForce(diceRollTransform.forward * 250f);
            rb.AddTorque(UnityEngine.Random.onUnitSphere * 30f);
            diceObj.transform.parent = transform;
            currentDice.Add(dice);
        }

        // Calculate dice total
        diceValueTotal = 0;
        foreach (GameDice d in currentDice) diceValueTotal += d.getValue();
    }

    private void ResetDice()
    {
        // Reset current dice
        if (currentDice == null) currentDice = new List<GameDice>();
        foreach (GameDice d in currentDice) Destroy(d.gameObject);
        currentDice.Clear();
    }


    public void SetMatch(Match match_) => match = match_;
}


using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // Declare variables
    public static GameManager instance { get; private set; }

    [Header("References")]
    [SerializeField] private HoverChild cornerViewHover;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private MultiplayerManager multiplayerManager;
    [SerializeField] private Fire fire;
    [SerializeField] private Book book;

    [SerializeField] private AppManager.AppStateIngame appState;
    [SerializeField] private TokenData[] potentialTokens;
    [SerializeField] private Transform diceRollTransform;
    [SerializeField] private GameBoard localBoard;
    [SerializeField] GameObject dicePfb;

    private bool isActive;
    private bool isStarted;
    private ClassData currentClass;
    private List<GameDice> currentDice;
    private int diceValueTotal;


    private void Awake()
    {
        // Singleton handling
        if (instance != null) return;
        instance = this;
    }
    
    
    private void Update()
    {
        if (!isStarted) return;

        // Update dice value total
        diceValueTotal = 0;
        foreach (GameDice dice in currentDice) diceValueTotal += dice.getValue();

        // Update state
        book.toOpen = book.inPosition && (book.isHovered || cornerViewHover.GetHovered());
        book.SetContentTitle(currentClass.className);
        book.SetContentDescription(currentClass.classDescription);
        book.SetPlace("Tabletop Corner");
        fire.brightness = 0.2f;
        cameraController.SetView("Default");
        if (book.toOpen) cameraController.SetView("Tabletop Corner");
        else cameraController.SetView("Default");

        // Leave on space
        if (Input.GetKeyDown(KeyCode.Space)) multiplayerManager.TryLeaveMatchmaking();
    }


    public void EnterGame(ClassData currentClass_, AppManager.AppStateIngame appState_)
    {
        if (isActive) return;

        // Update variables
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        currentClass = currentClass_;
        appState = appState_;
        isActive = true;

        // Ready up
        Match.instance.ReadyUp(StartGame);
    }

    public void ExitGame()
    {
        if (!isStarted) return;

        // Update variables
        if (isActive) StopGame();
        if (gameObject.activeSelf) gameObject.SetActive(false);
        currentClass = null;
        appState = null;
        isActive = false;
    }


    public void StartGame()
    {
        if (isStarted) return;

        // Roll dice, generate tokens
        RollDice(4, true);
        localBoard.GenerateTokens(5, true);
        isStarted = true;
    }

    public void StopGame()
    {
        if (!isStarted) return;

        // Unload variables
        ResetDice();
        localBoard.ResetTokens();
        isStarted = false;
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
}

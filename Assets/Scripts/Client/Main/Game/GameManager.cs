using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Fix
public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public void EnterGame(ClassData currentClass_, ClientStateIngame clientState_)
    {
        if (isActive) return;

        // Update variables
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        currentClass = currentClass_;
        clientState = clientState_;
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
        clientState = null;
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

    [Header("References")]
    [SerializeField] private HoverChild cornerViewHover;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Fire fire;
    [SerializeField] private Book book;

    [SerializeField] private ClientStateIngame clientState;
    [SerializeField] private TokenData[] potentialTokens;
    [SerializeField] private Transform diceRollTransform;
    [SerializeField] private GameBoard localBoard;
    [SerializeField] private GameObject dicePfb;

    private bool isActive;
    private bool isStarted;
    private ClassData currentClass;
    private List<GameDice> currentDice;
    private int diceValueTotal;

    private void Awake()
    {
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
        book.toOpen = book.Waypointer.ReachedPosition && (book.IsHovered || cornerViewHover.GetHovered());
        book.SetContentTitle(currentClass.className);
        book.SetContentDescription(currentClass.classDescription);
        book.Waypointer.SetPlace("Tabletop Corner");
        fire.brightness = 0.2f;
        cameraController.Waypointer.SetPlace("Default");
        if (book.toOpen) cameraController.Waypointer.SetPlace("Tabletop Corner");
        else cameraController.Waypointer.SetPlace("Default");

        // Leave on space
        if (Input.GetKeyDown(KeyCode.Space)) Matchmaker.Instance.TryLeaveMatchmaking();
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

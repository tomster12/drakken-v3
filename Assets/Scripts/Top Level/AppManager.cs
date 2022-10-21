
using System;
using UnityEngine;


public class AppManager : MonoBehaviour
{
    public static AppManager instance { get; private set; }

    [Header("References")]
    [SerializeField] private MultiplayerManager multiplayerManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private ClassSelect classSelect;
    [SerializeField] private Fire fire;
    [SerializeField] private Book book;

    private AppState state;
    [SerializeField] private AppStateMatchmaker stateMatchmaker;
    [SerializeField] private AppStateSelecting stateSelecting;
    [SerializeField] private AppStateIngame stateIngame;


    private void Awake()
    {
        // Singleton handling
        if (instance != null) return;
        instance = this;

        // Add event listeners
        MultiplayerManager.OnTryConnect += OnTryConnect;
        MultiplayerManager.OnDisconnect += OnDisconnect;
        Matchmaker.OnFindMatch += OnFindMatch;
        Matchmaker.OnCloseMatch += OnCloseMatch;

        // Main initialization
        multiplayerManager.Init(Application.dataPath + "\\config.cfg");
        if (multiplayerManager.isServer) return;
        GotoFirstState();
    }


    private void Update()
    {
        // Update state
        if (state != null) state.Update();
    }


    #region States

    [Serializable]
    abstract public class AppState
    {
        [SerializeField] protected AppManager app;

        virtual public void Set() { }
        virtual public void Unset() { }

        virtual public void Update() { }

        virtual public void OnTryConnect(bool success) { }
        virtual public void OnDisconnect() { }
        virtual public void OnFindMatch(bool success) { }
        virtual public void OnCloseMatch() { }
    }


    [Serializable]
    public class AppStateMatchmaker : AppState
    {
        [Header("Config")]
        [SerializeField] private Vector3 hoverPosOffset = new Vector3(0.0f, 2f, 0.0f);
        [SerializeField] private Quaternion hoverRotOffset = Quaternion.Euler(-10f, 0.0f, 0.0f);
        [SerializeField] private float floatDuration = 1.65f;
        [SerializeField] private float floatMagnitude = 0.25f;
        [SerializeField] private float movementLerpSpeed = 4.5f;
        private float timeOffset;
        private float spinTimeOffset;
        private int matchmakingStatus = 0;


        public override void Set()
        {
            // Initialize book state
            app.cameraController.SetView("Default");
            timeOffset = Time.time;
            matchmakingStatus = 0;
            app.fire.brightness = 0.0f;
            app.fire.SetValues();
        }

        public void SetToBase()
        {
            // Set to base positions
            app.book.SetPlace("Tabletop Central", true);
        }


        public override void Update()
        {
            UpdateDynamics();
            UpdateInteractions();
        }

        private void UpdateDynamics()
        {
            // Calculate target pos / rot
            float time = (Time.time - timeOffset);
            float spinTime = (Time.time - spinTimeOffset);
            app.book.movementLerpSpeed = movementLerpSpeed;
            if (matchmakingStatus != 0)
            {
                app.book.SetPlace("Fire Loading");
                if (app.book.inPosition && matchmakingStatus == 1) StartMatchmaking();
                if (matchmakingStatus >= 1) app.book.targetRotOffset = Quaternion.AngleAxis(0.2f * spinTime * (360), app.book.transform.right);
                app.fire.brightness = 0.2f;
            }
            else if (app.book.isHovered)
            {
                app.book.SetPlace("Tabletop Central");
                app.book.targetPosOffset = hoverPosOffset;
                app.book.targetPosOffset += new Vector3(0.0f, Mathf.Sin(time / floatDuration * Mathf.PI * 2f) * floatMagnitude, 0.0f);
                app.book.targetRotOffset = hoverRotOffset;
                app.fire.brightness = 0.25f;
            } else
            {
                app.book.SetPlace("Tabletop Central");
                app.book.targetPosOffset = Vector3.zero;
                app.book.targetRotOffset = Quaternion.identity;
                app.fire.brightness = 0.0f;
            }

            // Set states
            app.book.toOpen = false;
        }

        private void UpdateInteractions()
        {
            // Disconnect on space
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!app.multiplayerManager.isConnected && !app.multiplayerManager.isConnecting) matchmakingStatus = 0;
                else app.multiplayerManager.Disconnect();
            }

            // Start matchmaking if clicked when in position
            if (matchmakingStatus == 0 && app.book.isHovered && Input.GetMouseButtonDown(0))
            {
                spinTimeOffset = Time.time;
                matchmakingStatus = 1;
            }
        }


        private void StartMatchmaking()
        {
            // Send off the initial connection request
            if (!MultiplayerManager.instance.isConnected)
            {
                Debug.Log("BOOK: Trying to connect...");
                matchmakingStatus = 2;
                if (!MultiplayerManager.instance.TryConnect())
                {
                    Debug.Log("BOOK: Could not start connecting");
                    matchmakingStatus = 0;
                }
            }

            // Already connected so find a match
            else
            {
                Debug.Log("BOOK: Finding a match immediately...");
                matchmakingStatus = 3;
                if (!Matchmaker.instance.TryFindMatch())
                {
                    Debug.Log("BOOK: Could not start match search");
                    matchmakingStatus = 0;
                }
            }
        }

        private void GotoNext() => app.GotoSelecting();


        public override void OnTryConnect(bool success)
        {
            if (matchmakingStatus != 2) return;

            // Connection failed so stop matchmaking
            Debug.Log("BOOK: Connection: " + success);
            if (!success) { matchmakingStatus = 0; return; }

            // Start searching for a match
            Debug.Log("BOOK: Finding a match...");
            matchmakingStatus = 3;
            if (!Matchmaker.instance.TryFindMatch())
            {
                Debug.Log("BOOK: Could not start match search");
                matchmakingStatus = 0;
            }
        }

        public override void OnDisconnect()
        {
            if (matchmakingStatus == 0) return;

            // Disconnected so stop matchmaking
            Debug.Log("BOOK: Disconnected from server");
            matchmakingStatus = 0;
        }

        public override void OnFindMatch(bool success)
        {
            if (matchmakingStatus == 0) return;

            // If failed finding a match then exit out
            Debug.Log("BOOK: Match: " + success);
            if (!success) { matchmakingStatus = 0; return; }

            // Found a match so continue
            GotoNext();
        }

        public override void OnCloseMatch()
        {
            if (matchmakingStatus == 0) return;

            // Disconnected from match so stop matchmaking
            Debug.Log("BOOK: Disconnected from match");
            matchmakingStatus = 0;
        }
    }

    [Serializable]
    public class AppStateSelecting : AppState
    {
        // Declare variables
        [Header("Config")]
        [SerializeField] private float movementLerpSpeed = 3.5f;
        [SerializeField] private float floatDuration = 3f;
        [SerializeField] private float floatMagnitude = 0.25f;
        private bool initialSet;
        private float timeOffset;


        public override void Set()
        {
            // Initialize book state
            app.cameraController.SetView("Default");
            initialSet = false;
            timeOffset = Time.time;
        }

        public override void Unset()
        {
            // Uninitialize book state
            app.classSelect.SetActive(false);
            initialSet = false;
        }


        public override void Update()
        {
            // Update state
            app.classSelect.SetActive(initialSet);
            app.book.movementLerpSpeed = movementLerpSpeed;

            // Handle logic for before class select is setup
            if (!app.classSelect.isSetup)
            {
                // Update variables
                initialSet |= app.book.inPosition;
                app.fire.brightness = 0.8f;
                app.book.toOpen = true;
                app.book.SetPlace("Fire Class Select");
                app.book.targetPosOffset = new Vector3(0.0f, 0.8f, 0.0f);
            }

            // Handle logic for once class select is setup
            else
            {
                ClassSelectOption selectedOption = app.classSelect.GetSelectedOption();

                // Show current tokens information
                if (selectedOption != null)
                {
                    float time = (Time.time - timeOffset);
                    app.fire.brightness = 0.7f;
                    app.book.toOpen = true;
                    app.book.SetPlace("Fire Class View");
                    app.book.targetPosOffset = new Vector3(0.0f, Mathf.Sin(time / floatDuration * Mathf.PI * 2f) * floatMagnitude, 0.0f);
                    app.book.SetContentTitle(selectedOption.optionClass.className);
                    app.book.SetContentDescription(selectedOption.optionClass.classDescription);
                    app.cameraController.SetView("Fire Class View");
                }

                // Close book and set to default view
                else
                {
                    app.fire.brightness = 0.4f;
                    app.book.toOpen = false;
                    app.book.SetPlace("Fire Class Select");
                    app.book.targetPosOffset = Vector3.zero;
                    app.cameraController.SetView("Default");
                }

                // Try start game on click token
                if (selectedOption != null && selectedOption.GetClicked()) GotoNext(selectedOption);
            }

            // Leave on space
            if (Input.GetKeyDown(KeyCode.Space)) app.multiplayerManager.TryLeaveMatchmaking();
        }


        private void GotoNext(ClassSelectOption option)
        {
            // Start GameManager and goto ingame state
            app.stateIngame.chosenClass = option.optionClass;
            app.classSelect.SetActive(false);
            app.GotoIngame();
        }


        public override void OnDisconnect() => Reset();
        
        public override void OnCloseMatch() => Reset();


        private void Reset() => app.GotoMatchmaker();
    }

    [Serializable]
    public class AppStateIngame : AppState
    {
        // Declare variables
        [SerializeField] private float movementLerpSpeed = 4f;
        private bool cornerHovered;
        public ClassData chosenClass;


        public override void Set()
        {
            // Start the game
            app.gameManager.EnterGame(chosenClass, this);
        }


        public override void OnDisconnect() => ExitGame();
        public override void OnCloseMatch() => ExitGame();

        private void ExitGame()
        {
            // Game finished
            app.gameManager.ExitGame();
            app.GotoMatchmaker();
        }
    }


    private void OnTryConnect(bool success) => state.OnTryConnect(success);
    private void OnDisconnect() => state.OnDisconnect();
    private void OnFindMatch(bool success) => state.OnFindMatch(success);
    private void OnCloseMatch() => state.OnCloseMatch();

    #endregion


    public void GotoFirstState()
    {
        GotoMatchmaker();
        stateMatchmaker.SetToBase();
    }

    [ContextMenu("Goto Matchmaker")]
    private void GotoMatchmaker() => SetAppState(stateMatchmaker);
    [ContextMenu("Goto Selecting")]
    private void GotoSelecting() => SetAppState(stateSelecting);
    [ContextMenu("Goto Ingame")]
    private void GotoIngame() => SetAppState(stateIngame);


    private void SetAppState(AppState state_)
    {
        if (state != null) state.Unset();
        state = state_;
        state.Set();
    }
}

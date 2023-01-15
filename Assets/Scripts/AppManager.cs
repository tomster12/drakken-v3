
using System;
using UnityEngine;


public class AppManager : MonoBehaviour
{
    public static AppManager instance { get; private set; }

    [Header("References")]
    [SerializeField] private MultiplayerManager multiplayerManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Fire fire;
    [SerializeField] private Book book;

    private AppState state;
    [SerializeField] private AppStateMenu stateMenu;
    [SerializeField] private AppStateSettings stateSettings;
    [SerializeField] private AppStateMatchmaker stateMatchmaker;
    [SerializeField] private AppStateClassSelect stateClassSelect;
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
        multiplayerManager.Init();
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
    public class AppStateMenu : AppState
    {
        [Header("References")]
        [SerializeField] private Menu menu;

        [Header("Config")]
        [SerializeField] private Vector3 bookHoverPosOffset = new Vector3(0.0f, 2f, 0.0f);
        [SerializeField] private Quaternion bookHoverRotOffset = Quaternion.Euler(-10f, 0.0f, 0.0f);
        [SerializeField] private Vector3 bookOpeningPosOffset = new Vector3(0.0f, 3f, 0.0f);
        [SerializeField] private Quaternion bookOpeningRotOffset = Quaternion.Euler(0.0f, 0.0f, -17f);
        [SerializeField] private Vector3 bookSelectingPosOffset = new Vector3(0.0f, 1f, 0.0f);
        [SerializeField] private string camPlace = "Tabletop Default";
        [SerializeField] private string camPlaceSelected = "Floor Close Up";
        [SerializeField] private string bookPlace = "Tabletop Central";
        [SerializeField] private float bookWaveDuration = 1.65f;
        [SerializeField] private float bookWaveMagnitude = 0.25f;
        [SerializeField] private float bookHoverLerpSpeed = 3.0f;
        [SerializeField] private float bookOpeningPosLerpSpeed = 5f;
        [SerializeField] private float bookOpeningRotLerpSpeed = 10.0f;
        [SerializeField] private float bookSelectingPosLerpSpeed = 6.0f;
        [SerializeField] private float bookSelectingRotLerpSpeed = 15.0f;
        [SerializeField] private float bookInPositionThreshold = 0.2f;
        [SerializeField] private float bookGlowLerpSpeed = 3.5f;
        [SerializeField] private float fireBrightnessOpening = 0.3f;
        [SerializeField] private float fireBrightnessSelecting = 0.1f;
        [SerializeField] private float fireBrightnessHovering = 0.25f;
        [SerializeField] private float fireBrightnessSelected = 0.35f;
        [SerializeField] private float bookToOpenLerpSpeed = 1.5f;
        [SerializeField] private float bookOpeningToOpenThreshold = 0.8f;

        private bool hoverCheck = false;
        private bool hasStarted = false;
        private bool bookInitialSet;
        private float bookTimeOffset;


        public override void Set()
        {
            // Initialize book state
            app.cameraController.placeLerper.SetPlace(camPlace);
            app.book.placeLerper.SetPlace(bookPlace);
            app.book.placeLerper.inPositionThreshold = bookInPositionThreshold;
            app.book.toOpen = false;
            app.book.outlineAmount = 0.0f;
            app.book.glowAmount = 0.0f;
            app.fire.brightness = 0.0f;
            menu.SetActive(false);
            menu.onClick += ClickOption;
            menu.onSelect += SelectOption;
            bookInitialSet = false;
            bookTimeOffset = Time.time;
        }

        public void SetToBase()
        {
            // Set to base values
            app.book.placeLerper.SetPlace(bookPlace, true);
            app.book.glowLerpSpeed = bookGlowLerpSpeed;
            app.fire.SetValues();
        }

        public override void Unset()
        {
            app.book.targetPct = 1.0f;
            app.book.openLerpSpeed = 3.0f;
            menu.SetActive(false);
            menu.onClick -= ClickOption;
            menu.onSelect -= SelectOption;
        }


        public override void Update()
        {
            float time = Time.time - bookTimeOffset;

            if (!hasStarted)
            {
                app.cameraController.placeLerper.SetPlace(camPlace);
                app.book.placeLerper.positionlerpSpeed = bookHoverLerpSpeed;
                app.fire.brightness = 0.0f;
                if (app.book.isHovered)
                {
                    if (!hoverCheck)
                    {
                        hoverCheck = true;
                        bookTimeOffset = Time.time;
                    }
                    app.book.placeLerper.SetOffsetPosition(bookHoverPosOffset + new Vector3(0.0f, Mathf.Sin(time / bookWaveDuration * Mathf.PI * 2f) * bookWaveMagnitude, 0.0f));
                    app.book.placeLerper.SetOffsetRotation(bookHoverRotOffset);
                    app.book.outlineAmount = 1.0f;
                    app.book.glowAmount = 0.7f;
                    if (Input.GetMouseButtonDown(0)) hasStarted = true;
                }
                else
                {
                    hoverCheck = false;
                    app.book.placeLerper.SetOffsetPosition(Vector3.zero);
                    app.book.placeLerper.SetOffsetRotation(Quaternion.identity);
                    app.book.outlineAmount = 0.0f;
                    app.book.glowAmount = 0.0f;
                }
            }
            else
            {
                app.book.outlineAmount = 0.0f;
                app.cameraController.placeLerper.SetPlace(camPlaceSelected);
                if (!bookInitialSet)
                {
                    app.book.placeLerper.positionlerpSpeed = bookOpeningPosLerpSpeed;
                    app.book.placeLerper.rotationLerpSpeed = bookOpeningRotLerpSpeed;
                    app.book.placeLerper.SetOffsetPosition(bookOpeningPosOffset);
                    app.book.placeLerper.SetOffsetRotation(bookOpeningRotOffset);
                    app.book.targetPct = 0.9f;
                    app.book.glowAmount = 0.9f;
                    app.book.openLerpSpeed = bookToOpenLerpSpeed;
                    app.book.toOpen = true;
                    app.fire.brightness = fireBrightnessOpening;
                    bookInitialSet |= app.book.placeLerper.inPosition && (app.book.normalizedPct > bookOpeningToOpenThreshold);
                    if (bookInitialSet) menu.SetActive(true);
                }
                else
                {
                    app.book.placeLerper.positionlerpSpeed = bookSelectingPosLerpSpeed;
                    app.book.placeLerper.rotationLerpSpeed = bookSelectingRotLerpSpeed;
                    app.book.placeLerper.SetOffsetPosition(bookSelectingPosOffset);
                    app.book.placeLerper.SetOffsetRotation(Quaternion.identity);
                    app.book.targetPct = 1.0f;
                    app.book.glowAmount = 0.0f;
                    app.fire.brightness = fireBrightnessSelecting;
                    if (menu.state == Menu.State.SELECTING) app.fire.brightness = fireBrightnessSelecting;
                    if (menu.selectedIndex != -1) app.fire.brightness = fireBrightnessSelected;
                    else if (menu.hoveredIndex != -1) app.fire.brightness = fireBrightnessHovering;
                }
            }
        }


        private void ClickOption(int index)
        {
            // Quit Button
            if (index == 2)
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #endif
                Application.Quit();
            }
        }

        private void SelectOption(int index)
        {
            // Play / Settings buttons
            if (index == 0) app.GotoMatchmaker();
            else if (index == 1) app.GotoSettings();
        }
    }

    [Serializable]
    public class AppStateSettings : AppState
    {
        [Header("References")]
        [SerializeField] private Settings settings;

        [Header("Config")]
        [SerializeField] private float fireBrightnessIdle = 0.05f;
        [SerializeField] private float fireBrightnessHover = 0.18f;
        [SerializeField] private float movingBookOpenLerpSpeed = 4.0f;
        [SerializeField] private float readyBookOpenLerpSpeed = 2.0f;
        [SerializeField] private float bookMovementLerpSpeed = 5.0f;
        [SerializeField] private string camPlace = "Settings";
        [SerializeField] private string bookPlace = "Fire Class Select";


        public override void Set()
        {
            // Initialize book state
            app.cameraController.placeLerper.SetPlace(camPlace);
            app.book.placeLerper.SetPlace(bookPlace);
            app.book.glowAmount = 0.0f;
            app.book.placeLerper.positionlerpSpeed = bookMovementLerpSpeed;
            app.book.toOpen = false;
            app.fire.brightness = fireBrightnessIdle;
            settings.SetActive(false);
        }

        public override void Unset()
        {
            app.book.glowAmount = 0.0f;
            settings.SetActive(false);
        }


        public override void Update()
        {
            // Update book and settings
            app.book.toOpen = app.book.placeLerper.inPosition;
            if (!app.book.toOpen) app.book.openLerpSpeed = movingBookOpenLerpSpeed;
            else app.book.openLerpSpeed = readyBookOpenLerpSpeed;
            bool ready = app.book.toOpen && app.book.normalizedPct > 0.5f;
            settings.SetActive(ready);

            // Go back once fizzled
            if (settings.hasExited) app.GotoMenu();
        }
    }

    [Serializable]
    public class AppStateMatchmaker : AppState
    {
        [Header("Config")]
        [SerializeField] private Vector3 bookHoverPosOffset = new Vector3(0.0f, 2f, 0.0f);
        [SerializeField] private Quaternion bookHoverRotOffset = Quaternion.Euler(-10f, 0.0f, 0.0f);
        [SerializeField] private float bookFloatDuration = 1.65f;
        [SerializeField] private float bookFloatMagnitude = 0.25f;
        [SerializeField] private float bookMovementLerpSpeed = 4.5f;
        [SerializeField] private float fireBrightnessIdle = 0.05f;
        [SerializeField] private float fireBrightnessHover = 0.18f;
        [SerializeField] private float fireBrightnessLoading = 0.28f;
        [SerializeField] private string camPlace = "Default";
        [SerializeField] private string bookPlaceWait = "Tabletop Central";
        [SerializeField] private string bookPlaceLoading = "Fire Loading";
        private float bookTimeOffset;
        private float bookSpinTimeOffset;
        private int matchmakingStatus = 0;


        public override void Set()
        {
            // Initialize book state
            app.cameraController.placeLerper.SetPlace(camPlace);
            app.book.placeLerper.SetPlace(bookPlaceWait);
            bookTimeOffset = Time.time;
            matchmakingStatus = 0;
            app.fire.brightness = 0.0f;
        }

        public void SetToBase()
        {
            // Set to base values
            app.book.placeLerper.SetPlace(bookPlaceWait, true);
            app.fire.SetValues();
        }

        public override void Unset()
        {
            app.book.glowAmount = 0.0f;
        }


        public override void Update()
        {
            UpdateDynamics();
            UpdateInteractions();
        }

        private void UpdateDynamics()
        {
            // Calculate target pos / rot
            float time = (Time.time - bookTimeOffset);
            float spinTime = (Time.time - bookSpinTimeOffset);
            app.book.placeLerper.positionlerpSpeed = bookMovementLerpSpeed;
            if (matchmakingStatus != 0)
            {
                app.book.placeLerper.SetPlace(bookPlaceLoading);
                if (app.book.placeLerper.inPosition && matchmakingStatus == 1) StartMatchmaking();
                if (matchmakingStatus >= 1) app.book.placeLerper.SetOffsetRotation(Quaternion.AngleAxis(0.2f * spinTime * (360), app.book.transform.right));
                app.fire.brightness = fireBrightnessLoading;
                app.book.glowAmount = 0.0f;
            }
            else if (app.book.isHovered)
            {
                app.book.placeLerper.SetPlace(bookPlaceWait);
                app.book.placeLerper.SetOffsetPosition(bookHoverPosOffset + new Vector3(0.0f, Mathf.Sin(time / bookFloatDuration * Mathf.PI * 2f) * bookFloatMagnitude, 0.0f));
                app.book.placeLerper.SetOffsetRotation(bookHoverRotOffset);
                app.fire.brightness = fireBrightnessHover;
                app.book.glowAmount = 1.0f;
            }
            else
            {
                app.book.placeLerper.SetPlace(bookPlaceWait);
                app.book.placeLerper.SetOffsetPosition(Vector3.zero);
                app.book.placeLerper.SetOffsetRotation(Quaternion.identity);
                app.fire.brightness = fireBrightnessIdle;
                app.book.glowAmount = 0.0f;
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
                bookSpinTimeOffset = Time.time;
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

        private void GotoNext() => app.GotoClassSelect();


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
    public class AppStateClassSelect : AppState
    {
        [Header("References")]
        [SerializeField] private ClassSelect classSelect;

        [Header("Config")]
        [SerializeField] private float bookMovementLerpSpeed = 3.5f;
        [SerializeField] private float bookFloatDuration = 3f;
        [SerializeField] private float bookFloatMagnitude = 0.25f;
        [SerializeField] private float bookInPositionThreshold = 0.25f;
        [SerializeField] private float fireBrightnessSetup = 0.8f;
        [SerializeField] private float fireBrightnessSelected = 0.7f;
        [SerializeField] private float fireBrightnessSelecting = 0.35f;
        [SerializeField] private string camPlace = "Default";
        [SerializeField] private string camPlaceView = "Fire Class View";
        [SerializeField] private string bookPlaceSelect = "Fire Class Select";
        [SerializeField] private string bookPlaceView = "Fire Class View";
        [SerializeField] private VisualToken selectedOption;
        private bool inPositionSet;
        private bool hasOpenedSet;
        private float bookTimeOffset;

        [SerializeField] private Menu menu;

        public override void Set()
        {
            // Initialize book state
            app.cameraController.placeLerper.SetPlace(camPlace);
            inPositionSet = false;
            hasOpenedSet = false;
            bookTimeOffset = Time.time;
            classSelect.onSelectClass += SelectClass;
        }

        public override void Unset()
        {
            // Uninitialize book state
            classSelect.SetActive(false);
            inPositionSet = false;
            hasOpenedSet = false;
            classSelect.onSelectClass -= SelectClass;
        }


        public override void Update()
        {
            // Update state
            app.cameraController.placeLerper.SetPlace(camPlace);
            classSelect.SetActive(hasOpenedSet);    
            app.book.placeLerper.positionlerpSpeed = bookMovementLerpSpeed;

            // Handle logic for before class select is setup
            if (!classSelect.isSetup)
            {
                // Update variables
                app.fire.brightness = fireBrightnessSetup;
                app.book.placeLerper.inPositionThreshold = bookInPositionThreshold;
                app.book.placeLerper.SetPlace(bookPlaceSelect);
                app.book.placeLerper.SetOffsetPosition(new Vector3(0.0f, 0.8f, 0.0f));
                app.book.toOpen = inPositionSet;
                inPositionSet |= app.book.placeLerper.inPosition;
                hasOpenedSet |= inPositionSet && app.book.isOpen;
            }

            // Handle logic for once class select is setup
            else
            {
                selectedOption = classSelect.GetSelectedOption();

                // Show current tokens information
                if (selectedOption != null)
                {
                    float time = (Time.time - bookTimeOffset);
                    app.fire.brightness = fireBrightnessSelected;
                    app.book.toOpen = true;
                    app.book.placeLerper.SetPlace(bookPlaceView);
                    app.book.placeLerper.SetOffsetPosition(new Vector3(0.0f, Mathf.Sin(time / bookFloatDuration * Mathf.PI * 2f) * bookFloatMagnitude, 0.0f));
                    app.book.SetContentTitle(selectedOption.optionClass.className);
                    app.book.SetContentDescription(selectedOption.optionClass.classDescription);
                    app.cameraController.placeLerper.SetPlace(camPlaceView);
                }

                // Close book and set to default view
                else
                {
                    app.fire.brightness = fireBrightnessSelecting;
                    app.book.toOpen = false;
                    app.book.placeLerper.SetPlace(bookPlaceSelect);
                    app.book.placeLerper.SetOffsetPosition(Vector3.zero);
                    app.cameraController.placeLerper.SetPlace(camPlace);
                }
            }

            // Leave on space
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (app.multiplayerManager.isConnected || app.multiplayerManager.isConnecting) app.multiplayerManager.TryLeaveMatchmaking();
                else GotoPrev();
            }
        }


        private void SelectClass(ClassData selectedClass)
        {
            // Start GameManager and goto ingame state
            app.stateIngame.chosenClass = selectedClass;
            classSelect.SetActive(false);
            app.GotoIngame();
        }


        public override void OnDisconnect() => GotoPrev();
        
        public override void OnCloseMatch() => GotoPrev();


        private void GotoPrev() => app.GotoMatchmaker();
    }

    [Serializable]
    public class AppStateIngame : AppState
    {
        // Declare variables
        [SerializeField] private float bookMovementLerpSpeed = 4f;
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


    public void GotoFirstState() => GotoMenu();

    [ContextMenu("Goto Menu")]
    private void GotoMenu() => SetAppState(stateMenu);
    [ContextMenu("Goto Settings")]
    private void GotoSettings() => SetAppState(stateSettings);
    [ContextMenu("Goto Matchmaker")]
    private void GotoMatchmaker() => SetAppState(stateMatchmaker);
    [ContextMenu("Goto ClassSelect")]
    private void GotoClassSelect() => SetAppState(stateClassSelect);
    [ContextMenu("Goto Ingame")]
    private void GotoIngame() => SetAppState(stateIngame);


    private void SetAppState(AppState state_)
    {
        if (state != null) state.Unset();
        state = state_;
        state.Set();
    }
}

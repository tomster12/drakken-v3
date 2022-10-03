
using TMPro;
using System;
using UnityEngine;


public class Book : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MultiplayerManager multiplayerManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private ClassSelect classSelect;
    [SerializeField] private Fire fire;

    [SerializeField] private Animator animator;
    [SerializeReference] private BookState bookState;
    public bool isHovered { get; private set; }
    public bool toOpen { get; private set; }
    public bool isOpen { get; private set; }


    private void Awake()
    {
        // Add event listeners
        MultiplayerManager.OnDisconnect += OnDisconnect;
        Matchmaker.OnLeaveMatch += OnLeaveMatch;
    }


    private void Update()
    {
        if (bookState == null) return;

        // Update animator
        SetContentTitle("");
        SetContentDescription("");

        animator.SetBool("isOpen", toOpen);
        isOpen = toOpen && animator.GetCurrentAnimatorStateInfo(0).IsName("Open");

        // Update functions
        bookState.Update();
        UpdateUI();
    }


    #region States

    [Serializable] abstract public class BookState
    {
        protected Book book;

        public BookState(Book book_) { book = book_; }

        virtual public void Set() { }
        virtual public void Unset() { }

        abstract public void Update();

        virtual public void OnDisconnect() { }
        virtual public void OnLeaveMatch() { }
    }


    public class BookStateMatchmaker : BookState
    {
        [Header("Config")]
        [SerializeField] private Vector3 basePos = new Vector3(0f, 5.2f, 0f);
        [SerializeField] private Quaternion baseRot = Quaternion.Euler(-27f, 0f, 0f);
        [SerializeField] private Vector3 hoverPos = new Vector3(0f, 7f, 0f);
        [SerializeField] private Quaternion hoverRot = Quaternion.Euler(-12f, 0f, 0f);
        [SerializeField] private float floatDuration = 1.65f;
        [SerializeField] private float floatMagnitude = 0.25f;
        [SerializeField] private float movementLerpSpeed = 4.5f;
        private bool inPosition;
        private float timeOffset;
        private float spinTimeOffset;
        private bool isMatchmaking = false;


        public BookStateMatchmaker(Book book_) : base(book_) { }


        public override void Set()
        {
            // Initialize book state
            book.cameraController.SetView("Default");
            timeOffset = Time.time;
            inPosition = false;
        }

        public void SetToBase()
        {
            // Set to base positions
            book.transform.position = basePos;
            book.transform.rotation = baseRot;
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
            Vector3 targetPos = ((book.isHovered || isMatchmaking) && inPosition) ? hoverPos : basePos;
            Quaternion targetRot = ((book.isHovered || isMatchmaking) && inPosition) ? hoverRot : baseRot;
            if (inPosition && !book.isHovered && !isMatchmaking) targetPos.y += Mathf.Sin(time / floatDuration * Mathf.PI * 2f) * floatMagnitude;
            if (isMatchmaking) targetRot *= Quaternion.AngleAxis(0.2f * spinTime * (360), book.transform.right);

            // Lerp towards target
            book.transform.position = Vector3.Lerp(book.transform.position, targetPos, Time.deltaTime * movementLerpSpeed);
            book.transform.rotation = Quaternion.Lerp(book.transform.rotation, targetRot, Time.deltaTime * movementLerpSpeed);
            inPosition = inPosition || (targetPos - book.transform.position).magnitude < 0.1f;

            // Set states
            book.toOpen = false;
            book.fire.setBrightness(0);
        }

        private void UpdateInteractions()
        {
            // Disconnect on space
            if (Input.GetKeyDown(KeyCode.Space)) book.multiplayerManager.Disconnect();

            // Try connect if clicked when in position
            if ((!isMatchmaking && inPosition) && (book.isHovered && Input.GetMouseButtonDown(0)))
            {
                isMatchmaking = true;
                spinTimeOffset = Time.time;

                Debug.Log("Trying to connect...");
                book.multiplayerManager.TryConnect(delegate (bool isConnected)
                {
                    Debug.Log("Connection: " + isConnected);
                    if (!isMatchmaking || !isConnected) { isMatchmaking = false; return; }
                    book.multiplayerManager.FindMatch(delegate (bool hasMatch)
                    {
                        Debug.Log("Match: " + hasMatch);
                        if (!isMatchmaking || !hasMatch) { isMatchmaking = false; return; }
                        GotoNext();
                    });
                });
            }
        }


        private void GotoNext() => book.SetBookState(new BookStateOpening(book));


        public override void OnDisconnect() => isMatchmaking = false;
    }


    public class BookStateOpening : BookState
    {
        // Declare variables
        [Header("Config")]
        [SerializeField] private Vector3 basePos = new Vector3(0f, 4.5f, 0f);
        [SerializeField] private Quaternion baseRot = Quaternion.identity;
        [SerializeField] private float movementLerpSpeed = 6f;
        private bool inPosition;


        public BookStateOpening(Book book_) : base(book_) { }


        public override void Set() => book.cameraController.SetView("Default");


        public override void Update()
        {
            // Setup variables
            Vector3 targetPos;
            Quaternion targetRot;

            // Update states
            book.toOpen = true;
            book.fire.setBrightness(1);
            targetPos = basePos;
            targetRot = baseRot;
            
            // Lerp towards target
            book.transform.position = Vector3.Lerp(book.transform.position, targetPos, Time.deltaTime * movementLerpSpeed);
            book.transform.rotation = Quaternion.Lerp(book.transform.rotation, targetRot, Time.deltaTime * movementLerpSpeed);

            // Update state
            inPosition = (targetPos - book.transform.position).magnitude < 0.2f;
            book.classSelect.SetActive(inPosition);

            // Change state when in position
            if (book.classSelect.isSetup)
            {
                book.SetBookState(new BookStateSelecting(book));
                return;
            }
        }


        public override void OnDisconnect()
        {
            // Match disconnected
            book.classSelect.SetActive(false);
            book.SetBookState(new BookStateMatchmaker(book));
        }
    }


    public class BookStateSelecting : BookState
    {
        // Declare variables
        [Header("Config")]
        [SerializeField] private Vector3 basePos = new Vector3(0f, 3.5f, 0f);
        [SerializeField] private Quaternion baseRot = Quaternion.identity;
        [SerializeField] private Vector3 selectedPos = new Vector3(0f, 12f, 0f);
        [SerializeField] private Quaternion selectedRot = Quaternion.Euler(-50f, 0f, 0f);
        [SerializeField] private float movementLerpSpeed = 3.5f;
        [SerializeField] private float floatDuration = 3f;
        [SerializeField] private float floatMagnitude = 0.25f;
        private bool inPosition;
        private float timeOffset;


        public BookStateSelecting(Book book_) : base(book_) { }

        public override void Set()
        {
            // Initialize book state
            book.cameraController.SetView("Default");
            timeOffset = Time.time;
        }


        public override void Update()
        {
            // Setup variables
            Vector3 targetPos = basePos;
            Quaternion targetRot = baseRot;

            // Update state
            inPosition = (targetPos - book.transform.position).magnitude < 0.2f;
            ClassSelectOption selectedOption = book.classSelect.GetSelectedOption();

            // Show current tokens information
            if (selectedOption != null)
            {
                float time = (Time.time - timeOffset);
                book.fire.setBrightness(0.7f);
                book.toOpen = true;
                targetPos = selectedPos;
                targetRot = selectedRot;
                targetPos.y += Mathf.Sin(time / floatDuration * Mathf.PI * 2f) * floatMagnitude;
                book.SetContentTitle(selectedOption.GetClass().className);
                book.SetContentDescription(selectedOption.GetClass().classDescription);
                book.cameraController.SetView("Options Close");
            }


            // Close book and set to default view
            else
            {
                book.fire.setBrightness(0.35f);
                book.toOpen = false;
                book.cameraController.SetView("Default");
            }

            // Lerp towards target
            book.transform.position = Vector3.Lerp(book.transform.position, targetPos, Time.deltaTime * movementLerpSpeed);
            book.transform.rotation = Quaternion.Lerp(book.transform.rotation, targetRot, Time.deltaTime * movementLerpSpeed);

            // Try start game on click token
            if (selectedOption != null && selectedOption.GetClicked())
            {
                BookStateIngame ingameState = new BookStateIngame(book);
                book.gameManager.StartGame(selectedOption.GetClass(), ingameState);
                book.classSelect.SetActive(false);
                book.SetBookState(ingameState);
                return;
            }

            // Leave on space
            if (Input.GetKeyDown(KeyCode.Space)) book.multiplayerManager.Disconnect();
        }


        public override void OnDisconnect()
        {
            // Match disconnected
            book.classSelect.SetActive(false);
            book.SetBookState(new BookStateMatchmaker(book));
        }
    }


    public class BookStateIngame : BookState
    {
        // Declare variables
        [SerializeField] private float movementLerpSpeed = 4f;
        [SerializeField] private Vector3 cornerPos = new Vector3(-14f, 2.6f, -9f);
        [SerializeField] private Quaternion cornerRot = Quaternion.Euler(-27.2f, -27f, 0f);
        private bool inPosition;
        private bool cornerHovered;


        public BookStateIngame(Book book_) : base(book_) { }

        public override void Set() => book.cameraController.SetView("Default");


        public override void Update()
        {
            // Setup variables
            Vector3 targetPos = cornerPos;
            Quaternion targetRot = cornerRot;
            
            // Update state
            inPosition = (targetPos - book.transform.position).magnitude < 0.2f;
            book.toOpen = inPosition && (book.isHovered || cornerHovered);
            book.fire.setBrightness(0.7f);

            // Lerp towards target
            book.transform.position = Vector3.Lerp(book.transform.position, targetPos, Time.deltaTime * movementLerpSpeed);
            book.transform.rotation = Quaternion.Lerp(book.transform.rotation, targetRot, Time.deltaTime * movementLerpSpeed);

            // Leave on space
            if (Input.GetKeyDown(KeyCode.Space))
            {
                book.gameManager.StopGame();
                book.SetBookState(new BookStateOpening(book));
                return;
            }
        }


        public override void OnDisconnect()
        {
            // Match disconnected
            book.gameManager.UnloadGame();
            book.SetBookState(new BookStateMatchmaker(book));
        }


        // TODO: Think about sorting this out
        public void SetCornerHovered(bool cornerHovered_) => cornerHovered = cornerHovered_;
        public void SetContentTitle(String text_) => book.SetContentTitle(text_);
        public void SetContentDescription(String text_) => book.SetContentDescription(text_);
        public bool IsOpen() => book.toOpen;
    }


    private void OnDisconnect() => bookState.OnDisconnect();

    private void OnLeaveMatch() => bookState.OnLeaveMatch();

    #endregion


    #region Content

    // Declare variables
    [Header("Content References")]
    [SerializeField] private GameObject contentUI;
    [SerializeField] private TextMeshProUGUI contentTitleText;
    [SerializeField] private TextMeshProUGUI contentDescriptionText;


    private void UpdateUI()
    {
        // Enable / Disable UI
        contentUI.SetActive(isOpen);
    }


    public void SetContentTitle(String text_) => contentTitleText.text = text_;
    public void SetContentDescription(String text_) => contentDescriptionText.text = text_;

    #endregion


    public void SetBookState(BookState bookState_)
    {
        if (bookState != null) bookState.Unset();
        bookState = bookState_;
        bookState.Set();
    }


    private void OnMouseOver() => isHovered = true;

    private void OnMouseExit() => isHovered = false;
}

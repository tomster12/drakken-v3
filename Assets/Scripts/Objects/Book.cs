
using TMPro;
using System;
using UnityEngine;


public class Book : MonoBehaviour
{
    // Declare variables
    [Header("References")]
    [SerializeField] private AppManager appManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ClassSelect classSelect;
    [SerializeField] private Fire fire;

    [SerializeField] private Animator animator;
    [SerializeReference] private BookState bookState;
    private bool isHovered;
    private bool toOpen;
    private bool isOpen;
    private bool overrideToOpen;


    private void Update()
    {
        if (bookState == null) return;

        // Update animator
        SetContentTitle("");
        SetContentDescription("");

        animator.SetBool("isOpen", toOpen || overrideToOpen);
        isOpen = (toOpen || overrideToOpen) && animator.GetCurrentAnimatorStateInfo(0).IsName("Open");

        // Update functions
        bookState.Update();
        UpdateUI();

        // Reset override
        overrideToOpen = false;
    }


    #region States

    [Serializable] abstract public class BookState
    {
        protected Book book;

        public BookState(Book book_) { book = book_; }
        
        abstract public void Update();
        abstract public void OnDisconnect();
    }


    public class BookStateUnopened : BookState
    {
        // Declare variables
        [Header("Config")]
        [SerializeField] private Vector3 basePos = new Vector3(0f, 5.2f, 0f);
        [SerializeField] private Quaternion baseRot = Quaternion.Euler(-27f, 0f, 0f);
        [SerializeField] private Vector3 hoverPos = new Vector3(0f, 7f, 0f);
        [SerializeField] private Quaternion hoverRot = Quaternion.Euler(-12f, 0f, 0f);
        [SerializeField] private float floatDuration = 1.65f;
        [SerializeField] private float floatMagnitude = 0.25f;
        [SerializeField] private float movementLerpSpeed = 4.5f;
        private bool isReady;
        private float timeOffset;
        private float spinTimeOffset;
        private bool isConnecting = false;


        public BookStateUnopened(Book book_, bool set = true) : base(book_)
        {
            // Initialize state
            book.cameraController.SetView("Default");
            timeOffset = Time.time;
            isReady = false;

            // Set to base positions
            if (set)
            {
                book.transform.position = basePos;
                book.transform.rotation = baseRot;
            }
        }


        public override void Update()
        {
            // Calculate target pos / rot
            float time = (Time.time - timeOffset);
            float spinTime = (Time.time - spinTimeOffset);
            Vector3 targetPos = ((book.isHovered || isConnecting) && isReady) ? hoverPos : basePos;
            Quaternion targetRot = ((book.isHovered || isConnecting) && isReady) ? hoverRot : baseRot;
            if (isReady && !book.isHovered && !isConnecting) targetPos.y += Mathf.Sin(time / floatDuration * Mathf.PI * 2f) * floatMagnitude;
            if (isConnecting) targetRot *= Quaternion.AngleAxis(0.2f * spinTime * (360), book.transform.right);

            // Lerp towards target
            book.transform.position = Vector3.Lerp(book.transform.position, targetPos, Time.deltaTime * movementLerpSpeed);
            book.transform.rotation = Quaternion.Lerp(book.transform.rotation, targetRot, Time.deltaTime * movementLerpSpeed);

            // Set states
            isReady = isReady || (targetPos - book.transform.position).magnitude < 0.1f;
            book.toOpen = false;
            book.fire.setBrightness(0);

            // Open when clicked
            if (!isConnecting && isReady && book.isHovered && Input.GetMouseButtonDown(0))
            {
                isConnecting = true;
                spinTimeOffset = Time.time;
                book.appManager.TryConnect(OnConnectionStatus);
            }

            // Disconnect on space
            if (Input.GetKeyDown(KeyCode.Space)) book.appManager.Disconnect();
        }


        private void OnConnectionStatus(int status)
        {
            // Could not connect
            if (status == 0)
            {
                isConnecting = false;

            // Connected and waiting for match
            } else if (status == 1)
            {

            // Connected and found match
            } else if (status == 2)
            {
                // Transition to opening book
                book.SetBookState(new BookStateOpening(book));
            }
        }


        public override void OnDisconnect()
        {
            // Match disconnected
            isConnecting = false;
        }
    }


    public class BookStateOpening : BookState
    {
        // Declare variables
        [Header("Config")]
        [SerializeField] private Vector3 basePos = new Vector3(0f, 4.5f, 0f);
        [SerializeField] private Quaternion baseRot = Quaternion.identity;
        [SerializeField] private float movementLerpSpeed = 6f;
        private bool inPosition;


        public BookStateOpening(Book book_) : base(book_)
        {
            // Initialize state
            book.cameraController.SetView("Default");
        }


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
            book.classSelect.SetActive(inPosition);

            // Update state
            inPosition = (targetPos - book.transform.position).magnitude < 0.2f;

            // Lerp towards target
            book.transform.position = Vector3.Lerp(book.transform.position, targetPos, Time.deltaTime * movementLerpSpeed);
            book.transform.rotation = Quaternion.Lerp(book.transform.rotation, targetRot, Time.deltaTime * movementLerpSpeed);

            // Change state when in position
            if (book.classSelect.GetSetup())
            {
                book.SetBookState(new BookStateSelecting(book));
                return;
            }
        }


        public override void OnDisconnect()
        {
            // Match disconnected
            book.classSelect.SetActive(false);
            book.SetBookState(new BookStateUnopened(book, false));
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


        public BookStateSelecting(Book book_) : base(book_)
        {
            // Initialize state
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
            if (Input.GetKeyDown(KeyCode.Space)) book.appManager.Disconnect();
        }


        public override void OnDisconnect()
        {
            // Match disconnected
            book.classSelect.SetActive(false);
            book.SetBookState(new BookStateUnopened(book, false));
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


        public BookStateIngame(Book book_) : base(book_)
        {
            // Initialize state
            book.cameraController.SetView("Default");
        }


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
            book.SetBookState(new BookStateUnopened(book, false));
        }


        public void SetCornerHovered(bool cornerHovered_) => cornerHovered = cornerHovered_;
        public void SetContentTitle(String text_) => book.SetContentTitle(text_);
        public void SetContentDescription(String text_) => book.SetContentDescription(text_);
        public bool IsOpen() => book.toOpen;
    }


    public void OnDisconnect() => bookState.OnDisconnect();

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


    // Getters / Setters
    public void SetBookState(BookState bookState_) => bookState = bookState_;
    public bool SetOpen(bool toOpen_) => overrideToOpen = toOpen_;
    public bool GetHovered() => isHovered;
    public bool GetToOpen() => toOpen;


    // Handle hovering
    private void OnMouseOver() => isHovered = true;
    private void OnMouseExit() => isHovered = false;
}

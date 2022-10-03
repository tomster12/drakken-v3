    
using UnityEngine;


public class AppManager : MonoBehaviour
{
    // Declare variables, references
    public static AppManager instance;

    [Header("References")]
    [SerializeField] public MultiplayerManager multiplayerManager;
    [SerializeField] public Book book;


    private void Awake()
    {
        // Singleton handling
        if (instance != null) return;
        instance = this;

        // Main initialization
        multiplayerManager.Init(Application.dataPath + "\\config.cfg");
        if (multiplayerManager.isServer) return;

        // Book initialization
        Book.BookStateMatchmaker s = new Book.BookStateMatchmaker(book);
        s.SetToBase();
        book.SetBookState(s);
    }
}

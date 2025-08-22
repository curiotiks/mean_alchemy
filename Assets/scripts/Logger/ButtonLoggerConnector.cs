using UnityEngine;
using UnityEngine.UI;

public class ButtonLoggerConnector : MonoBehaviour
{
    [Header("Catalog Reference")]
    [Tooltip("Catalog that supplies the Category->Event options for the dropdown below")] 
    public EventPayloadCatalog catalog;

    [Tooltip("Pick the Category and Event key to log")] 
    public EventRef eventRef;

    [Header("Logging Modes")]
    [Tooltip("If true, clicking a Button on this GameObject will log the payload.")]
    public bool logOnButtonClick = true;

    [Tooltip("If true, collisions/triggers with the Player will log the payload.")]
    public bool logOnCollision = true;

    [Tooltip("Only collisions with objects tagged as this will be logged. Leave as 'Player' for default setups.")]
    public string playerTag = "Player";

    private GameLogger logger;
    private Button button;

    private void OnValidate()
    {
        // Auto-assign the catalog in the editor when possible
        if (catalog == null)
        {
            var gl = FindObjectOfType<GameLogger>();
            if (gl != null && gl.catalog != null)
            {
                catalog = gl.catalog;
            }
            else
            {
                // Fallback to Resources
                var fallback = Resources.Load<EventPayloadCatalog>("EventPayloadCatalog");
                if (fallback != null)
                {
                    catalog = fallback;
                }
            }
        }
    }

    void Awake()
    {
        button = GetComponent<Button>();
    }

    void Start()
    {
        TryFindLogger();

        if (catalog == null && logger != null && logger.catalog != null)
        {
            catalog = logger.catalog;
        }
        else if (catalog == null)
        {
            var fallback = Resources.Load<EventPayloadCatalog>("EventPayloadCatalog");
            if (fallback != null) catalog = fallback;
        }

        // Wire up button click logging
        if (logOnButtonClick && button != null)
        {
            if (logger != null)
            {
                if (catalog == null)
                {
                    Debug.LogWarning($"⚠️ {name}: No catalog assigned. The EventRef dropdown may be empty; attempting Resources fallback in GameLogger.");
                }
                button.onClick.AddListener(() => logger.LogEvent(eventRef));
            }
            else
            {
                Debug.LogError("❌ GameLogger not found. Make sure it exists in the scene.");
            }
        }

        // Warn if collision mode is enabled but no collider is present
        if (logOnCollision && GetComponent<Collider2D>() == null && GetComponent<Collider>() == null)
        {
            Debug.LogWarning($"⚠️ {name}: logOnCollision is enabled but no Collider/Collider2D is attached.");
        }
    }

    private void TryFindLogger()
    {
        if (logger == null)
            logger = FindObjectOfType<GameLogger>();
    }

    // ================= 2D Physics =================
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!logOnCollision) return;
        if (!IsPlayer(collision.collider)) return;
        TryFindLogger();
        if (logger != null) logger.LogEvent(eventRef);
        else Debug.LogError("❌ GameLogger not found. Make sure it exists in the scene.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!logOnCollision) return;
        if (!IsPlayer(other)) return;
        TryFindLogger();
        if (logger != null) logger.LogEvent(eventRef);
        else Debug.LogError("❌ GameLogger not found. Make sure it exists in the scene.");
    }

    // ================= 3D Physics =================
    void OnCollisionEnter(Collision collision)
    {
        if (!logOnCollision) return;
        if (!IsPlayer(collision.collider)) return;
        TryFindLogger();
        if (logger != null) logger.LogEvent(eventRef);
        else Debug.LogError("❌ GameLogger not found. Make sure it exists in the scene.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!logOnCollision) return;
        if (!IsPlayer(other)) return;
        TryFindLogger();
        if (logger != null) logger.LogEvent(eventRef);
        else Debug.LogError("❌ GameLogger not found. Make sure it exists in the scene.");
    }

    private bool IsPlayer(Component col)
    {
        if (col == null) return false;
        if (string.IsNullOrEmpty(playerTag)) return true; // no filter
        return col.CompareTag(playerTag) || col.gameObject.CompareTag(playerTag);
    }
}
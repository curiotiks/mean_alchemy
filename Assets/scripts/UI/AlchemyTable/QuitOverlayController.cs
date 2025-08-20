// Assets/Scripts/UI/QuitOverlayController.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuitOverlayController : MonoBehaviour
{
    [Header("Buttons (children of this overlay)")]
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    
    // Enum-backed dropdown in Inspector; avoids hardcoded scene name strings
    public enum QuitTarget { AlchemyTable, TheLab, BountyBoard, CombatArena, WorldMap }

    [Header("Destination")]
    [SerializeField] private QuitTarget destination = QuitTarget.AlchemyTable;

    void Awake()
    {
        // Auto-wire if left empty (expects children named "YesButton" / "NoButton")
        if (!yesButton)  yesButton = transform.Find("YesButton")?.GetComponent<Button>();
        if (!noButton)   noButton  = transform.Find("NoButton")?.GetComponent<Button>();

        if (yesButton) yesButton.onClick.AddListener(OnYes);
        if (noButton)  noButton.onClick.AddListener(OnNo);

        // Start hidden when the scene loads (Awake runs even if this GO is active)
        gameObject.SetActive(false);
    }

    // Call this from your main Quit button
    public void Open()  => gameObject.SetActive(true);

    // Hides the overlay (used by the No button)
    public void Close() => gameObject.SetActive(false);

    private void OnNo()  => Close();

    private void OnYes()
    {
        SceneManager.LoadScene(ResolveSceneName());
    }

    private string ResolveSceneName()
    {
        switch (destination)
        {
            case QuitTarget.AlchemyTable: return SceneNames.AlchemyTable;
            case QuitTarget.TheLab:       return SceneNames.TheLab;
            case QuitTarget.BountyBoard:  return SceneNames.BountyBoard;
            case QuitTarget.CombatArena:  return SceneNames.CombatArena;
            case QuitTarget.WorldMap:     return SceneNames.WorldMap;
            default:                      return SceneNames.AlchemyTable;
        }
    }
}
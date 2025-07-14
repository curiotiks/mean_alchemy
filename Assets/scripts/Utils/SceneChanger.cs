using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] public string destScene;
    

    private void Start()
    {
        // Check if this object has a Button component
        if (TryGetComponent(out Button button))
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnTrigger);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            OnTrigger();
        }
    }

    private void OnTrigger()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        string selectedScene = destScene.ToString();
        
        if (currentScene == selectedScene)
        {
            Debug.LogError($"SceneChanger: You're trying to send the player to the same scene. Change the drop-down in inspector...");
            return;
        }

        SceneManager.LoadScene(destScene);
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] public string destScene;
    [SerializeField] public SpawnLocation spawnLocation;
    

    private void Start()
    {
        // Check if this object has a Button component
        // If it's a button, then add script to button and 
        // add the button to itself as an onClick() listener and select the 
        // relevant function. 
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

        // This was to check that the GameManager is instantiated BEFORE anything else
        if (GameManager.instance == null || GameManager.instance.userInfo == null)
        {
            Debug.LogError("GameManager or userInfo is null");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        string selectedScene = destScene.ToString();
        
        // Check that the current scene is not the same as the selected scene
        if (currentScene == selectedScene)
        {
            Debug.LogError($"SceneChanger: You're trying to send the player to the same scene. Change the drop-down in inspector...");
            return;
        }

        // Only When leaving lab, set the spawn location
        if (selectedScene != SceneNames.TheLab)
        {
            GameManager.instance.userInfo.lastSpawnLocation = spawnLocation;
            Debug.Log($"[SceneChanger] Setting spawn location to {spawnLocation} before entering {selectedScene}");
        }
        
        // Load into the destination scene.
        SceneManager.LoadScene(destScene);
    }
} 
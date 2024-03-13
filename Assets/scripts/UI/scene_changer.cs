using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TrollBridge;

public class scene_changer : MonoBehaviour
{
    [SerializeField] private string destScene;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            //when the current scene is world_map_A1, set entry_trigger_name
            if (SceneManager.GetActiveScene().name == "world_map_A1")
            {
                GameObject globalGameManager = GameObject.Find("GlobalGameManager");
                globalGameManager.GetComponent<Dont_Destroy_On_Scene_Load>().setEntryTriggerName(gameObject.name);
            }

            SceneManager.LoadScene(destScene);
            Debug.Log("Scene Triggered");

            //after the scene is loaded, the player will be moved to the original position


            //only when current scene name is not world_map_A1
            if (SceneManager.GetActiveScene().name != "world_map_A1")
                SceneManager.sceneLoaded += OnSceneLoaded;

        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject globalGameManager = GameObject.Find("GlobalGameManager");
        //if the avatar came from world_map_A1, move the player to the original position through the entry_trigger_name
        if (globalGameManager.GetComponent<Dont_Destroy_On_Scene_Load>().entry_trigger_name == null)
        {
            Debug.Log("entry_trigger_name is null");
            return;
        }

        string entry_trigger_name = globalGameManager.GetComponent<Dont_Destroy_On_Scene_Load>().entry_trigger_name;
        if (entry_trigger_name == "gateway_entry_trigger")
        {
            GameObject entryTrigger = GameObject.Find(globalGameManager.GetComponent<Dont_Destroy_On_Scene_Load>().entry_trigger_name);
            globalGameManager.GetComponent<Dont_Destroy_On_Scene_Load>().movePlayerToPosition(entryTrigger.transform.position - new Vector3(0, 2, 0));
        }
        else if (entry_trigger_name == "lab_gate_entry_trigger")
        {
            GameObject entryTrigger = GameObject.Find(globalGameManager.GetComponent<Dont_Destroy_On_Scene_Load>().entry_trigger_name);
            globalGameManager.GetComponent<Dont_Destroy_On_Scene_Load>().movePlayerToPosition(entryTrigger.transform.position - new Vector3(0, 2, 0));
        }

        Debug.Log("Scene Loaded: " + scene.name);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

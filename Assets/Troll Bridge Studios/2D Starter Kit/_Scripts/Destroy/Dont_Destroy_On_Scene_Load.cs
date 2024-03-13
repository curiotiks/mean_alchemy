using UnityEngine;
using System.Collections;

namespace TrollBridge
{

    public class Dont_Destroy_On_Scene_Load : MonoBehaviour
    {
        public Vector2 playerPosition;
        public string entry_trigger_name = null;

        void Awake()
        {
            //find GlobalGameManager with the tag
            GameObject[] globalGameManager = GameObject.FindGameObjectsWithTag("GlobalGameManager");
            if (globalGameManager.Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }

        // public void SetPlayerPosition(Vector2 position)
        // {
        //     playerPosition = position;
        // }

        public void movePlayerToPosition(Vector2 position)
        {
            GameObject player = GameObject.Find("Player Manager");
            player.transform.position = position;
        }

        public void setEntryTriggerName(string name)
        {
            entry_trigger_name = name;
        }
    }
}

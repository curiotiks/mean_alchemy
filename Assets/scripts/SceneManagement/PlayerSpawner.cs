using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public Transform defaultSpawn;
    public Transform bountyBoardExitSpawn;
    public Transform alchemyExitSpawn;

    void Start()
    {
        if (GameManager.instance == null || GameManager.instance.userInfo == null)
        {
            Debug.LogError("[PlayerSpawner] GameManager or userInfo is null. Aborting spawn.");
            return;
        }
        
        Transform targetSpawn = defaultSpawn;

        switch (GameManager.instance.userInfo.lastSpawnLocation)
        {
            case SpawnLocation.FromBountyBoard:
                targetSpawn = bountyBoardExitSpawn;
                Debug.Log($"[PlayerSpawner] Spawn location set to Bounty Board Exit: {targetSpawn.name}");
                break;
            case SpawnLocation.FromAlchemyTable:
                targetSpawn = alchemyExitSpawn;
                Debug.Log($"[PlayerSpawner] Spawn location set to Alchemy Table Exit: {targetSpawn.name}");
                break;
        }

        Debug.Log($"[PlayerSpawner] Spawn location requested: {GameManager.instance.userInfo.lastSpawnLocation}");
        Debug.Log($"[PlayerSpawner] Target spawn point: {targetSpawn?.name}");

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = targetSpawn.position;
            Debug.Log($"[PlayerSpawner] Player found and moved to: {targetSpawn.position}");
        }
    }
}
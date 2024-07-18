using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

//manages how players will be spawned
//currently spawns player at position of PlayerSpawner GameObejct

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float range = 1f;

    private void Awake()
    {
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("Registering connection approval callback.");
            NetworkManager.Singleton.ConnectionApprovalCallback += CustomConnectionApprovalCallback;
        }
        else
        {
            Debug.LogError("NetworkManager is not set.");
        }

        SceneManager.sceneLoaded += OnSceneLoaded; // Register to scene loaded event
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("Client count " + NetworkManager.Singleton.ConnectedClientsList.Count);

        // Spawn players when scene is loaded
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("scene load found");
            SpawnPlayers();
        }
        else
        {
            Debug.Log("No server found");
        }
    }

    void CustomConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)    //mustnt be private
    {
        response.CreatePlayerObject = true;
        response.Position = GetPlayerSpawnPosition();
        response.Rotation = Quaternion.identity;
        response.Approved = true;
    }

    public Vector3 GetPlayerSpawnPosition()
    {
        Vector3 position = transform.position + new Vector3(Random.Range(-range, range), 0f, Random.Range(-range, range));
        Debug.Log(position);
        return position;
    }

    private void SpawnPlayers()
    {
        for(int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        {
            SpawnPlayer(NetworkManager.Singleton.ConnectedClientsList[i].ClientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        Debug.Log("attempt spawn players");
        Vector3 spawnPosition = GetPlayerSpawnPosition();
        GameObject playerInstance = Instantiate(player, spawnPosition, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    public override void OnDestroy()
    {
        

        if (NetworkManager.Singleton != null)
        {
            Debug.Log("Unregistering connection approval callback.");
            NetworkManager.Singleton.ConnectionApprovalCallback -= CustomConnectionApprovalCallback;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded; // Unregister scene loaded event
    }
}

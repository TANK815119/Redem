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
        NetworkManager.ConnectionApprovalCallback += CustomConnectionApprovalCallback;
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

    public override void OnDestroy()
    {
        NetworkManager.ConnectionApprovalCallback -= CustomConnectionApprovalCallback;
    }
}

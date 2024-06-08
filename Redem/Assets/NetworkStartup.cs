using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkStartup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(SceneTransitionHandler.Singleton.InitializeAsHost)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;

public class NetworkedRigidbodyOptimization : NetworkBehaviour
{
    private Rigidbody rb;
    private ClientNetworkTransform netTransform;
    private bool isSyncing = true;
    private float stationaryThreshold = 0.1f; // Velocity threshold to consider the object stationary
    private float stationaryTime = 2.0f; // Time threshold to disable network sync
    private float stationaryTimer = 0.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        netTransform = GetComponent<ClientNetworkTransform>();
    }

    void Update()
    {
        if (IsOwner)
        {
            if (rb.velocity.magnitude < stationaryThreshold)
            {
                stationaryTimer += Time.deltaTime;
                if (stationaryTimer >= stationaryTime && isSyncing)
                {
                    DisableNetworkSync();
                }
            }
            else
            {
                stationaryTimer = 0.0f;
                if (!isSyncing)
                {
                    EnableNetworkSync();
                }
            }
        }
    }

    void DisableNetworkSync()
    {
        isSyncing = false;
        netTransform.enabled = false; // Disable network transform updates
    }

    void EnableNetworkSync()
    {
        isSyncing = true;
        netTransform.enabled = true; // Enable network transform updates
    }
}
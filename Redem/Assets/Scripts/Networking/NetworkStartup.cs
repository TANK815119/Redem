using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;

//updated to soley function with relay
//I think these means--in its current state--singleplayer would not work
//I would have to add an extra "singleplayer" parameter to the SceneTransitionHandler
//to fix that
//or something else that can carry over the data
public class NetworkStartup : MonoBehaviour
{
    // Start is called before the first frame update
    private async void Start() //multiplayer
    {
        if(SceneTransitionHandler.Singleton.InitializeAsMultiplayer)
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            };
            //Debug.Log("after sign in created, before anonymous sign in");
            //await AuthenticationService.Instance.SignInAnonymouslyAsync(); //stops here, apparently im "already signed in, so I removed it
            //try
            //{
            //    await AuthenticationService.Instance.SignInAnonymouslyAsync();
            //}
            //catch (RelayServiceException e)
            //{
            //    Debug.Log(e);
            //}
            //Debug.Log("after anonymopus sign in, before decide client or host");
            if (SceneTransitionHandler.Singleton.InitializeAsHost) //host
            {
                //Debug.Log("after decided host, before create relay");
                CreateRelay();
            }
            else // client
            {
                JoinRelay(SceneTransitionHandler.Singleton.JoinRelayCode); //I pray this works; it seems so rickety
            }
        }
        else //single player
        {
            if (SceneTransitionHandler.Singleton.InitializeAsHost) //host
            {
                NetworkManager.Singleton.StartHost();
            }
            else // client
            {
                NetworkManager.Singleton.StartClient();
            }
        }
    }

    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(7); //8 player server host + 7

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);

            //assign to the scene transition handler singleton, so the join code can be fetched from anywhere
            SceneTransitionHandler.Singleton.JoinRelayCode = joinCode;

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
                );
            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining Relay with " + joinCode);
            JoinAllocation jointAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                jointAllocation.RelayServer.IpV4,
                (ushort)jointAllocation.RelayServer.Port,
                jointAllocation.AllocationIdBytes,
                jointAllocation.Key,
                jointAllocation.ConnectionData,
                jointAllocation.HostConnectionData
                );

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}

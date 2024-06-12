using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkOwnershipAura : MonoBehaviour
{
    [SerializeField] private NetworkObject networkObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<NetworkObject>(out NetworkObject otherNetObject) && NetworkManager.Singleton.IsHost)
        {
            //avoid ownership conflicts
            NetworkObject otherPlayerNetObject = NetworkManager.Singleton.ConnectedClients[otherNetObject.OwnerClientId].PlayerObject;
            if (!otherPlayerNetObject.OwnerClientId.Equals(networkObject.OwnerClientId))
            {
                List<NetworkOwnershipAura>  auraList = new List<NetworkOwnershipAura>();
                FindAuras(otherPlayerNetObject.transform, auraList);

                //check if this NetworkOwnershipAura is closest, and assign if is
                bool closestAura = true;
                float thisAuraDist = Vector3.Distance(this.transform.position, other.transform.position);
                for (int i = 0; i < auraList.Count; i++)
                {
                    float outherAuraDist = Vector3.Distance(auraList[i].transform.position, other.transform.position);
                    if(outherAuraDist < thisAuraDist)
                    {
                        closestAura = false;
                    }
                }

                if(closestAura)
                {
                    //assign ownership
                    otherNetObject.ChangeOwnership(networkObject.OwnerClientId);
                    Debug.Log("attempted change " + other.gameObject.name + " to client " + networkObject.OwnerClientId);
                }
            }
        }
    }

    private void FindAuras(Transform parent, List<NetworkOwnershipAura> auraList)
    {
        if (parent.gameObject.TryGetComponent(out NetworkOwnershipAura parentNetObject))
        {
            auraList.Add(parentNetObject);
        }

        for(int i = 0; i < parent.childCount; i++)
        {
            FindAuras(parent.GetChild(i), auraList);
        }
    }
}

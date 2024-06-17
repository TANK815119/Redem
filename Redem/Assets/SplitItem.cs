using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

//an item that breaks into two
//and has health
//is an object with the triboluminesence variable hits it, it losses health
public class SplitItem : NetworkBehaviour
{
    [SerializeField] private float minForce = 10f;
    [SerializeField] private int offspringCount = 2;
    [SerializeField] private NetworkVariable<float> health = new NetworkVariable<float>(50f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] private GameObject spawnObject;
    [SerializeField] private AudioClip damage;

    private NetworkVariable<bool> isSplit = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // Update is called once per frame
    void Update()
    {
        if(isSplit.Value && IsHost)
        {
            AttemptSplit();
        }
    }

    private void AttemptSplit()
    {
        //spawn the offspring
        for(int i = 0; i < offspringCount; i++)
        {
            GameObject splitPiece = Instantiate(spawnObject, transform.position, transform.rotation);
            if (splitPiece.TryGetComponent(out NetworkObject networkObject))
            {
                networkObject.Spawn();
            }
            else
            {
                Debug.LogError("The spawnObject in SplitAttempt lacks a NetworkObject component");
            }
        }


        //delete the mother
        if (this.TryGetComponent(out NetworkObject motherNetObject))
        {
            motherNetObject.Despawn(true);
            Destroy(this);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > minForce)//check the collsion is sufficently high velocity
        {
            if (collision.gameObject.TryGetComponent(out Triboluminescence tribo))//check for triboluminescence
            {
                health.Value -= collision.relativeVelocity.magnitude;

                AudioSource.PlayClipAtPoint(damage, this.transform.position, 0.175f); //hard coded volume

                if (health.Value < 0f)
                {
                    isSplit.Value = true;
                }
            }
        }
    }
}

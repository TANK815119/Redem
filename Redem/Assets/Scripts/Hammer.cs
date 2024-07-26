using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;

namespace Rekabsen
{
    //joints two objects if both hit by hammer
    //creates a fixed joint
    //requires a trigger collider
    public class Hammer : NetworkBehaviour
    {
        [SerializeField] float minForce = 5f;
        [SerializeField] AudioClip suctionClip;
        [SerializeField] AudioClip hitClip;
        private Rigidbody rb;

        //network
        private NetworkVariable<ulong> mainID = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkList<ulong> touchingIDs;
        //private NetworkVariable<Dictionary<ulong, bool>> clientMainIDsSynced = new NetworkVariable<Dictionary<ulong, bool>>(
        //    new Dictionary<ulong, bool>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<int> validClientCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private bool synced = false;


        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            touchingIDs = new NetworkList<ulong>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        }

        //private void FixedUpdate()
        //{
        //    if(mainID.Value == 0)
        //    {
        //        clientMainIDSynced[NetworkManager.Singleton.LocalClientId] = false;
        //    }
        //    else
        //    {
        //        clientMainIDSynced[NetworkManager.Singleton.LocalClientId] = false;
        //    }
        //}

        private void OnCollisionEnter(Collision collision)
        {
            if (IsOwner)
            {
                AttemptListenerAdd(collision);
                AttemptFusionRegister(collision);
            }
        }

        private void AttemptListenerAdd(Collision collision)
        {
            if (collision.rigidbody != null && !IsExcludedTags(collision.gameObject.tag))
            {
                //attach an listening compoonent to the object
                if (!collision.gameObject.TryGetComponent(out HammerListener listnerLess)) //check the object doesnt have the component
                {
                    HammerListener listener = collision.gameObject.AddComponent<HammerListener>();
                    listener.SetHammerBody(rb);
                }
            }
        }


        private void AttemptFusionRegister(Collision collision)
        {
            if (IsOwner && collision.rigidbody != null && !IsExcludedTags(collision.gameObject.tag) && collision.relativeVelocity.magnitude > minForce && collision.gameObject.TryGetComponent(out HammerListener listener))
            {
                //audio for a speedy strike
                AudioSource.PlayClipAtPoint(hitClip, this.transform.position, 0.175f); //hard coded volume

                //attempt to fuse the object touching the listner
                if (listener.TouchingBodies.Count > 0)
                {
                    //synchronize the main object ID
                    if (collision.rigidbody.gameObject.TryGetComponent(out NetworkObject mainNet)) { mainID.Value = mainNet.NetworkObjectId; }

                    //synchronize the list of touchingbodies
                    touchingIDs.Clear();
                    for (int i = 0; i < listener.TouchingBodies.Count; i++)
                    {
                        if (listener.TouchingBodies[i].gameObject.TryGetComponent(out NetworkObject touchingNet)) { touchingIDs.Add(touchingNet.NetworkObjectId); }
                    }

                    listener.TouchingBodies.Clear();

                    //attempt fusion
                    FuseBodiesServerRPC();
                }
            }
        }

        [ServerRpc] //first server
        private void FuseBodiesServerRPC(ServerRpcParams rpcParams = default)
        {
            FuseBodiesClientRPC();
        }

        [ClientRpc] //then clients
        private void FuseBodiesClientRPC(ClientRpcParams rpcParams = default)
        {
            //wait for variabels to be synchronized acropss clients
            StartCoroutine(WaitForSynchronization());
            //FuseBodiesGatherRigidbodies();
        }

        private void FuseBodiesGatherRigidbodies()
        {
            //read mainID and touchingIDs
            Rigidbody mainBody = null;
            List<Rigidbody> touchingBodies = new List<Rigidbody>();

            Debug.Log("mainID " + mainID.Value);
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(mainID.Value, out NetworkObject mainNet) && mainNet.TryGetComponent(out Rigidbody mainNetBody))
            {
                mainBody = mainNetBody;
                Debug.Log("mainID object " + mainBody.name);
            }

            for (int i = 0; i < touchingIDs.Count; i++)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(touchingIDs[i], out NetworkObject touchingNet) && touchingNet.TryGetComponent(out Rigidbody touchingNetBody))
                {
                    touchingBodies.Add(touchingNetBody);
                }
            }

            FuseBodies(mainBody, touchingBodies);

            if (IsOwner)
            {
                mainID.Value = 0;
                touchingIDs.Clear();
                synced = false;
            }
        }

        private void FuseBodies(Rigidbody mainBody,  List<Rigidbody> touchingBodies)
        {
            Debug.Log("fuse bodies calleed");

            if(mainBody == null)
            {
                Debug.Log("Main body is null");
            }

            if (touchingBodies.Count == 0)
            {
                Debug.Log("touchingBodies is empty");
            }

            if (mainBody == null) return;

            Debug.Log("mainBoy nullcheck passed");

            //make a bunch of fixed joints connecting to the mainBody
            //the fixedJoint should be on the mainBody
            for (int i = 0; i < touchingBodies.Count; i++)
            {
                Debug.Log("for loop entered");

                if (!IsExcludedTags(touchingBodies[i].gameObject.tag))
                {
                    Debug.Log("added fixed joint");
                    FixedJoint joint = mainBody.gameObject.AddComponent<FixedJoint>();
                    joint.connectedBody = touchingBodies[i];

                    //audio
                    AudioSource.PlayClipAtPoint(suctionClip, touchingBodies[i].position, 0.5f);
                }
            }
        }

        private IEnumerator WaitForSynchronization()
        {
            // Wait for a short period to ensure synchronization
            float waitTime = 2f; // Adjust as necessary
            float elapsedTime = 0f;

            while (elapsedTime < waitTime)
            {
                if (AreIDsSynchronized())
                {
                    FuseBodiesGatherRigidbodies(); // <----- this is where the magic happens
                    yield break;
                }
                else
                {
                    Debug.Log(mainID.Value + " time " + waitTime);
                    Debug.Log(touchingIDs.Count);
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Handle timeout or retry logic if necessary
            Debug.LogWarning("Synchronization timeout, could not retrieve all necessary objects.");
        }

        private bool AreIDsSynchronized()
        {
            InitiateMainIDCheckServerRPC();
            return synced;
        }

        private bool IsExcludedTags(string tag)
        {
            return tag.Equals("Body") || tag.Equals("PlayerCamera");
        }

        private void PrintList(List<Rigidbody> list)
        {
            for(int i = 0; i < list.Count; i++)
            {
                Debug.Log(list[i].gameObject.name);
            }
        }

        //public override void OnDestroy()
        //{
        //    base.OnDestroy();
        //    touchingIDs.Dispose();
        //    touchingIDs = null;
        //}

        //------------------------------- TERRIFYING CODE TO SYNC THE NETWORKS HERE

        // ServerRPC to initiate the check
        [ServerRpc(RequireOwnership = false)]
        public void InitiateMainIDCheckServerRPC(ServerRpcParams rpcParams = default)
        {
            // Reset the valid client count
            validClientCount.Value = 0;

            // Request all clients to check their mainID
            RequestMainIDStatusClientRPC();
        }

        // ClientRPC to request clients to check their mainID
        [ClientRpc]
        private void RequestMainIDStatusClientRPC(ClientRpcParams rpcParams = default)
        {
            bool hasValidMainID = mainID.Value != 0;
            ReportMainIDStatusServerRPC(hasValidMainID);
        }

        //// Client method to check mainID and report back
        //private bool CheckMainIDStatus()
        //{
        //    return hasValidMainID = mainID.Value != 0;
        //    ReportMainIDStatusClientRPC(hasValidMainID);
        //}

        // ClientRPC to report the status back to the server
        [ServerRpc(RequireOwnership = false)]
        private void ReportMainIDStatusServerRPC(bool hasValidMainID, ServerRpcParams rpcParams = default)
        {
            //count up the number of actualizes mainIDs
            if (hasValidMainID)
            {
                validClientCount.Value++;
            }

            // Check if all clients have reported
            if (validClientCount.Value >= NetworkManager.Singleton.ConnectedClientsList.Count)
            {
                bool allValid = validClientCount.Value == NetworkManager.Singleton.ConnectedClientsIds.Count;
                OnAllClientsCheckedClientRPC(allValid);
            }
        }

        // Server method to handle the results
        [ClientRpc]
        private void OnAllClientsCheckedClientRPC(bool allValid, ClientRpcParams rpcParams = default)
        {
            if (allValid)
            {
                synced = true;
            }
            else
            {
                synced = false;
            }
        }
    }
}
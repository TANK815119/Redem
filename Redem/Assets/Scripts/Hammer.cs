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
        private NetworkVariable<bool> willFuse = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<ulong> mainID = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkList<ulong> touchingIDs;


        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            touchingIDs = new NetworkList<ulong>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        }

        private void FixedUpdate()
        {
            if(willFuse.Value)
            {
                //read mainID and touchingIDs
                Rigidbody mainBody = null;
                List<Rigidbody> touchingBodies = new List<Rigidbody>();

                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(mainID.Value, out NetworkObject mainNet) && mainNet.TryGetComponent(out Rigidbody mainNetBody))
                {
                    mainBody = mainNetBody;
                }

                for (int i = 0; i < touchingIDs.Count; i++)
                {
                    if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(touchingIDs[i], out NetworkObject touchingNet) && touchingNet.TryGetComponent(out Rigidbody touchingNetBody))
                    {
                        touchingBodies.Add(touchingNetBody);
                    }
                }
                touchingIDs.Clear();

                FuseBodies(mainBody, touchingBodies);   

                willFuse.Value = false;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(IsOwner)
            {
                AttemptListenerAdd(collision);
                AttemptFusionRegister(collision);
            }
        }

        private void AttemptFusionRegister(Collision collision)
        {
            if (collision.rigidbody != null && !IsExcludedTags(collision.gameObject.tag) && collision.relativeVelocity.magnitude > minForce && collision.gameObject.TryGetComponent(out HammerListener listener))
            {
                //audio for a speedy strike
                AudioSource.PlayClipAtPoint(hitClip, this.transform.position, 0.175f); //hard coded volume

                //attempt to fuse the object touching the listner
                if (listener.TouchingBodies.Count > 0)
                {
                    //cache a call for fusion
                    willFuse.Value = true;

                    //synchronize the main object ID
                    if (collision.rigidbody.gameObject.TryGetComponent(out NetworkObject mainNet)) { mainID.Value = mainNet.NetworkObjectId; }

                    //synchronize the list of touchingbodies
                    touchingIDs.Clear();
                    for (int i = 0; i < listener.TouchingBodies.Count; i++)
                    {
                        if (listener.TouchingBodies[i].gameObject.TryGetComponent(out NetworkObject touchingNet)) { touchingIDs.Add(touchingNet.NetworkObjectId); }
                    }

                    listener.TouchingBodies = new List<Rigidbody>();
                }
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

        private void FuseBodies(Rigidbody mainBody,  List<Rigidbody> touchingBodies)
        {
            //make a bunch of fixed joints connecting to the mainBody
            //the fixedJoint should be on the mainBody
            for(int i = 0; i < touchingBodies.Count; i++)
            {
                if(!IsExcludedTags(touchingBodies[i].gameObject.tag))
                {
                    FixedJoint joint = mainBody.gameObject.AddComponent<FixedJoint>();
                    joint.connectedBody = touchingBodies[i];

                    //audio
                    AudioSource.PlayClipAtPoint(suctionClip, touchingBodies[i].position, 0.5f);
                }
            }
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
    }
}
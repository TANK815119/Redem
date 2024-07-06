using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rekabsen
{
    //joints two objects if both hit by hammer
    //creates a fixed joint
    //requires a trigger collider
    [RequireComponent(typeof(Collider))]
    public class Hammer : MonoBehaviour
    {
        [SerializeField] float minForce = 5f;
        [SerializeField] AudioClip suctionClip;
        [SerializeField] AudioClip hitClip;
        private Rigidbody rb;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        //private void OnTriggerEnter(Collider other)
        //{
        //    if (other.attachedRigidbody != null && !IsExcludedTags(other.gameObject.tag))
        //    {
        //        //attach an listening compoonent to the object
        //        if (!other.gameObject.TryGetComponent(out HammerListener listner)) //check the object doesnt have the component
        //        {
        //            other.gameObject.AddComponent<HammerListener>();
        //        }
        //    }
        //}

        //private void OnCollisionStay(Collision collision)
        //{
        //    if (collision.rigidbody != null && !IsExcludedTags(collision.gameObject.tag))
        //    {
        //        //attach an listening compoonent to the object
        //        if (!collision.gameObject.TryGetComponent(out HammerListener listner)) //check the object doesnt have the component
        //        {
        //            collision.gameObject.AddComponent<HammerListener>();
        //        }
        //    }
        //}

        //private void OnCollisionExit(Collision collision)
        //{
        //    if (collision.rigidbody != null && !IsExcludedTags(collision.gameObject.tag))
        //    {
        //        //attach an listening compoonent to the object
        //        if (!collision.gameObject.TryGetComponent(out HammerListener listnerLess)) //check the object doesnt have the component
        //        {
        //            collision.gameObject.AddComponent<HammerListener>();
        //        }
        //    }
        //    if (collision.rigidbody != null && !IsExcludedTags(collision.gameObject.tag))
        //    {
        //        //remove the listening component of the object
        //        if(collision.gameObject.TryGetComponent(out HammerListener listner))
        //        {
        //            Destroy(listner);
        //        }
        //    }
        //}

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.rigidbody != null && !IsExcludedTags(collision.gameObject.tag))
            {
                //attach an listening compoonent to the object
                if (!collision.gameObject.TryGetComponent(out HammerListener listnerLess)) //check the object doesnt have the component
                {
                    collision.gameObject.AddComponent<HammerListener>();
                }
            }

            if (collision.rigidbody != null && !IsExcludedTags(collision.gameObject.tag) && collision.relativeVelocity.magnitude > minForce && collision.gameObject.TryGetComponent(out HammerListener listner))
            {
                //audio for a speedy strike
                AudioSource.PlayClipAtPoint(hitClip, this.transform.position, 0.175f); //hard coded volume

                //attempt to fuse the object touching the listner
                if (listner.TouchingBodies.Count > 0)
                {
                    FuseBodies(collision.rigidbody, listner.TouchingBodies);
                    listner.TouchingBodies = new List<Rigidbody>();
                }
            }
        }

        private void FuseBodies(Rigidbody mainBody,  List<Rigidbody> touchingBodies)
        {
            //make a bunch of fixed joints connecting to the mainBody
            //the fixedJoint should be on the mainBody
            for(int i = 0; i < touchingBodies.Count; i++)
            {
                if(!IsExcludedTags(touchingBodies[i].gameObject.tag) && !touchingBodies[i].Equals(rb))
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
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triboluminescence : MonoBehaviour
{
    [SerializeField] private float minForce = 1f;
    [SerializeField] private float combustRadius = 0.5f;
    [SerializeField] private GameObject sparks;

    private List<Transform> flamableList; //list of flamable objects in range(sticks)

    private void Start()
    {
        flamableList = new List<Transform>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<MonoBehaviour>() != null) //MonoBehavior stand-in
        {
            flamableList.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<MonoBehaviour>() != null) //MonoBehavior stand-in
        {
            flamableList.Remove(other.transform);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.relativeVelocity.magnitude > minForce)//check the collsion is sufficently high velocity
        {
            if(collision.gameObject.GetComponent<Triboluminescence>() != null)//check for triboluminescence
            {
                for(int i = 0; i < collision.contactCount; i++) //for each collsion point
                {
                    //create sparks
                    //this method of Instantiation is super expensive, but easy
                    GameObject sparkInstance = Instantiate(sparks, collision.contacts[i].point, Quaternion.identity);
                    sparkInstance.transform.forward = collision.contacts[i].normal;

                    //look for neary flamable objects to light
                    AttemptCombust(collision.contacts[i]);
                }
            }
        }
    }

    private void AttemptCombust(ContactPoint contactPoint)
    {
        for(int i = 0; i < flamableList.Count; i++)
        {
            Debug.Log(gameObject.name);
            if(Vector3.Distance(contactPoint.point, flamableList[i].position) < combustRadius)
            {
                //combuts the flamable object

            }
        }
    }
}

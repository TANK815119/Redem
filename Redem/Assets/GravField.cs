using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class GravField : MonoBehaviour
{
    [SerializeField] private float attractiveness = 10f;
    private List<Rigidbody> influence;
    // Start is called before the first frame update
    void Start()
    {
        influence = new List<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < influence.Count; i++)
        {
            Vector3 direction = transform.position - influence[i].position;
            float distance = direction.magnitude;
            if (distance <= 1f) continue; //so that we don't get extreme forces at close range
            float forceMagnitude = 1f / (distance * distance); // Inverse square law
            Vector3 force = direction.normalized * forceMagnitude * attractiveness; // Scale the force
            influence[i].AddForce(force);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Rigidbody thisBody = null;
        if (other.TryGetComponent<Rigidbody>(out Rigidbody body))
        {
            thisBody = body;
        }
        else
        {
            return;
        }

        if(influence.Contains(thisBody))
        {
            return;
        }

        thisBody.useGravity = false;
        influence.Add(thisBody);
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody thisBody = null;
        if (other.TryGetComponent<Rigidbody>(out Rigidbody body))
        {
            thisBody = body;
        }
        else
        {
            return;
        }

        if (!influence.Contains(thisBody))
        {
            return;
        }

        thisBody.useGravity = true;
        influence.Remove(thisBody);
    }
}

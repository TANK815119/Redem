using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private Transform ball;
    public Transform pointA;
    public Transform pointB;
    // Start is called before the first frame update
    void Start()
    {
        ball.position = pointA.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(ball.position, pointB.position) < 0.5)
        {
            ball.position = pointB.position;
            ball.GetComponent<Rigidbody>().isKinematic = true;
            Debug.Log("Close!");
        }
    }
}

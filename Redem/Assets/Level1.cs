using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Level1 : MonoBehaviour
{
    [SerializeField] private Transform ball;
    public Transform start;
    public Transform end;
    // Start is called before the first frame update
    bool p1 = false;

    bool start_level = false;

    public void StartLevel()
    {
        if (start_level == true)
        {
            ball.position = start.position;
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }
        start_level = true;
        ball.GetComponent<Rigidbody>().isKinematic = false;
    }

    void Start()
    {
        ball.position = start.position;
        ball.GetComponent<Rigidbody>().isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!start_level) return;

        if (Vector3.Distance(ball.position, end.position) < 1)
        {
            p1 = true;
        }
        if (p1)
        {
            ball.position = end.position;
            ball.GetComponent<Rigidbody>().isKinematic = true;
            Debug.Log("Passed Level 1!");
            start_level = false;
            SceneManager.LoadScene("Level2");
        }
    }
}

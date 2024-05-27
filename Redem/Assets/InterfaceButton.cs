using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceButton : MonoBehaviour
{
    [SerializeField] private Transform cursor;

    private bool cursorTouching = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(cursorTouching)
        {
            Debug.Log("touching");
            transform.Translate(new Vector3(0f, -0.5f, 0f), Space.Self);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.transform.gameObject.name);
        if (other.transform.Equals(cursor))
        {
            cursorTouching = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.Equals(cursor))
        {
            cursorTouching = false;
        }
    }
}

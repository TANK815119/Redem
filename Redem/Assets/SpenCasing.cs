using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this script's enitre prupose is to despawn spent casings

public class SpenCasing : MonoBehaviour
{
    [SerializeField] private float despawnTimer = 10f;
    [SerializeField] private bool immortal = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        despawnTimer -= Time.deltaTime;
        if(despawnTimer < 0f && !immortal)
        {
            Destroy(gameObject);
        }
    }
}

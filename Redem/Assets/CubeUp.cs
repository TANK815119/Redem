using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeUp : MonoBehaviour
{
    [SerializeField] private Transform cube;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        cube.position = new Vector3(0f, cube.position.y + 1f * Time.deltaTime, 0f);
    }
}

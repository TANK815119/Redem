using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class RandomStart : MonoBehaviour
{
    [SerializeField] private int numberOfCubes;
    [SerializeField] private Vector3 minPosition;
    [SerializeField] private Vector3 maxPosition;
    [SerializeField] private GameObject targetPrefab;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numberOfCubes; i++)
        {
            // Pick random position
            float x = Random.Range(minPosition.x, maxPosition.x);
            float y = Random.Range(minPosition.y, maxPosition.y);
            float z = Random.Range(minPosition.z, maxPosition.z);

            Vector3 randomPosition = new Vector3(x, y, z);

            // Spawn cube
            Instantiate(targetPrefab, randomPosition, transform.rotation);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

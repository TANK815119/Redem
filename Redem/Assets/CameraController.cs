using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float duration = 2f;
    [SerializeField] private Camera cam;
    [SerializeField] private float rotationSpeed = 100f;

    private Coroutine currentMoveCoroutine;

    

    private float pitch = 0f; // Up/down
    private float yaw = 0f;   // Left/right

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    /*IEnumerator MoveCamera()
    {
        // Move to point A

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            yield return StartCoroutine(LerpPosition(pointA.position, duration));
        }
        // Wait a second at point A (optional)

        // Move to point B

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            yield return StartCoroutine(LerpPosition(pointB.position, duration));
        }
    } */

    IEnumerator LerpPosition(Vector3 targetPos, float duration)
    {
        Vector3 startPos = cam.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Calculate normalized time [0..1]
            float t = elapsed / duration;

            // Interpolate position
            cam.transform.position = Vector3.Lerp(startPos, targetPos, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Make sure camera reaches the exact target position at the end
        cam.transform.position = targetPos;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.RightArrow))
        //{
        //    // Stop any ongoing movement
        //    if (currentMoveCoroutine != null)
        //        StopCoroutine(currentMoveCoroutine);

        //    // Start moving to point A
        //    currentMoveCoroutine = StartCoroutine(LerpPosition(pointA.position, duration));
        //}
        //else if (Input.GetKeyDown(KeyCode.LeftArrow))
        //{
        //    // Stop any ongoing movement
        //    if (currentMoveCoroutine != null)
        //        StopCoroutine(currentMoveCoroutine);

        //    // Start moving to point B
        //    currentMoveCoroutine = StartCoroutine(LerpPosition(pointB.position, duration));
        //}


        //// Get input
        //float horizontal = 0f;
        //float vertical = 0f;

        //if (Input.GetKey(KeyCode.W)) vertical = 1f;
        //if (Input.GetKey(KeyCode.S)) vertical = -1f;
        //if (Input.GetKey(KeyCode.A)) horizontal = -1f;
        //if (Input.GetKey(KeyCode.D)) horizontal = 1f;

        //// Adjust yaw and pitch
        //yaw += horizontal * rotationSpeed * Time.deltaTime;
        //pitch -= vertical * rotationSpeed * Time.deltaTime;

        //// Clamp pitch so you can’t flip the camera upside down
        //pitch = Mathf.Clamp(pitch, -80f, 80f);

        //// Apply rotation
        //transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Required for new Input System

public class CameraToggle : MonoBehaviour
{
    [SerializeField] private Transform mainView;
    [SerializeField] private Transform auxView;
    [SerializeField] private float transitionSpeed = 2f; // how quickly to pan

    private bool usingMainView = true;
    private Transform targetView;

    void Start()
    {
        // Start on main view
        usingMainView = true;
        targetView = mainView;
        transform.position = mainView.position;
        transform.rotation = mainView.rotation;
    }

    void Update()
    {
        // Check for "T" key press using legacy Input
        if (Input.GetKeyDown(KeyCode.T))
        {
            usingMainView = !usingMainView;
            targetView = usingMainView ? mainView : auxView;
        }

        // Smoothly pan/rotate toward the target view
        transform.position = Vector3.Lerp(
            transform.position,
            targetView.position,
            Time.deltaTime * transitionSpeed);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetView.rotation,
            Time.deltaTime * transitionSpeed);
    }

}

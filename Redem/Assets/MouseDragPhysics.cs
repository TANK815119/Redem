using UnityEngine;

public class MouseDragPhysics : MonoBehaviour
{
    private Camera mainCamera;
    private GameObject draggedObject;
    private ConfigurableJoint dragJoint;
    private float dragDistance = 5f; // Distance from camera to drag the object
    private Vector3 initialOffset; // Offset between object and joint anchor

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
        }
    }

    void Update()
    {
        // Start dragging on left mouse button down
        if (Input.GetMouseButtonDown(0))
        {
            StartDragging();
        }

        // Stop dragging on left mouse button release
        if (Input.GetMouseButtonUp(0))
        {
            StopDragging();
        }

        // Update dragged object position
        if (draggedObject != null)
        {
            UpdateDragPosition();
        }
    }

    void StartDragging()
    {
        // Cast a ray from the mouse position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        LayerMask mask = LayerMask.GetMask("Draggable");
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
        {
            // Check if the hit object has a Rigidbody
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                draggedObject = hit.collider.gameObject;
                initialOffset = hit.point - draggedObject.transform.position;

                // Create a temporary GameObject to act as the joint anchor
                GameObject anchorObject = new GameObject("DragAnchor");
                anchorObject.transform.position = hit.point;

                // Add ConfigurableJoint to the dragged object
                dragJoint = draggedObject.AddComponent<ConfigurableJoint>();
                dragJoint.connectedBody = anchorObject.AddComponent<Rigidbody>();
                dragJoint.connectedBody.isKinematic = true; // Anchor doesn't move with physics
                dragJoint.autoConfigureConnectedAnchor = false;
                dragJoint.anchor = initialOffset; // Anchor at the hit point in local space
                dragJoint.connectedAnchor = Vector3.zero; // Connected anchor at anchorObject's origin

                // Configure joint motion (lock all except position driven by spring)
                dragJoint.xMotion = ConfigurableJointMotion.Free;
                dragJoint.yMotion = ConfigurableJointMotion.Free;
                dragJoint.zMotion = ConfigurableJointMotion.Free;
                dragJoint.angularXMotion = ConfigurableJointMotion.Locked;
                dragJoint.angularYMotion = ConfigurableJointMotion.Locked;
                dragJoint.angularZMotion = ConfigurableJointMotion.Locked;

                // Configure joint drive for smooth dragging
                JointDrive jointDrive = new JointDrive
                {
                    positionSpring = 10000f, // Strong spring for responsiveness
                    positionDamper = 50f,    // Damping for smooth motion
                    maximumForce = 1000f     // Maximum force to apply
                };
                dragJoint.xDrive = jointDrive;
                dragJoint.yDrive = jointDrive;
                dragJoint.zDrive = jointDrive;
            }
        }
    }

    void UpdateDragPosition()
    {
        // Get the mouse position in world space at the drag distance
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPosition = ray.origin + ray.direction * dragDistance;

        // Move the joint's connected body (anchor) to the target position
        if (dragJoint != null && dragJoint.connectedBody != null)
        {
            dragJoint.connectedBody.transform.position = targetPosition;
        }
    }

    void StopDragging()
    {
        if (draggedObject != null && dragJoint != null)
        {
            // Destroy the joint and the temporary anchor object
            if (dragJoint.connectedBody != null)
            {
                Destroy(dragJoint.connectedBody.gameObject); // Destroy anchor object
            }
            Destroy(dragJoint); // Destroy the joint
            draggedObject = null;
            dragJoint = null;
        }
    }
}
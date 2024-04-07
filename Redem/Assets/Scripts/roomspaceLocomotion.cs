using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomspaceLocomotion : MonoBehaviour
{
    [SerializeField] private Transform headset;
    [SerializeField] private Transform physicsHead;
    [SerializeField] private GameObject rotoball;
    [SerializeField] private float radius = 0.2f;

    private Rigidbody rotoBody;
    private Transform rotoTrans;
    private RotoState rotoState;

    private Vector3 lastHeadsetPosition = Vector3.zero;
    private Quaternion lastRotoballRotation = Quaternion.identity;
    // Start is called before the first frame update
    void Start()
    {
        rotoBody = rotoball.GetComponent<Rigidbody>();
        rotoTrans = rotoball.transform;
        rotoState = rotoball.GetComponent<RotoState>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rotoBody.angularVelocity = Vector3.zero;
        //update headset deltas
        Vector3 headsetPosition = headset.position - physicsHead.position;
        Vector3 deltaHeadsetPosition = headsetPosition - lastHeadsetPosition;
        lastHeadsetPosition = headsetPosition;

        //update rotoball deltas
        Quaternion rotoballRotation = rotoTrans.rotation;
        Quaternion deltaRotoballRotation = rotoballRotation * Quaternion.Inverse(lastRotoballRotation); //this - last
        lastRotoballRotation = rotoballRotation;

        //update rotostate
        float xDeltaRot = deltaRotoballRotation.eulerAngles.x;
        float zDeltaRot = deltaRotoballRotation.eulerAngles.z;
        if(xDeltaRot > 180f)
        {
            xDeltaRot = xDeltaRot - 360f;
        }
        if(zDeltaRot > 180f)
        {
            zDeltaRot = zDeltaRot - 360f;
        }
        rotoState.IdealRotDelta(deltaHeadsetPosition.x, deltaHeadsetPosition.z, radius);
        rotoState.RealRotDelta(xDeltaRot, zDeltaRot, radius); // should only appear in one place

        //rotate in the rotoState direction;
        Vector3 direction = rotoState.RotationDirection() * rotoState.PIDTorque(Time.fixedDeltaTime, rotoBody.velocity.magnitude);
        rotoBody.AddTorque(direction.x, 0f, -direction.z);
    }
}

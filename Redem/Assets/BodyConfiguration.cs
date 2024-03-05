using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Rendering;

public class BodyConfiguration : MonoBehaviour
{
    [SerializeField] private Transform anchor;
    [SerializeField] private Transform hip;
    [SerializeField] private Transform head;
    [SerializeField] private Transform headCamera;
    [SerializeField] private Transform headset;
    [SerializeField] private Transform legs;
    [SerializeField] private float playerHeightMax = 1.90f;
    [SerializeField] private float torsoLengthMax = 0.625f;
    [SerializeField] private float rotoBallRadius = 0.2f;
    [SerializeField] private float crouchSpeed = 1f; 
    [SerializeField] private float jumpDepth = 1f; 
    [SerializeField] private float turnSpeed = 120f; 

    private InputData inputData;
    private ConfigurableJoint hipJoint;
    private ConfigurableJoint headJoint;
    private ConfigurableJoint legJoint;
    private float torsoHeight;
    private float playerHeight;
    private float hipHeight;
    public float virtualCrouch = 0f;
    private float jumpCrouch = 0f;
    private float turn = 0f;
    // Start is called before the first frame update
    void Start()
    {
        inputData = GetComponent<InputData>();
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;

        hipJoint = hip.gameObject.GetComponent<ConfigurableJoint>();
        headJoint = head.gameObject.GetComponent<ConfigurableJoint>();
        legJoint = legs.gameObject.GetComponent<ConfigurableJoint>();

        hip.gameObject.GetComponent<Rigidbody>().sleepThreshold = 0f;
        hipHeight = rotoBallRadius / 4;
    }
    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    // Update is called once per frame
    void Update()
    {
        VirtualCrouch();
        BodilyCaltulations();
        PhysicsHead();
        PhysicsHip();
        PhysicsLegs();
    }
    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        BodilyCaltulations();
        VirtualTurn();
        PhysicsHead();
        PhysicsHip();
        PhysicsLegs();
    }

    private void BodilyCaltulations()
    {
        //pretty simple math
        playerHeight = headset.localPosition.y + virtualCrouch;
        // playerHeight = 1.75f + virtualCrouch;
        torsoHeight = torsoLengthMax; //for now

        //keep the hipHeight in a range beteen the ground and max height possible(-torso)
        if (playerHeight <= playerHeightMax && playerHeight - torsoHeight - (rotoBallRadius / 4) >= 0f) //remove "rotoball radius/4" if want even lower coruch
        {
            hipHeight = playerHeight - torsoHeight;
        }
    }
    private void PhysicsHead()
    {
        //update the head's rotation to be same as headset
        headCamera.rotation = Quaternion.Euler(0f, turn, 0f) * 
            ((inputData.headset.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion headsetRotation)) ?  headsetRotation : headCamera.rotation);
        //headCamera.rotation = headCamera.rotation * Quaternion.Euler(0f, turn, 0f);

        headJoint.connectedAnchor = new Vector3(0f, torsoHeight * 10f, 0f);
    }

    private void PhysicsHip()
    {
        hipJoint.connectedAnchor = new Vector3(0f, (hipHeight - rotoBallRadius) * 10f, 0f);
    }

    private void PhysicsLegs()
    {
        //find the distance between the hip and anchor
        float legLength = hip.position.y - anchor.position.y;
        if(legLength > 0f)
        {
            //find the midpoint between the hip and anchor
            float legPosition = hip.position.y - (legLength / 2f);

            //manipulate the leg transform with this information
            legs.localScale = new Vector3(0.2f, Mathf.Abs(legLength / 2f), 0.2f);
            legJoint.anchor = new Vector3(0f, legPosition * 1f, 0f);
        }
        else
        {
            legs.localScale = new Vector3(0.0f, 0f, 0.0f);
            legJoint.connectedAnchor = new Vector3(0f, 0f, 0f);
        }
    }

    private void VirtualTurn()
    {
        if (inputData.rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstick))
        {
            if (Mathf.Abs(thumbstick.x) > 0.5f)
            {
                turn += turnSpeed * thumbstick.x * Time.deltaTime;  //delta tiem ought to work, even here

            }
        }
    }

    private void VirtualCrouch()
    {
        inputData.rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstick);
        inputData.rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool squating);

        if (virtualCrouch < 0f)
        {
            if (thumbstick.y > 0.1f)
            {
                virtualCrouch += thumbstick.y * crouchSpeed * Time.deltaTime;
            }
            if (!squating && jumpCrouch <= 0) //instant
            {
                virtualCrouch -= jumpCrouch;
                jumpCrouch = 0f;
            }
        }
        else
        {
            virtualCrouch = 0f;
        }

        if (playerHeight - torsoHeight >= 0f)
        {
            if (thumbstick.y < -0.4f)
            {
                virtualCrouch += thumbstick.y * crouchSpeed * Time.deltaTime;
            }
            if (squating && jumpCrouch > jumpDepth)
            {
                virtualCrouch += -crouchSpeed * Time.deltaTime;
                jumpCrouch += -crouchSpeed * Time.deltaTime;
            }
        }
        else
        {
            if(virtualCrouch - (playerHeight - torsoHeight) < 0f && playerHeight - torsoHeight < 0f)
            {
                virtualCrouch -= (playerHeight - torsoHeight); //in event "mathematical" hip goes too low
            }

            //the pplayer is prepped to jump if "bottoms out"
            if ((playerHeight - virtualCrouch) < Mathf.Abs(jumpDepth) && squating)
            {
                jumpCrouch = jumpDepth;
            }
            else if(squating)
            {
                jumpCrouch = -(playerHeight - virtualCrouch);
            }
        }
    }
}

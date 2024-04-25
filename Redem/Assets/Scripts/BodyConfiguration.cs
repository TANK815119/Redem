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
    [SerializeField] private float nerdNeckMin = 0.4f; 

    private InputData inputData;
    private ConfigurableJoint hipJoint;
    private ConfigurableJoint headJoint;
    private ConfigurableJoint legJoint;

    //"virtual" body values
    private float torsoHeight;
    private float playerHeight;
    private float hipHeight;
    private float nerdNeck;

    private float virtualCrouch = 0f;
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
        //fetch player's input height using the virtualcrouch
        playerHeight = headset.localPosition.y + virtualCrouch;

        //torso/head offset calculations
        torsoHeight = torsoLengthMax; //for now
        nerdNeck = nerdNeckMin; //for now
        

        ////use height(actual) to manipulate nerneck and torso length proportionatly @TODO major adjustments with if statements
        //nerdNeck = Mathf.Clamp(nerdNeckMin + nerdNeckMin * (1f - playerHeight / playerHeightMax) * 1f, nerdNeckMin, nerdNeckMin * 2f); //needs tweeking
        ////torsoHeight = Mathf.Clamp(torsoHeight - torsoHeight * (playerHeightMax - playerHeight / playerHeightMax) * 0.3333f, torsoLengthMax * 0.333f, torsoLengthMax); //needs tweeking
        //torsoHeight = Mathf.Sqrt(torsoLengthMax * torsoLengthMax - nerdNeck * nerdNeck); // pythagorean therom;
        ////Debug.Log("nerd neck: " + nerdNeck + " Torso Height: " + torsoHeight + " TorsoLength: " + Mathf.Sqrt(torsoHeight * torsoHeight + nerdNeck * nerdNeck));

        //use head rotation to manipulate hip/head(leg/torso) height ratio height and nerneck
        float headRoll = headset.localEulerAngles.x;
        if (headRoll >= 0f && headRoll <= 90f)
        {
            torsoHeight = torsoHeight - (Mathf.Abs(headRoll / 90f) * torsoHeight / 3f); //needs tweeking
            nerdNeck = nerdNeck - (Mathf.Abs(headRoll / 90f) * nerdNeck); //needs tweeking
        }
        //Debug.Log("Head Roll: " + headRoll + "Herd Neck: " + nerdNeck);

        //keep the hipHeight in a range beteen the ground and max height possible(-torso)
        if (playerHeight <= playerHeightMax && playerHeight - torsoHeight - (rotoBallRadius / 2f) >= 0f) //remove "rotoball radius/4" if want even lower coruch
        {
            hipHeight = playerHeight - torsoHeight;
        }
    }
    private void PhysicsHead()
    {
        //update the head's rotation to be same as headset
        headCamera.rotation = Quaternion.Euler(0f, turn, 0f) * 
            ((inputData.headset.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion headsetRotation)) ?  headsetRotation : headCamera.rotation);

        //float xNerdNeck = Mathf.Sin(Mathf.Deg2Rad * headCamera.eulerAngles.y) * nerdNeck; // using the transform rotation may be unwise as it may be "slow"/behind
        //float zNerdNeck = Mathf.Cos(Mathf.Deg2Rad * headCamera.eulerAngles.y) * nerdNeck; // using the transform rotation may be unwise as it may be "slow"/behind
        headJoint.connectedAnchor = new Vector3(0f * 10f, torsoHeight * 10f, nerdNeck * 10f);
    }
    private void PhysicsHip()
    {
        hipJoint.connectedAnchor = new Vector3(0f, (hipHeight - rotoBallRadius) * 10f, 0f);

        //calculate hip y rotation
        if(!(headCamera.eulerAngles.x > 75f && headCamera.eulerAngles.x <= 95f) || !(headCamera.eulerAngles.z > 75f && headCamera.eulerAngles.z <= 95f))  // make sure not looking down too much, else must lerp
        {
            hip.rotation = Quaternion.Euler
            (0f,
            (Quaternion.Euler(0f, turn, 0f) * ((inputData.headset.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion headsetRotation)) ? headsetRotation : headCamera.rotation)).eulerAngles.y,
            0f);
        }
        else 
        {
            Quaternion newQuaternion = Quaternion.Euler
            (0f,
            (Quaternion.Euler(0f, turn, 0f) * ((inputData.headset.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion headsetRotation)) ? headsetRotation : headCamera.rotation)).eulerAngles.y,
            0f);
            hip.rotation = Quaternion.Slerp(hip.rotation, newQuaternion, 0.1f * Time.deltaTime);
        }
    }

    private void PhysicsLegs()
    {
        //find the distance between the hip and anchor
        float legLength = hip.position.y - anchor.position.y;
        if(legLength > 0f)
        {
            //manipulate the leg transform with this information
            legs.localScale = new Vector3(0.2f, Mathf.Abs(legLength / 2f), 0.2f);
            legJoint.anchor = new Vector3(0f, (legLength / 2f) * 1f, 0f);
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

    public Vector3 HipOffset() // get the positional difference between the head and hip  
    {
        Quaternion headRotation = ((inputData.headset.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion headsetRotation)) ? headsetRotation : headCamera.rotation);
        //return hip.position - head.position;
        return Quaternion.Euler(0f, headRotation.eulerAngles.y, 0f) * new Vector3(0f, -torsoHeight, -nerdNeck);
    }
}

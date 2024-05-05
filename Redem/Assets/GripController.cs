using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class GripController : MonoBehaviour
{
    [SerializeField] bool isRightController = false;

    private bool gripping = false;

    private ConfigurableJoint joint;
    private List<Transform> grabList;
    private HandAnimation handAnim;
    private Rigidbody handBody;
    private InputData inputData;
    // Start is called before the first frame update
    void Start()
    {
        grabList = new List<Transform>();
        handAnim = gameObject.GetComponent<HandAnimation>();
        handBody = gameObject.GetComponent<Rigidbody>();
        inputData = gameObject.GetComponent<InputData>();
    }

    // Update is called once per frame
    void Update()
    {
        float grip = (isRightController) ? GetRightGrip() : GetLeftGrip();

        if(grip > 0.85f && !gripping && grabList.Count != 0)
        {
            gripping = true;
            GrabPoint grabPoint = grabList[0].GetComponent<GrabPoint>();

            //change hand animation state
            handAnim.Gripping = true;
            handAnim.GripState = grabPoint.GrabType;

            //form a joint with that object
            joint = grabPoint.ParentTrans.gameObject.AddComponent<ConfigurableJoint>();

            //position
            joint.configuredInWorldSpace = true;
            joint.connectedBody = handBody;
            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = new Vector3(0f, 0f, 0f);
            joint.targetPosition = grabPoint.GetCurrParentOffset() - 0.1f * grabPoint.transform.up;
            
            //rotation
            joint.targetRotation = grabPoint.GetCurrParentRotationOffset() * grabPoint.ParentTrans.rotation * Quaternion.Inverse(handBody.transform.rotation);

            //drives
            JointDrive jointDrive = new JointDrive();
            jointDrive.positionSpring = 99999f;
            jointDrive.maximumForce = Mathf.Infinity;
            joint.xDrive = jointDrive;
            joint.yDrive = jointDrive;
            joint.zDrive = jointDrive;
            joint.angularXDrive = jointDrive;
            joint.angularYZDrive = jointDrive;
        }

        if(grip < 0.65 && gripping)
        {
            gripping = false;

            //change hand animation state
            handAnim.Gripping = false;

            //destroy the joint with the gripped object
            Destroy(joint);
            joint = null;
        }
    }

    private void CreateGrip()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<GrabPoint>() != null)
        {
            grabList.Add(other.transform);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<GrabPoint>() != null)
        {
            grabList.Remove(other.transform);
        }
    }

    private float GetRightGrip()
    {
        if (inputData.rightController.TryGetFeatureValue(CommonUsages.grip, out float controllerGrip))
        {
            return controllerGrip;
        }
        return 0f;
    }

    private float GetLeftGrip()
    {
        if (inputData.leftController.TryGetFeatureValue(CommonUsages.grip, out float controllerGrip))
        {
            return controllerGrip;
        }
        return 0f;
    }
}

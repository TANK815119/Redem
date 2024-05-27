using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class GripController : MonoBehaviour
{
    [SerializeField] private bool isRightController = false;
    [SerializeField] private bool falseGrip = false;
    [SerializeField] private AudioClip grip;
    [SerializeField] private AudioClip ungrip;

    private bool gripping = false;
    private bool clenched = false;

    private ConfigurableJoint joint;
    private GrabPoint grabbedPoint;
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

        if(falseGrip == true)
        {
            grip = 1f;
        }

        if(grip > 0.85f && !gripping && !clenched)
        {
            if(grip > 0.975f)
            {
                clenched = true;
            }
            if(grabList.Count != 0)
            {
                CreateGrip();
            }
        }

        if(grip < 0.85f)
        {
            clenched = false;
            if(gripping)
            {
                DestroyGrip();
            }
        }


        //if(joint != null && joint.currentForce.magnitude > 9999f) //currentFOrce only works one "limited" joints(works well now but may acause problems later)
        //{
        //    Debug.Log(joint.currentForce.magnitude);
        //    DestroyGrip();
        //}
    }

    private void CreateGrip()
    {
        gripping = true;
        GrabPoint grabPoint = FindClosestGrabPoint(grabList, handBody.transform).GetComponent<GrabPoint>();
        grabbedPoint = grabPoint;
        grabbedPoint.Grabbed = true;
        grabbedPoint.IsRightController = isRightController;

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
        joint.connectedAnchor = new Vector3(0f, 0f, 0f);
        joint.targetPosition = grabPoint.GetCurrParentOffset() - 0.1f * grabPoint.transform.up; //hard coded for hand length

        //rotation
        joint.targetRotation = grabPoint.GetCurrParentRotationOffset() * grabPoint.ParentTrans.rotation * Quaternion.Inverse(handBody.transform.rotation);

        //drives
        if(!grabPoint.SoftGrip)
        {
            JointDrive jointDrive = new JointDrive();
            jointDrive.positionSpring = 99999f;
            jointDrive.maximumForce = Mathf.Infinity;
            joint.xDrive = jointDrive;
            joint.yDrive = jointDrive;
            joint.zDrive = jointDrive;
            joint.angularXDrive = jointDrive;
            joint.angularYZDrive = jointDrive;
        }
        else
        {
            //JointDrive jointDrive = new JointDrive();
            //jointDrive.positionSpring = 99999f;
            //jointDrive.maximumForce = Mathf.Infinity;
            //joint.xDrive = jointDrive;
            //joint.yDrive = jointDrive;
            //joint.zDrive = jointDrive;
            //joint.angularXDrive = jointDrive;
            //joint.angularYZDrive = jointDrive;

            //joint.xMotion = ConfigurableJointMotion.Limited;
            //joint.yMotion = ConfigurableJointMotion.Limited;
            //joint.zMotion = ConfigurableJointMotion.Limited;

            //joint.targetPosition = Vector3.zero;
            //joint.connectedAnchor = grabPoint.GetCurrParentOffset() + 0.1f * grabPoint.transform.up; //hard coded for hand length
        }

        //play audio
        AudioSource.PlayClipAtPoint(grip, transform.position, 1f);
    }

    private void DestroyGrip()
    {
        gripping = false;
        grabbedPoint.Grabbed = false;

        //change hand animation state
        handAnim.Gripping = false;

        //destroy the joint with the gripped object
        Destroy(joint);
        joint = null;

        //play audio
        AudioSource.PlayClipAtPoint(ungrip, transform.position, 1f);
    }

    private Transform FindClosestGrabPoint(List<Transform> grabList, Transform hand)
    {
        Vector3 handPoint = hand.position + 0.1f * hand.up; //hard coded hand length

        //search through the list for the closest grab point
        Transform closestGrabPoint = grabList[0];
        for(int i = 0; i < grabList.Count; i++)
        {
            float oldDistance = (closestGrabPoint.position - handPoint).magnitude;
            float thisDistance = (grabList[i].position - handPoint).magnitude;
            if (thisDistance < oldDistance)
            {
                closestGrabPoint = grabList[i];
            }
        }
        return closestGrabPoint;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<GrabPoint>() != null)
        {
            grabList.Add(other.transform);
        }
        if (other.GetComponent<GrabPlane>() != null)
        {
            grabList.Add(other.GetComponent<GrabPlane>().FollowHand(transform));
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<GrabPoint>() != null)
        {
            grabList.Remove(other.transform);
        }
        if (other.GetComponent<GrabPlane>() != null)
        {
            grabList.Remove(other.GetComponent<GrabPlane>().UnfollowHand(transform));
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

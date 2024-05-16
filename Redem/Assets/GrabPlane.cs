using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPlane : MonoBehaviour
{
    [field: SerializeField] public bool SoftGrip { get; private set; } // not implemented yet
    [field: SerializeField] public int GrabPose { get; private set; }

    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private int surfaceType = 0;

    /*
    0 - plane - box collider(thin)
    1 - corner - cylinder collider
    2 - sphere - sphere collider
    3 - cylinder - cylinder collider
    */

    private Collider trigCol;
    private List<GameObject> grabList;
    private List<Transform> followList;
    // Start is called before the first frame update
    void Start()
    {
        trigCol = gameObject.GetComponent<Collider>();
        grabList = new List<GameObject>();
        followList = new List<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (followList.Count != 0 && grabList.Count != 0)
        {
            for (int i = 0; i < followList.Count; i++)
            {
                switch (surfaceType)
                {
                    case 0: UpdatePlaneFollow(grabList[i], followList[i], 0.1f); break;//hard coded hand length
                    case 1: UpdateCornerFollow(grabList[i], followList[i], 0.1f); break;//hard coded hand length
                    case 2: UpdateSphereFollow(grabList[i], followList[i], 0.1f); break;//hard coded hand length
                    case 3: UpdateCylinderFollow(grabList[i], followList[i], 0.1f); break;//hard coded hand length
                    default: UpdatePlaneFollow(grabList[i], followList[i], 0.1f); break;//hard coded hand length
                }
            }
        }
    }

    public Transform FollowHand(Transform handTrans)
    {
        //stow the hand transform for tracking in update()
        followList.Add(handTrans);

        //make a grabPoint that will be mainpulated by the transform of the hand
        GameObject grabObject = new GameObject();
        grabObject.name = "FollowGrabPoint";
        grabObject.transform.parent = transform.parent;

        GameObject grabMarker = Instantiate(markerPrefab, Vector3.zero, Quaternion.identity);
        grabMarker.transform.parent = grabObject.transform;
        grabMarker.transform.localPosition = Vector3.zero;
        grabMarker.transform.localRotation = Quaternion.identity;

        GrabPoint grabPoint = grabObject.AddComponent<GrabPoint>();
        grabPoint.SoftGrip = SoftGrip;
        grabPoint.GrabType = GrabPose;
        grabObject.transform.position = transform.position;
        grabObject.transform.rotation = transform.rotation;

        //stow the grabPopint at the same index as the handTrans
        grabList.Add(grabObject);

        return grabObject.transform;
    }

    public Transform UnfollowHand(Transform handTrans) // precondition: handTrans and grabObject are in the list
    {
        //search and destroy both at index
        int index = 0;
        while (index < followList.Count && !followList[index].Equals(handTrans))
        {
            index++;
        }
        followList.RemoveAt(index);
        GameObject grabObject = grabList[index];
        Destroy(grabList[index]);
        grabList.RemoveAt(index);
        return grabObject.transform;
    }

    /*
    Every method past this point is essentially a different surface,
    each one having slightly different mathemeatical calculations to place and rotate the FollowGripPoint
    */

    private void UpdatePlaneFollow(GameObject grabPoint, Transform handTrans, float handLength) //box collider
    {
        //calculate the offset handPosition and rotation
        Vector3 handPosition = handTrans.position + handLength * handTrans.up;
        Quaternion handRotation = handTrans.rotation;

        //find the position
        Vector3 planeNormal = trigCol.transform.up; // Get the normal of the plane
        Vector3 planePoint = trigCol.transform.position; // Get any point on the plane

        Vector3 directionToObject = handPosition - planePoint; // Vector from any point on the plane to the object
        float distance = Vector3.Dot(directionToObject, planeNormal); // Project the vector onto the plane's normal

        Vector3 closestPoint = handPosition - distance * planeNormal; // Subtract the projected distance from the object's position
        closestPoint = trigCol.ClosestPoint(closestPoint); // constrain the position to the collider
        Debug.DrawLine(handPosition, closestPoint, Color.red); // Draw a line from the object to the closest point on the plane

        grabPoint.transform.position = closestPoint;

        //find the rotation
        grabPoint.transform.rotation = handRotation;
        grabPoint.transform.parent = trigCol.transform;
        grabPoint.transform.localRotation = Quaternion.Euler(90f, grabPoint.transform.localRotation.eulerAngles.y, grabPoint.transform.localRotation.eulerAngles.z);
        
        //reset hierarchy
        grabPoint.transform.parent = trigCol.transform.transform.parent;
    }

    private void UpdateCornerFollow(GameObject grabPoint, Transform handTrans, float handLength) //cylinder collider
    {
        //calculate the offset handPosition and rotation
        Vector3 handPosition = handTrans.position + handLength * handTrans.up;
        Quaternion handRotation = handTrans.rotation;

        //find position
        CapsuleCollider capCol = (CapsuleCollider)(trigCol);
        Vector3 lineEnd = capCol.transform.position + capCol.transform.up * capCol.height/2f;
        Vector3 lineStart = capCol.transform.position - capCol.transform.up * capCol.height / 2f;
        Vector3 lineDirection = (lineEnd - lineStart).normalized; // Direction of the line

        Vector3 pointToLineStart = lineStart - handPosition;
        float distanceAlongLine = Vector3.Dot(pointToLineStart, lineDirection); // Project the vector onto the line's direction

        Vector3 closestPointOnLine = lineStart - distanceAlongLine * lineDirection; // Calculate the closest point on the line
        closestPointOnLine = trigCol.ClosestPoint(closestPointOnLine); // constrain the position to the collider
        grabPoint.transform.position = closestPointOnLine;
        Debug.DrawLine(handPosition, grabPoint.transform.position, Color.red); // Draw a line from the object to the closest point on the plane

        //find the rotation
        grabPoint.transform.rotation = handRotation;
        grabPoint.transform.parent = trigCol.transform; //parent to collider

        //float xCompress = grabPoint.transform.localRotation.eulerAngles.x;
        //float xdiff = Quaternion.Angle(Quaternion.Euler(xCompress, 0f, 0f), Quaternion.Euler(45f, 0f, 0f));
        //if(xdiff > 45f)
        //{
        //    xCompress = 45f;
        //}
        //Quaternion anteriorQuat = Quaternion.Euler(xCompress, capCol.transform.localRotation.eulerAngles.y, capCol.transform.localRotation.eulerAngles.z + 90f);
        Quaternion anteriorQuat = Quaternion.Euler(0f, -45f, 90f);
        Quaternion posteriorQuat = Quaternion.Euler(0f, -45f, -90f);
        if(Quaternion.Angle(grabPoint.transform.parent.parent.parent.rotation * grabPoint.transform.parent.parent.localRotation * grabPoint.transform.parent.localRotation *  anteriorQuat, handRotation) < //mind boggling line of code
            Quaternion.Angle(grabPoint.transform.parent.parent.parent.rotation * grabPoint.transform.parent.parent.localRotation * grabPoint.transform.parent.localRotation * posteriorQuat, handRotation))  //compares the global rotations of the hand the the (theorhetical)grabpoints
        {
            grabPoint.transform.localRotation = anteriorQuat;
        }
        else
        {
            grabPoint.transform.localRotation = posteriorQuat;
        }

        //reset hierarchy
        grabPoint.transform.parent = trigCol.transform.transform.parent;
    }

    private void UpdateSphereFollow(GameObject grabPoint, Transform handTrans, float handLength)
    {
        //calculate the offset handPosition and rotation
        Vector3 handPosition = handTrans.position + handLength * handTrans.up;
        Quaternion handRotation = handTrans.rotation;

        //find position
        //essentially, just use closest point for the easiest result(it is likely just the math I would do)
        SphereCollider sphererCol = (SphereCollider)(trigCol);
        Vector3 directionToTarget = handPosition - sphererCol.transform.position; // Vector from sphere center to target point
        Vector3 closestPoint = sphererCol.transform.position + directionToTarget.normalized * sphererCol.radius; // Closest point on sphere's surface
        grabPoint.transform.position = closestPoint;

        //find rotation
        //find whatever the quaternion is for pointing the bottom of the hand twards the center of the sphereCollider;
        //or maybe the vector of the middle to the surface, then rotate the resulting rotation 90 on the x(global or local doesnt matter, just dont make it child of the collider
        //grabPoint.transform.rotation *= Quaternion.LookRotation(handPosition, grabPoint.transform.position);
        grabPoint.transform.parent = trigCol.transform;

        Vector3 surfaceVector = grabPoint.transform.position - trigCol.transform.position;
        grabPoint.transform.up = surfaceVector;

        grabPoint.transform.localRotation = Quaternion.Euler(0f * grabPoint.transform.localRotation.eulerAngles.x + 90f, grabPoint.transform.localRotation.eulerAngles.y, 0f * grabPoint.transform.localRotation.eulerAngles.y);

        //reset hierarchy
        grabPoint.transform.parent = trigCol.transform.transform.parent;
    }

    private void UpdateCylinderFollow(GameObject grabPoint, Transform handTrans, float handLength)
    {
        //allot like the sphereFollow, just locked on one axis for rotation.
        //calculate the offset handPosition and rotation
        Vector3 handPosition = handTrans.position + handLength * handTrans.up;
        Quaternion handRotation = handTrans.rotation;

        //position
        CapsuleCollider capCol = (CapsuleCollider)(trigCol);
        Vector3 axisDirection = capCol.transform.up;
        Vector3 pointOnAxis = capCol.transform.position + axisDirection * Vector3.Dot(handPosition - capCol.transform.position, axisDirection);
        //make radius 0, if want a 2d grip, maybe limit placement from "ends"?
        Vector3 direction = (handPosition - pointOnAxis).normalized;
        Vector3 closestPoint = pointOnAxis + direction * capCol.radius;
        grabPoint.transform.position = closestPoint;

        //rotation
        grabPoint.transform.parent = trigCol.transform;

        Vector3 surfaceVector = (grabPoint.transform.position - pointOnAxis).normalized;
        Debug.DrawLine(pointOnAxis, grabPoint.transform.position, Color.red);
        grabPoint.transform.up = surfaceVector;

        //lock to clock or counter hand position akin to corner grip
        Quaternion clockQuat = Quaternion.Euler(90f, -90f, 0f);
        Quaternion counterQuat = Quaternion.Euler(90f, 90f, 0f);
        if (Quaternion.Angle(clockQuat, handTrans.localRotation) < //mind boggling line of code
            Quaternion.Angle(counterQuat, handTrans.localRotation))  //compares the global rotations of the hand the the (theorhetical)grabpoints
        {
            grabPoint.transform.localRotation *= clockQuat;
        }
        else
        {
            grabPoint.transform.localRotation *= counterQuat;
        }

        //reset hierarchy
        grabPoint.transform.parent = trigCol.transform.transform.parent;
    }
}
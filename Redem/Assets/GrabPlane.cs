using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPlane : MonoBehaviour
{
    [field: SerializeField] public bool SoftGrip { get; private set; } // not implemented yet
    [field: SerializeField] public int GrabType { get; private set; }

    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private int surfaceType = 0;

    /*
    0 - plane
    1 - corner
    2 - sphere
    3 - cylinder
    */

    private List<GameObject> grabList;
    private List<Transform> followList;
    // Start is called before the first frame update
    void Start()
    {
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
                switch(surfaceType)
                {
                    case 0: UpdatePlaneFollow(grabList[i], followList[i], 0.1f); break;//hard coded hand length
                    default: UpdatePlaneFollow(grabList[i], followList[i], 0.1f); break;//hard coded hand length
                }
                UpdatePlaneFollow(grabList[i], followList[i], 0.1f); //hard coded hand length
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
        grabPoint.GrabType = GrabType;
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
        while(index < followList.Count && !followList[index].Equals(handTrans))
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

    private void UpdatePlaneFollow(GameObject grabPoint, Transform handTrans, float handLength)
    {
        //calculate the offset handPosition and rotation
        Vector3 handPosition = handTrans.position + 0.1f * handTrans.up;
        Quaternion handRotation = handTrans.rotation;

        //find the position
        Vector3 planeNormal = transform.up; // Get the normal of the plane
        Vector3 planePoint = transform.position; // Get any point on the plane

        Vector3 directionToObject = handPosition - planePoint; // Vector from any point on the plane to the object
        float distance = Vector3.Dot(directionToObject, planeNormal); // Project the vector onto the plane's normal

        Vector3 closestPoint = handPosition - distance * planeNormal; // Subtract the projected distance from the object's position
        Debug.DrawLine(handPosition, closestPoint, Color.red); // Draw a line from the object to the closest point on the plane

        grabPoint.transform.position = closestPoint;

        //find the rotation
        grabPoint.transform.rotation = handRotation;
        grabPoint.transform.localRotation = Quaternion.Euler(90f, grabPoint.transform.localRotation.eulerAngles.y, grabPoint.transform.localRotation.eulerAngles.z);
    }
}

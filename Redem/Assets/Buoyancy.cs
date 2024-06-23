using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// creates the physics for acurate movement in bodies of water

[RequireComponent(typeof(Rigidbody))]
public class Buoyancy : MonoBehaviour
{
    [SerializeField] private bool debugMode = false;
    [SerializeField] private GameObject debugMarker;
    [SerializeField] [Range(0f, 5f)] private float staticDrag = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float dynamicDrag = 0.2f;
    [SerializeField] [Range(0f, 5f)] private float staticAngleDrag = 0.5f; // reduce to zero if small object
    [SerializeField] [Range(0f, 1f)] private float dynamicAngleDrag = 0.1f; // reduce to zero if small object
    [SerializeField] private float submersionRValue = 1f;
    private List<Collider> colliders;
    private List<Collider> waterTriggers;
    private Rigidbody rb;
    private bool floating = false;
    private float wholeVolume = 0f;

    private Vector3 lastSubmersionCenter = Vector3.zero;
    private float lastSubmergedVolume = 0f;

    private GameObject[] markers;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        colliders = new List<Collider>();
        waterTriggers = new List<Collider>();
        lastSubmersionCenter = transform.position + rb.centerOfMass;

        //fetch ther colliders associated with the rigidbody
        if (TryGetComponent(out Collider thisCollider))
        {
            colliders.Add(thisCollider);
        }

        Collider[] allColliders = GetComponentsInChildren<Collider>();
        for(int i = 0; i < allColliders.Length; i++)
        {
            if(allColliders[i].attachedRigidbody.Equals(rb))
            {
                colliders.Add(allColliders[i]);
            }
        }

        //caclulate the wholeVolume
        for(int i = 0; i < colliders.Count; i++)
        {
            wholeVolume += ColliderVolume(colliders[i]);
        }

        if(debugMode)
        {
            markers = new GameObject[343 * 2];
            for (int i = 0; i < markers.Length; i++)
            {
                markers[i] = Instantiate(debugMarker);
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(floating)
        {
            Debug.Log("water triggers: " + waterTriggers.Count);
            //find which colliders are submerged
            List<Collider> submergedColliders = SubmergedColliders();

            //find submerged volume
            float submergedVolume = Mathf.Lerp(lastSubmergedVolume, SubmergedVolume(submergedColliders), (5f + rb.velocity.magnitude + rb.angularVelocity.magnitude) * Time.fixedDeltaTime); //lerp smoothing
            if (lastSubmergedVolume == 0f)
            {
                submergedVolume = SubmergedVolume(submergedColliders);

            }
            lastSubmergedVolume = submergedVolume;

            Debug.Log("aproximate submerged volume: " + submergedVolume);

            //find average of submerged point extremtities
            Vector3 submersionCenter = Vector3.Lerp(lastSubmersionCenter, SubmergedBoundCenter(submergedColliders), (5f + rb.velocity.magnitude + rb.angularVelocity.magnitude) * Time.fixedDeltaTime); // lerp smoothing
            if (lastSubmersionCenter.Equals(transform.position) || lastSubmersionCenter.Equals(Vector3.zero))
            {
                submersionCenter = SubmergedBoundCenter(submergedColliders);
            }
            lastSubmersionCenter = submersionCenter;

            Debug.Log("Center of submersion: " + submersionCenter);
            if(debugMode)
            {
                debugMarker.transform.position = submersionCenter;
            }

            //apply upwards force at average based on volume submerged and gravity(archemedes princiopal)
            float bouyantForce = ArchimedesBouyantForce(submergedVolume, 1000f);
            Debug.Log("bouyant force: " + bouyantForce);
            rb.AddForceAtPosition(Vector3.up * bouyantForce, submersionCenter, ForceMode.Force);

            //apply positional forces based on inverse of velocity at average position of submerged colliders
            //this should simulate water drag
            Vector3 drag = HydroDrag(Mathf.Sqrt(submergedVolume), staticDrag, dynamicDrag);
            rb.AddForceAtPosition(drag, submersionCenter, ForceMode.Force);

            Vector3 angularDrag = AngularHydroDrag(Mathf.Sqrt(submergedVolume), staticAngleDrag, dynamicAngleDrag);
            rb.AddTorque(angularDrag, ForceMode.Force);
        }
        else
        {
            //decay submersionCenter and bouyancy back to 0
            float submergedVolume = Mathf.Lerp(lastSubmergedVolume, 0f, (5f + rb.velocity.magnitude + rb.angularVelocity.magnitude) * Time.fixedDeltaTime); 
            lastSubmergedVolume = submergedVolume;

            Vector3 submersionCenter = Vector3.Lerp(lastSubmersionCenter, transform.position + rb.centerOfMass, (5f + rb.velocity.magnitude + rb.angularVelocity.magnitude) * Time.fixedDeltaTime);
            lastSubmersionCenter = submersionCenter;

            if (debugMode)
            {
                debugMarker.transform.position = submersionCenter;
            }
        }
    }

    private List<Collider> SubmergedColliders() //submersion is based on center of collider
    {
        List<Collider> submergedColliders = new List<Collider>();
        for(int x = 0; x < colliders.Count; x++)
        {
            for(int y = 0; y < waterTriggers.Count; y++)
            {
                if(waterTriggers[y].bounds.Intersects(colliders[x].bounds))
                {
                    submergedColliders.Add(colliders[x]);
                }
            }
        }

        return submergedColliders;
    }

    private float SubmergedVolume(List<Collider> submergedColliders)
    {
        float totalVolume = 0f;
        for(int i = 0; i < submergedColliders.Count; i++)
        {
            Vector3[] corners = GetBoundsCorners(submergedColliders[i].bounds, i);

            //see how many corners overlap with the waterTrigger bounds
            int submergedCorners = SubmergedPointsCount(corners);

            //use # of submerged points to estimate this submergedvolume
            float subPointFraction = (float)submergedCorners / 343f;
            //float subPointModulator = Mathf.Pow((wholeVolume * 1000f) / rb.mass, 1/2);
            float subPointFractionModulated = Mathf.Pow(subPointFraction, 1f);
            float thisSubVolume = subPointFractionModulated * ColliderVolume(submergedColliders[i]);
            totalVolume += thisSubVolume;
        }

        return totalVolume;
    }

    private Vector3 SubmergedBoundCenter(List<Collider> submergedColliders)
    {
        // list of all submergedPoints on the colliders
        List<Vector3> submergedPoints = new List<Vector3>();
        for (int i = 0; i < submergedColliders.Count; i++)
        {
            Vector3[] corners = GetBoundsCorners(submergedColliders[i].bounds, i);

            List<Vector3> thisSubPoints = SubmergedPointsList(corners);

            for(int j = 0; j < thisSubPoints.Count; j++)
            {
                submergedPoints.Add(thisSubPoints[j]);
            }
        }

        ////create a bound for all submergedPoint
        //Bounds submergedBound;
        //if (submergedPoints.Count > 0)
        //{
        //    submergedBound = new Bounds(submergedPoints[0], Vector3.zero);
        //}
        //else
        //{
        //    submergedBound = new Bounds(transform.position, Vector3.zero); //default to the center of the object
        //}

        //for (int i = 0; i < submergedPoints.Count; i++)
        //{
        //    submergedBound.Encapsulate(submergedPoints[i]);
        //}

        ////rerturn the center of the bound area
        //return submergedBound.center;

        //average all submerged points ASSUMES EVEN POINT DISTRIBUTION
        Vector3 submersionCenter = Vector3.zero;
        if (submergedPoints.Count > 0)
        {
            for(int i = 0; i < submergedPoints.Count; i++)
            {
                submersionCenter += submergedPoints[i];
            }
            submersionCenter = submersionCenter / submergedPoints.Count;
        }
        else
        {
            submersionCenter = transform.position + rb.centerOfMass; //default to the center of mass of the object
        }

        return submersionCenter;
    }

    private float ArchimedesBouyantForce(float displacedVolume, float fluidDensity)
    {
        return -1f * fluidDensity * Physics.gravity.y * displacedVolume;
    }

    private Vector3 HydroDrag(float submergedVolume, float statDrag, float dynDrag)
    {
        float submersionFraction = submergedVolume / wholeVolume;

        Vector3 momentum = rb.mass * rb.velocity * statDrag; // momentum = m*v for static drag
        Vector3 force = rb.mass * (rb.velocity / Time.fixedDeltaTime) * dynDrag; // force = m*a for dynamic drag

        return -submersionFraction * (momentum + force);
    }

    private Vector3 AngularHydroDrag(float submergedVolume, float statDrag, float dynDrag)
    {
        float submersionFraction = submergedVolume / wholeVolume;

        Vector3 momentum = rb.mass * rb.angularVelocity * statDrag; // momentum = m*v for static drag
        Vector3 force = rb.mass * (rb.angularVelocity / Time.fixedDeltaTime) * dynDrag; // force = m*a for dynamic drag

        return -submersionFraction * (momentum + force);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 8 && !waterTriggers.Contains(other)) //8 is water layer
        {
            waterTriggers.Add(other);
            floating = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 8 && waterTriggers.Contains(other)) //8 is water layer
        {
            waterTriggers.Remove(other);

            if (waterTriggers.Count == 0)
            {
                floating = false;
                //lastSubmersionCenter = Vector3.zero;
                //lastSubmergedVolume = 0f;
            }
        }
    }

    Vector3[] GetBoundsCorners(Bounds bounds, int objIndex)
    {
        Vector3[] corners = new Vector3[343];

        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        //// Calculate the 8 corners
        //corners[0] = center + new Vector3(extents.x, extents.y, extents.z);  // Top front right
        //corners[8] = center + new Vector3(extents.x/2f, extents.y, extents.z);
        //corners[9] = center + new Vector3(extents.x, extents.y/2f, extents.z);
        //corners[10] = center + new Vector3(extents.x, extents.y, extents.z/2f);
        //corners[1] = center + new Vector3(extents.x, extents.y, -extents.z); // Top back right
        //corners[11] = center + new Vector3(extents.x/2f, extents.y, -extents.z);
        //corners[12] = center + new Vector3(extents.x, extents.y/2f, -extents.z);
        //corners[13] = center + new Vector3(extents.x, extents.y, -extents.z/2f);
        //corners[2] = center + new Vector3(extents.x, -extents.y, extents.z); // Bottom front right
        //corners[14] = center + new Vector3(extents.x/2f, -extents.y, extents.z);
        //corners[15] = center + new Vector3(extents.x, -extents.y/2f, extents.z);
        //corners[16] = center + new Vector3(extents.x, -extents.y, extents.z/2f);
        //corners[3] = center + new Vector3(extents.x, -extents.y, -extents.z);// Bottom back right
        //corners[17] = center + new Vector3(extents.x/2f, -extents.y, -extents.z);
        //corners[18] = center + new Vector3(extents.x, -extents.y/2f, -extents.z);
        //corners[19] = center + new Vector3(extents.x, -extents.y, -extents.z/2f);
        //corners[4] = center + new Vector3(-extents.x, extents.y, extents.z); // Top front left
        //corners[20] = center + new Vector3(-extents.x/2f, extents.y, extents.z);
        //corners[21] = center + new Vector3(-extents.x, extents.y/2f, extents.z);
        //corners[22] = center + new Vector3(-extents.x, extents.y, extents.z/2f);
        //corners[5] = center + new Vector3(-extents.x, extents.y, -extents.z);// Top back left
        //corners[23] = center + new Vector3(-extents.x/2f, extents.y, -extents.z);
        //corners[24] = center + new Vector3(-extents.x, extents.y/2f, -extents.z);
        //corners[25] = center + new Vector3(-extents.x, extents.y, -extents.z/2f);
        //corners[6] = center + new Vector3(-extents.x, -extents.y, extents.z);// Bottom front left
        //corners[26] = center + new Vector3(-extents.x/2f, -extents.y, extents.z);
        //corners[27] = center + new Vector3(-extents.x, -extents.y/2f, extents.z);
        //corners[28] = center + new Vector3(-extents.x, -extents.y, extents.z/2f);
        //corners[7] = center + new Vector3(-extents.x, -extents.y, -extents.z);// Bottom back left
        //corners[29] = center + new Vector3(-extents.x/2f, -extents.y, -extents.z);
        //corners[30] = center + new Vector3(-extents.x, -extents.y/2f, -extents.z);
        //corners[31] = center + new Vector3(-extents.x, -extents.y, -extents.z/2f);

        //Interpolate between adjacent points to make a higher fidelity point map
        //List<Vector3> points = new List<Vector3>();
        //for (int i = 0; i < corners.Length; i++)
        //{
        //    for (int j = 0; j < corners.Length; j++)
        //    {
        //        //interpolate nearby points
        //        int coordinateMathces = 0;
        //        Vector3 interpPoint = corners[i];

        //        if(corners[i].x == corners[j].x) //x coordinate
        //        {
        //            coordinateMathces++;
        //        }
        //        else
        //        {
        //            interpPoint.x = (corners[i].x + corners[y].x) / 2f;
        //        }
        //        if (corners[i].y == corners[j].y) //y coordinate
        //        {
        //            coordinateMathces++;
        //        }
        //        else
        //        {
        //            interpPoint.x = (corners[i].x + corners[y].x) / 2f;
        //        }
        //        if (corners[i].x == corners[j].x) //z coordinate
        //        {
        //            coordinateMathces++;
        //        }
        //        else
        //        {
        //            interpPoint.x = (corners[i].x + corners[y].x) / 2f;
        //        }
        //    }
        //}

        //calculate the position of 7*7*7 = 343 3d points within the object
        int index = 0;
        int pointScale = 4; //essentiall the # of points deep, wide, high
        float xUnit = extents.x / (float)(pointScale - 1);
        float yUnit = extents.y / (float)(pointScale - 1);
        float zUnit = extents.z / (float)(pointScale - 1);
        for(int x = -(pointScale - 1); x < pointScale; x++)
        {
            for(int z = -(pointScale - 1); z < pointScale; z++)
            {
                for(int y = -(pointScale - 1); y < pointScale; y++)
                {
                    corners[index] = center + new Vector3(xUnit * x, yUnit * y, zUnit * z);
                    if(debugMode)
                    {
                        markers[index * (objIndex + 1)].transform.position = corners[index];
                    }
                    index++;
                }
            }
        }

        return corners;
    }

    private int SubmergedPointsCount(Vector3[] points)
    {
        //see how many corners overlap with the waterTrigger bounds
        int submergedCorners = 0;
        for (int x = 0; x < points.Length; x++)
        {
            for (int y = 0; y < waterTriggers.Count; y++)
            {
                if (waterTriggers[y].bounds.Contains(points[x]))
                {
                    submergedCorners++;
                }
            }
        }

        return submergedCorners;
    }

    private List<Vector3> SubmergedPointsList(Vector3[] points)
    {
        //see which many corners overlap with the waterTrigger bounds
        List<Vector3> submergedPoints = new List<Vector3>();
        for (int x = 0; x < points.Length; x++)
        {
            for (int y = 0; y < waterTriggers.Count; y++)
            {
                if (waterTriggers[y].bounds.Contains(points[x]))
                {
                    submergedPoints.Add(points[x]);
                }
            }
        }

        return submergedPoints;
    }

    private float ColliderVolume(Collider collider)
    {
        Vector3 localScale = collider.transform.lossyScale;

        if (collider is BoxCollider)
        {
            BoxCollider box = (BoxCollider)collider;
            Vector3 size = box.size;
            return size.x * localScale.x * size.y * localScale.y * size.z * localScale.z;
        }
        else if (collider is SphereCollider)
        {
            SphereCollider sphere = (SphereCollider)collider;
            float radius = sphere.radius;
            return (4f / 3f) * Mathf.PI * Mathf.Pow(radius * localScale.x, 3); // Assuming uniform scale
        }
        else if (collider is CapsuleCollider)
        {
            CapsuleCollider capsule = (CapsuleCollider)collider;
            float radius = capsule.radius;
            float height = capsule.height - 2 * radius;

            float scaledRadius = radius * Mathf.Max(localScale.x, localScale.z); // Assuming uniform scale for radius
            float scaledHeight = height * localScale.y;

            float volumeCaps = Mathf.PI * Mathf.Pow(scaledRadius, 2) * scaledHeight;
            float volumeSpheres = (4f / 3f) * Mathf.PI * Mathf.Pow(scaledRadius, 3);
            return volumeCaps + volumeSpheres;
        }
        else
        {
            Debug.LogError("Collider type not supported for volume calculation.");
            return 0;
        }
    }
}

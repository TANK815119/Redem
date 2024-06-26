using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// creates the physics for acurate movement in bodies of water

[RequireComponent(typeof(Rigidbody))]
public class Buoyancy : MonoBehaviour
{
    [SerializeField] private bool showGizmo = false;
    [SerializeField] private GameObject debugMarker;
    [SerializeField] private float staticDrag = 5f;
    [SerializeField] private float dynamicDrag = 0.0f;
    [SerializeField] private float angularDragCoefficient = 5f;

    private List<Collider> waterTriggers;
    private List<ColliderData> colliderData;
    private Rigidbody rb;
    private bool floating = false;
    private float submergedVolume = 0f;
    private float submergedCrossSection = 0f;
    private float wholeVolume = 0f;
    private float wholeCrossSection = 0f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        waterTriggers = new List<Collider>();
        colliderData = new List<ColliderData>();

        //fetch ther colliders associated with the rigidbody
        if (TryGetComponent(out Collider thisCollider) && thisCollider.isTrigger == false) //collider on this object
        {
            colliderData.Add(new ColliderData(thisCollider, ColliderVolume(thisCollider), AproximateCrossSectionalArea(thisCollider)));
        }

        Collider[] allColliders = GetComponentsInChildren<Collider>(); //all child colliders
        for(int i = 0; i < allColliders.Length; i++)
        {
            if (allColliders[i].attachedRigidbody.Equals(rb) && allColliders[i].isTrigger == false) //cull for associoated colliders
            {
                colliderData.Add(new ColliderData(allColliders[i], ColliderVolume(allColliders[i]), AproximateCrossSectionalArea(allColliders[i])));
            }
        }

        //caclulate the wholeVolume and wholeCorssSection
        for(int i = 0; i < colliderData.Count; i++)
        {
            wholeVolume += colliderData[i].Volume;
            wholeCrossSection += colliderData[i].CrossSection;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //if(floating && false)
        //{
        //    Debug.Log("water triggers: " + waterTriggers.Count);
        //    //find which colliders are submerged
        //    List<Collider> submergedColliders = SubmergedColliders();

        //    //find submerged volume
        //    float submergedVolume = SubmergedVolume(submergedColliders);

        //    Debug.Log("aproximate submerged volume: " + submergedVolume);

        //    //find average of submerged point extremtities
        //    Vector3 submersionCenter = SubmergedBoundCenter(submergedColliders);

        //    Debug.Log("Center of submersion: " + submersionCenter);
        //    if(showGizmo)
        //    {
        //        gizmoCenterOfSubmersion = submersionCenter;
        //    }

        //    //apply upwards force at average based on volume submerged and gravity(archemedes princiopal)
        //    float bouyantForce = ArchimedesBouyantForce(submergedVolume, 1000f);
        //    Debug.Log("bouyant force: " + bouyantForce);
        //    rb.AddForceAtPosition(Vector3.up * bouyantForce, submersionCenter, ForceMode.Force);

        //    //apply positional forces based on inverse of velocity at average position of submerged colliders
        //    //this should simulate water drag
        //    Vector3 drag = HydroDrag(submergedVolume, staticDrag, dynamicDrag);
        //    rb.AddForceAtPosition(drag, submersionCenter, ForceMode.Force);

        //    if (stableObject)
        //    {
        //        Vector3 angularDrag = AngularHydroDrag(submergedVolume, staticAngleDrag, dynamicAngleDrag); //physically accurate rotation
        //        rb.AddTorque(angularDrag, ForceMode.Force);
        //    }
        //    else
        //    {
        //        //use simple drag
        //        float dragCoefficient = 10f * (submergedVolume / wholeVolume);
        //        rb.angularVelocity = rb.angularVelocity * (1f - dragCoefficient * Time.fixedDeltaTime); 
        //    }
        //}

        if (floating)
        {
            submergedVolume = 0f;
            submergedCrossSection = 0f;
            
            for (int i = 0; i < colliderData.Count; i++)
            {
                //update the mutable values of colliderData for FractionSubmerged and Submerged
                UpdateColliderData(colliderData[i]);

                if(colliderData[i].Submerged)
                {
                    //calculate and apply the buoyancy force at the collider
                    ApplyArchemedicBouyancy(colliderData[i], 1000f); //hard coded fluid density

                    //calculate and apply the Hydraulic drag at the collider
                    //make sure to utilize both velocity and
                    //"real" angular velocty in whcihever direction is the tangent of rotation and distance from center of mass
                    ApplyHydraulicDrag(colliderData[i], staticDrag, dynamicDrag, 1000f); //hard coded fluid density
                }
            }

            //apply rotation drag using air equation
            float dragCoefficient = angularDragCoefficient * (submergedCrossSection / wholeCrossSection);
            ApplyAngularNewtonianDrag(dragCoefficient);
        }
        else
        {
            //make sure the objec does not exceed its terminal velocity
            for(int i = 0; i < colliderData.Count; i++)
            {
                //update all collider data for air
                colliderData[i].FractionSubmerged = 1f; //totally submereged in air

                //calculate and apply the buoyancy force at the collider
                ApplyArchemedicBouyancy(colliderData[i], 1.25f); //hard coded fluid density of air

                //calculate and apply the Hydraulic drag at the collider
                ApplyHydraulicDrag(colliderData[i], staticDrag, dynamicDrag, 1.25f); //hard coded fluid density

            }
        }
    }

    //new methods-----------------------------------------------------------------

    private void UpdateColliderData(ColliderData colData)
    {
        colData.FractionSubmerged = GetFractionSubmerged(colData.Collider.bounds);
        submergedVolume += colData.Volume * colData.FractionSubmerged;
        submergedCrossSection += colData.CrossSection * colData.FractionSubmerged;

        if (colData.FractionSubmerged > 0f)
        {
            colData.Submerged = true;
        }
        else
        {
            colData.Submerged = false;
        }
    }

    private void ApplyArchemedicBouyancy(ColliderData colData, float fluidDensity)
    {
        //calculate the bouyant force
        float displacedVolume = colData.Volume * colData.FractionSubmerged;
        float force = ArchimedesBouyantForce(displacedVolume, fluidDensity);

        //calculate the world space poition of the collider center
        Vector3 colliderCenter = GetColliderWorldCenter(colData.Collider);

        //apply the force
        rb.AddForceAtPosition(Vector3.up * force, colliderCenter);
    }

    private void ApplyHydraulicDrag(ColliderData colData, float statDrag, float dynDrag, float fluidDensity)
    {
        //caclulate positional vector from purely rigidbody
        Vector3 positionalVelocity = rb.velocity;

        //calculate tangential velocity at the center of the collider
        Vector3 relativePosition = GetColliderWorldCenter(colData.Collider) - (transform.position + rb.centerOfMass);
        Vector3 rotationalVelocity = Vector3.Cross(rb.angularVelocity, relativePosition);

        //add the two forces together
        Vector3 totalVelocity = positionalVelocity + rotationalVelocity;
        //Vector3 totalAcceleration = totalVelocity / Time.fixedDeltaTime;

        //calculate drag
        float crossSectionArea = colData.CrossSection * colData.FractionSubmerged;
        Vector3 staticDragForce = CalculateDragForce(fluidDensity, statDrag, crossSectionArea, totalVelocity);
        //Vector3 dynamicDragForce = CalculateDragForce(fluidDensity, dynDrag, crossSectionArea, totalAcceleration);

        //make sure the force is physically possible
        float linearKineticEnergy = 0.5f * rb.mass * rb.velocity.sqrMagnitude; // F = 0.5mv^2
        float rotationKineticEnergy = 0.5f * (rb.mass * relativePosition.magnitude) * rb.angularVelocity.sqrMagnitude; //F = 0.5IW^2
        float kineticEnergyParsel = (linearKineticEnergy + rotationKineticEnergy); // calculates how much KE is in this part of the object, assuming uniform denstity
        float reactiveKineticEnergy = staticDragForce.magnitude; //calculated reactionary force
        if(reactiveKineticEnergy > kineticEnergyParsel)
        {
            staticDragForce = kineticEnergyParsel * staticDragForce.normalized; //squash down to physical limit
            //must mean there is some physical impossibility happening
        }

        //addfore in the opposite direction
        //Debug.Log(gameObject.name + " " + staticDragForce + " values -0.5f" + fluidDensity + " * " + staticDrag + " * " + crossSectionArea + " * " + totalVelocity.magnitude * totalVelocity.magnitude + " total corss: " + wholeCrossSection);
        rb.AddForceAtPosition(staticDragForce, GetColliderWorldCenter(colData.Collider));
    }

    private void ApplyAngularNewtonianDrag(float dragCoefficient)
    {
        rb.angularVelocity = rb.angularVelocity * (1f - dragCoefficient * Time.fixedDeltaTime);
    }

    //new helper methods-----------------------------------------------------------

    private float GetFractionSubmerged(Bounds colliderBounds)
    {
        float boundVolume = colliderBounds.extents.x * colliderBounds.extents.y * colliderBounds.extents.z;
        float boundVolumeSubmerged = 0f;

        for(int i = 0; i < waterTriggers.Count; i++)
        { 
            if(waterTriggers[i].bounds.Intersects(colliderBounds))
            {
                Bounds intersectionBounds = GetIntersection(waterTriggers[i].bounds, colliderBounds);
                boundVolumeSubmerged = intersectionBounds.extents.x * intersectionBounds.extents.y * intersectionBounds.extents.z;
            }
        }

        return boundVolumeSubmerged / boundVolume;
    }

    private Vector3 GetColliderWorldCenter(Collider collider)
    {
        return collider switch
        {
            BoxCollider boxCollider => boxCollider.transform.TransformPoint(boxCollider.center),
            SphereCollider sphereCollider => sphereCollider.transform.TransformPoint(sphereCollider.center),
            CapsuleCollider capsuleCollider => capsuleCollider.transform.TransformPoint(capsuleCollider.center),
            MeshCollider meshCollider => meshCollider.bounds.center,
            _ => collider.transform.position
        };
    }

    private float AproximateCrossSectionalArea(Collider collider) //onjly needs to be calleed once cause it doesnt include roation
    {
        Vector3 localScale = collider.transform.lossyScale;

        switch (collider)
        {
            case BoxCollider boxCollider: return Mathf.Max(boxCollider.size.x * localScale.x, boxCollider.size.z * localScale.z) * boxCollider.size.y * localScale.y;
            case SphereCollider sphereCollider: return Mathf.PI * Mathf.Pow(sphereCollider.radius * localScale.x, 2f);
            case CapsuleCollider capsuleCollider: return Mathf.PI * Mathf.Pow(capsuleCollider.radius * localScale.x, 2f) + 2f * capsuleCollider.radius * localScale.x * capsuleCollider.height * localScale.y;
            default: Debug.LogError("Collider not supported for cross-sectional calculations");
                return 0f;
        }
    }

    private Vector3 CalculateDragForce(float fluidDensity, float dragCoefficient, float crossSection, Vector3 velocity)
    {
        return -0.5f * fluidDensity * dragCoefficient * crossSection * Mathf.Pow(velocity.magnitude, 2f) * velocity.normalized;

    }

    //old methods-------------------------------------------------------------------
    private List<ColliderData> GetSubmergedColliders() //submersion is based on if it intersects
    {
        List<ColliderData> submergedColliders = new List<ColliderData>();
        for(int x = 0; x < colliderData.Count; x++)
        {
            for(int y = 0; y < waterTriggers.Count; y++)
            {
                if(waterTriggers[y].bounds.Intersects(colliderData[x].Collider.bounds))
                {
                    submergedColliders.Add(colliderData[x]);
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
            for(int j = 0; j < waterTriggers.Count; j++)
            {
                //find, for ratio of collider volumme, the size of the intersection
                Bounds intersectionBounds = GetIntersection(submergedColliders[i].bounds, waterTriggers[j].bounds);
                float intersectionVolume = intersectionBounds.extents.x * intersectionBounds.extents.y * intersectionBounds.extents.z;
                float objectVolume = submergedColliders[i].bounds.extents.x * submergedColliders[i].bounds.extents.y * submergedColliders[i].bounds.extents.z;

                float thisSubVolume = (intersectionVolume/objectVolume) * ColliderVolume(submergedColliders[i]);
                totalVolume += thisSubVolume;
            }
        }

        return totalVolume;
    }

    private Vector3 SubmergedBoundCenter(List<Collider> submergedColliders)
    {
        // list of all submergedPoints on the colliders
        List<Vector3> submergedPoints = new List<Vector3>();
        if(showGizmo)
        {
            gizmoPointMap = new Vector3[submergedColliders.Count][];
        }
        for (int i = 0; i < submergedColliders.Count; i++)
        {
            Vector3[] corners = GetBoundsCorners(submergedColliders[i].bounds);

            if(showGizmo)
            {
                gizmoPointMap[i] = corners;
            }

            List<Vector3> thisSubPoints = SubmergedPointsList(corners);

            for(int j = 0; j < thisSubPoints.Count; j++)
            {
                submergedPoints.Add(thisSubPoints[j]);
            }
        }

        //average all submerged points ASSUMES EVEN POINT DISTRIBUTION
        Vector3 submersionCenter = Vector3.zero;
        if (submergedPoints.Count > 0)
        {
            for (int i = 0; i < submergedPoints.Count; i++)
            {
                submersionCenter += submergedPoints[i];
            }
            submersionCenter = submersionCenter / submergedPoints.Count;
        }
        else
        {
            submersionCenter = transform.TransformPoint(rb.centerOfMass); //default to the center of mass of the object
        }

        ////fetch the center of submersion for all submerged colliders and average them
        //Vector3 submersionCenter = Vector3.zero;
        //if (submergedColliders.Count > 0 & waterTriggers.Count > 0)
        //{
        //    for (int i = 0; i < submergedColliders.Count; i++)
        //    {
        //        for (int j = 0; j < waterTriggers.Count; j++)
        //        {
        //            submersionCenter += GetIntersection(submergedColliders[i].bounds, waterTriggers[j].bounds).center;
        //        }
        //    }
        //    submersionCenter = submersionCenter / (submergedColliders.Count * waterTriggers.Count);
        //}
        //else
        //{
        //    submersionCenter = transform.TransformPoint(rb.centerOfMass); //default to the center of mass of the object
        //}


        return submersionCenter;
    }

    private float ArchimedesBouyantForce(float displacedVolume, float fluidDensity)
    {
        return -1f * fluidDensity * Physics.gravity.y * displacedVolume;
    }

    private Vector3 HydroDrag(float submergedVolume, float statDrag, float dynDrag)
    {
        float submersionFraction = submergedVolume / wholeVolume;

        Vector3 momentum = rb.mass * rb.velocity * statDrag; // momentum = m*v for static drag ///USING MASS IN DRAG DOWSNT MAKE SENSE, REID
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

    Vector3[] GetBoundsCorners(Bounds bounds)
    {
        Vector3[] corners = new Vector3[343];

        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

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

    private Bounds GetIntersection(Bounds a, Bounds b)
    {
        Vector3 min = Vector3.Max(a.min, b.min);
        Vector3 max = Vector3.Min(a.max, b.max);

        if (min.x < max.x && min.y < max.y && min.z < max.z)
        {
            return new Bounds((min + max) / 2, max - min);
        }
        else
        {
            // Return an empty bounds if there is no intersection
            return new Bounds();
        }
    }

    private Vector3 gizmoCenterOfSubmersion;
    private Vector3[][] gizmoPointMap;
    private void OnDrawGizmos()
    {
        if(showGizmo )
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(gizmoCenterOfSubmersion, 0.1f);

            if(gizmoPointMap != null)
            {
                for (int x = 0; x < gizmoPointMap.Length; x++)
                {
                    for (int y = 0; y < gizmoPointMap[x].Length; y++)
                    {
                        Gizmos.color = Color.gray;
                        Gizmos.DrawWireCube(gizmoPointMap[x][y], new Vector3(0.05f, 0.05f, 0.05f));
                    }
                }
            }
        }
    }

    private class ColliderData
    {
        public Collider Collider { get; }
        public float Volume { get; }
        public float CrossSection { get; }
        public float FractionSubmerged { get; set; } //value between 0 and 1
        public bool Submerged { get; set; }

        public ColliderData(Collider collider, float volume, float crossSection)
        {
            Collider = collider;
            Volume = volume;
            CrossSection = crossSection;
            FractionSubmerged = 0f;
            Submerged = false;
        }
    }
}

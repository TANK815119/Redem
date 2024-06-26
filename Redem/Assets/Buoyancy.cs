using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// creates the physics for acurate movement in bodies of water

[RequireComponent(typeof(Rigidbody))]
public class Buoyancy : MonoBehaviour
{
    [SerializeField] private float staticDrag = 5f;
    [SerializeField] private float angularDragCoefficient = 5f;

    private List<Collider> waterTriggers;
    private List<ColliderData> colliderData;
    private Rigidbody rb;
    private bool floating = false;
    private float submergedCrossSection = 0f;
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
            wholeCrossSection += colliderData[i].CrossSection;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (floating)
        {
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
                    ApplyHydraulicDrag(colliderData[i], staticDrag, 1000f); //hard coded fluid density
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
                ApplyHydraulicDrag(colliderData[i], staticDrag, 1.25f); //hard coded fluid density

            }
        }
    }

    //new methods-----------------------------------------------------------------

    private void UpdateColliderData(ColliderData colData)
    {
        colData.FractionSubmerged = GetFractionSubmerged(colData.Collider.bounds);
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

    private void ApplyHydraulicDrag(ColliderData colData, float statDrag, float fluidDensity)
    {
        //caclulate positional vector from purely rigidbody
        Vector3 positionalVelocity = rb.velocity;

        //calculate tangential velocity at the center of the collider
        Vector3 relativePosition = GetColliderWorldCenter(colData.Collider) - (transform.position + rb.centerOfMass);
        Vector3 rotationalVelocity = Vector3.Cross(rb.angularVelocity, relativePosition);

        //add the two forces together
        Vector3 totalVelocity = positionalVelocity + rotationalVelocity;

        //calculate drag
        float crossSectionArea = colData.CrossSection * colData.FractionSubmerged;
        Vector3 staticDragForce = CalculateDragForce(fluidDensity, statDrag, crossSectionArea, totalVelocity);

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

    private float ArchimedesBouyantForce(float displacedVolume, float fluidDensity)
    {
        return -1f * fluidDensity * Physics.gravity.y * displacedVolume;
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
            }
        }
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

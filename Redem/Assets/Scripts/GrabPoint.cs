using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPoint : MonoBehaviour
{
    [field: SerializeField] public bool SoftGrip { get; set; } // not implemented yet
    [field: SerializeField] public bool Grabbed { get; set; } // tracks if the player is grabbing it
    [field: SerializeField] public bool IsRightController { get; set; } // tracks if the player is grabbing it
    [field: SerializeField] public int GrabType { get; set; }
    [field: SerializeField] public Rigidbody ParentBody { get; private set; }
    //[field: SerializeField] public Vector3 ParentOffset { get; private set; }
    [field: SerializeField] public Transform ParentTrans { get; private set; }

    [SerializeField] private bool showGizmo = false;
    [SerializeField] [Range(0.0f, 1.0f)] private float gizmoScale = 1f;

    // Start is called before the first frame update
    void Start()
    {
        ParentTrans = transform.parent.parent;
        ParentBody = ParentTrans.GetComponent<Rigidbody>();
        //ParentOffset = transform.position - ParentTrans.position;
    }

    public Vector3 GetCurrParentOffset()
    {
        return transform.position - ParentTrans.position; ;
    }

    public Quaternion GetCurrParentRotationOffset()
    {
        return transform.rotation * Quaternion.Inverse(ParentTrans.rotation);
    }
    private void OnDrawGizmos()
    {
        if(showGizmo)
        {
            //draw a weird low-poly hand
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position - transform.up * 0.05f * gizmoScale, transform.position - transform.forward * 0.025f * gizmoScale - transform.up * 0.05f * gizmoScale); //arrow pointing up from mid hand
            Gizmos.DrawLine(transform.position, transform.position - transform.up * 0.1f * gizmoScale); //arrow pointing forward stem
            Gizmos.DrawLine(transform.position, transform.position - transform.up * 0.05f * gizmoScale - transform.right * 0.025f * gizmoScale); //arrow pointing forward side
            Gizmos.DrawLine(transform.position, transform.position - transform.up * 0.05f * gizmoScale + transform.right * 0.025f * gizmoScale); //arrow pointing forward side
        }
    }
}

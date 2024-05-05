using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPoint : MonoBehaviour
{
    [field: SerializeField] public bool SoftGrip { get; private set; } // not implemented yet
    [field: SerializeField] public int GrabType { get; private set; }
    [field: SerializeField] public Rigidbody ParentBody { get; private set; }
    [field: SerializeField] public Vector3 ParentOffset { get; private set; }
    [field: SerializeField] public Transform ParentTrans { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        ParentTrans = transform.parent.parent;
        ParentBody = ParentTrans.GetComponent<Rigidbody>();
        ParentOffset = transform.position - ParentTrans.position;
    }

    public Vector3 GetCurrParentOffset()
    {
        return transform.position - ParentTrans.position; ;
    }

    public Quaternion GetCurrParentRotationOffset()
    {
        return transform.rotation * Quaternion.Inverse(ParentTrans.rotation);
    }
}

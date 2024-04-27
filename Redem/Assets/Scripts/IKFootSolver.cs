using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
    [SerializeField] private Transform hip;
    [SerializeField] private Transform rotoball;
    [SerializeField] private float stepDistance;
    [SerializeField] private float stepHeight;
    [SerializeField] private float speed;
    [SerializeField] private float footOffset;
    [SerializeField] private IKFootSolver otherFoot;
    [SerializeField] private LayerMask mask;

    public bool stepping = false;

    private float footSpacing;
    private float lerp;
    private bool ungrounded = true;

    private Vector3 oldPosition;
    private Vector3 newPosition;
    private Rigidbody hipBody;
    // Start is called before the first frame update
    void Start()
    {
        footSpacing = transform.position.x - hip.position.x;
        hipBody = hip.gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //calculate next foot placement
        float maxDist = Mathf.Abs(hip.position.y - rotoball.position.y) + 0.3f;
        Ray ray = new Ray(hip.position + (hip.right * footSpacing) + (Vector3.up * 0.2f), Vector3.down);
        if(Physics.Raycast(ray, out RaycastHit info, Mathf.Infinity, mask))
        {
            float fastMod = 1f;
            if (fastMod < hipBody.velocity.magnitude * 0.5f) { fastMod = hipBody.velocity.magnitude * 0.5f; } //establishes speed floor
            if (!otherFoot.stepping && Vector3.Distance(newPosition, info.point) > stepDistance * fastMod)
            {
                lerp = 0;
                newPosition = info.point;
            }

            //check if player is in the air
            if(info.point.y < rotoball.position.y - 0.4f)
            {
                ungrounded = true;
            }
            else
            {
                ungrounded = false;
            }
        }

        //lerp foot from old to new foot position step-by-step
        Vector3 footPosition = Vector3.zero;
        if (lerp < 1 && !otherFoot.stepping)
        {
            footPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            footPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            stepping = true;
            float fastMod = 1f;
            if(fastMod < hipBody.velocity.magnitude) { fastMod = hipBody.velocity.magnitude; } //establishes speed floor
            lerp += Time.deltaTime * speed * fastMod;
        }
        else
        {
            stepping = false;
            footPosition = newPosition;
            oldPosition = newPosition;
        }

        //put feet in accordane to rotonall if not touching grounf
        if(ungrounded)
        {
            newPosition = rotoball.position + (hip.right * footSpacing) + (Vector3.down * 0.4f);
            footPosition = newPosition;
        }


        transform.position = footPosition + Vector3.up * footOffset;
    }
}

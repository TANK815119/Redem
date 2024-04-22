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
            if(!otherFoot.stepping && Vector3.Distance(newPosition, info.point) > stepDistance)
            {
                lerp = 0;
                newPosition = info.point;
            }

            //check if player is in the air
            if(info.transform.position.y < rotoball.position.y - 0.2f)
            {
                ungrounded = false;
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
            footPosition.y = Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            stepping = true;
            lerp += Time.deltaTime * speed * hipBody.velocity.magnitude;
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
            ungrounded = true;
            newPosition = rotoball.position + (hip.right * footSpacing) + (Vector3.down * 0.2f);
            footPosition = newPosition;
        }


        transform.position = footPosition + Vector3.up * footOffset;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
//shouyld purely be an input into the rorstate.
//nothing more

public class ControllerLocomotion : MonoBehaviour
{
    [SerializeField] private Transform headset;
    [SerializeField] private GameObject rotoball;
    [SerializeField] private float radius = 0.2f;
    [SerializeField] private float metersPerSecond = 1.5f;
    [SerializeField] private bool posZ = false;
    [SerializeField] private bool posX = false;

    private InputData inputData;
    private Rigidbody rotoBody;
    private Transform rotoTrans;
    private RotoState rotoState;

    // Start is called before the first frame update
    void Start()
    {
        inputData = rotoball.GetComponent<InputData>();
        rotoBody = rotoball.GetComponent<Rigidbody>();
        rotoTrans = rotoball.transform;
        rotoState = rotoball.GetComponent<RotoState>();
    }

    // Update is called once per frame
    void Update()
    {
        inputData.leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstick);

        if(posZ) { thumbstick.y = 1f; }
        if(posX) { thumbstick.x = 1f; }

        rotoState.IdealRotDelta(
            Mathf.Sin(Mathf.Deg2Rad * headset.eulerAngles.y) * thumbstick.y * metersPerSecond * Time.deltaTime +        //x-axis movement(parralel to view)
                Mathf.Cos(Mathf.Deg2Rad * headset.eulerAngles.y) * thumbstick.x * metersPerSecond * Time.deltaTime,     //x-axis movement(perpendicular to view)
            Mathf.Cos(Mathf.Deg2Rad * headset.eulerAngles.y) * thumbstick.y * metersPerSecond * Time.deltaTime +        //z-axis movement(parralel to view)
                Mathf.Sin(Mathf.Deg2Rad * headset.eulerAngles.y) * thumbstick.x * -metersPerSecond * Time.deltaTime,    //z-axis movement(perpendicular to view)
            radius);
    }
}

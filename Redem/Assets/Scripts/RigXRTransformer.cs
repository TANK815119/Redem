using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Rendering;

public class RigXRTransformer : MonoBehaviour
{
    [SerializeField] private Transform headset;
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;
    [SerializeField] private Transform head;
    [SerializeField] private float turnSpeed = 120f;

    private InputData inputData;
    private float turn;
    private Vector3 lastHeadsetPosition;
    private Vector3 headsetStack;
    // Start is called before the first frame update
    void OnEnable()
    {
        inputData = GetComponent<InputData>();
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        lastHeadsetPosition = new Vector3(0f, 0f, 0f);
        headsetStack = new Vector3(0f, 0f, 0f);
    }
    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    // Update is called once per frame
    void Update()
    {
        //TransformDevices();
    }
    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        TransformDevices();
    }

    void TransformDevices()
    {
        //headset
        if (inputData.headset.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 headsetPosition))
        {
            Vector3 deltaPosition = headsetPosition - lastHeadsetPosition;
            headsetStack = headsetStack + Quaternion.Euler(0f, turn, 0f) * deltaPosition;
            headset.position = headsetStack + new Vector3(head.position.x, head.position.y, head.position.z);
            lastHeadsetPosition = headsetPosition;
        }
        if (inputData.headset.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion headsetRotation))
        {
            //headset.localRotation = Quaternion.Euler(0f, turn, 0f) * headsetRotation;
        }

        //left controller
        if (inputData.leftController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftPosition))
        {
            leftController.position = Quaternion.Euler(0f, turn, 0f) *
                new Vector3(leftPosition.x - headsetPosition.x, 
                leftPosition.y - headsetPosition.y, 
                leftPosition.z - headsetPosition.z) + head.position;
        }
        if (inputData.leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion leftRotation))
        {
            leftController.localRotation = Quaternion.Euler(0f, turn, 0f) * leftRotation;
        }

        //rightController
        if (inputData.rightController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPosition))
        {
            rightController.position = Quaternion.Euler(0f, turn, 0f) * 
                new Vector3(rightPosition.x - headsetPosition.x, 
                rightPosition.y - headsetPosition.y, 
                rightPosition.z - headsetPosition.z) + head.position;
        }
        if (inputData.rightController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rightRotation))
        {
            rightController.localRotation = Quaternion.Euler(0f, turn, 0f) * rightRotation;
        }

        if (inputData.rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstick))
        {
            if (Mathf.Abs(thumbstick.x) > 0.5f)
            {
                turn += turnSpeed * thumbstick.x * Time.deltaTime;  //delta tiem ought to work, even here
                
            }
        }
    }
}

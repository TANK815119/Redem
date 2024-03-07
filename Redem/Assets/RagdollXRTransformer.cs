using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Rendering;


public class RagdollXRTransformer : MonoBehaviour
{
    [SerializeField] private Transform headset;
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;
    [SerializeField] private Transform hipTarget;
    [SerializeField] private BodyConfiguration bodyConfig;

    private InputData inputData;
    // Start is called before the first frame update
    void Start()
    {
        inputData = GetComponent<InputData>();
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    void Update()
    {
        UpdateHipTarget();
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
            headset.localPosition = headsetPosition;
        }
        if (inputData.headset.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion headsetRotation))
        {
            headset.localRotation = headsetRotation;
        }

        //left controller
        if (inputData.leftController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftPosition))
        {
            leftController.position = leftPosition;
        }
        if (inputData.leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion leftRotation))
        {
            leftController.localRotation = leftRotation;
        }

        //rightController
        if (inputData.rightController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPosition))
        {
            rightController.position = rightPosition;
        }
        if (inputData.rightController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rightRotation))
        {
            rightController.localRotation = rightRotation;
        }
    }
    private void UpdateHipTarget()
    {
        hipTarget.position = headset.position + bodyConfig.HipOffset();
        hipTarget.rotation = headset.rotation;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandAnimation : MonoBehaviour
{
    [SerializeField] bool isRightController;
    [SerializeField] Transform indexFinger;
    [SerializeField] Transform middleFinger;
    [SerializeField] Transform ringFinger;
    [SerializeField] Transform pinkyFinger;
    [SerializeField] Transform thumbFinger;

    private Transform[] indexArr;
    private Transform[] middleArr;
    private Transform[] ringArr;
    private Transform[] pinkyArr;
    private Transform[] thumbArr;
    private Transform[][] gripArr;

    private InputData inputData;

    [SerializeField] [Range(0.0f, 1.0f)] private float grip;
    [SerializeField] [Range(0.0f, 1.0f)] private float trigger;
    [SerializeField] [Range(0.0f, 1.0f)] private float thumb;
    // Start is called before the first frame update
    void Start()
    {
        indexArr = GetChildren(indexFinger);
        middleArr = GetChildren(middleFinger);
        ringArr = GetChildren(ringFinger);
        pinkyArr = GetChildren(pinkyFinger);
        thumbArr = GetChildren(thumbFinger);

        gripArr = new Transform[][] { middleArr, ringArr, pinkyArr };

        inputData = gameObject.GetComponent<InputData>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isRightController)
        {
            UpdateRightController();
        }
        else
        {
            UpdateLeftController();
        }

        HandPose();
    }

    private void HandPose()
    {
        //pose grip
        float gripRotation = grip * 90f;
        for (int x = 0; x < gripArr.Length; x++)
        {
            for(int y = 0; y < gripArr[x].Length; y++)
            {
                gripArr[x][y].localRotation = Quaternion.Euler(gripRotation, 0f, 0f);
            }
        }

        //pose trigger
        float indexRotation = trigger * 90f;
        for (int i = 0; i < indexArr.Length; i++)
        {
            indexArr[i].localRotation = Quaternion.Euler(indexRotation, 0f, 0f);
        }

        //pose thumb
        float thumbRotation = thumb * -60f;
        if(isRightController)
        {
            thumbArr[0].localRotation = Quaternion.Euler(thumb * 20f + 20f, 15.4f, 28.102f); //may need to be controlled by another float for grabs
            for (int i = 1; i < thumbArr.Length; i++) // note the int i = 1
            {
                thumbArr[i].localRotation = Quaternion.Euler(0f, 0f, thumbRotation + 15f);
            }
        }
        else
        {
            thumbArr[0].localRotation = Quaternion.Euler(thumb * 20f + 20f, -14.852f, -26.782f); //may need to be controlled by another float for grabs
            for (int i = 1; i < thumbArr.Length; i++) // note the int i = 1
            {
                thumbArr[i].localRotation = Quaternion.Euler(0f, 0f, -thumbRotation - 15f);
            }
        }
        
    } 

    private Transform[] GetChildren(Transform parent) // precondition a chain of 3 single children from parent
    {
        Transform[] childArr = { parent, parent.GetChild(0), parent.GetChild(0).GetChild(0), parent.GetChild(0).GetChild(0).GetChild(0) };
        return childArr;
    }

    private void UpdateRightController()
    {
        //grip values
        if (inputData.rightController.TryGetFeatureValue(CommonUsages.grip, out float controllerGrip))
        {
            if (controllerGrip > 0f)
            {
                if (grip >= 0.333f)
                {
                    grip = controllerGrip * (1 - 0.333f) + 0.333f; //smooshed range
                }
                else if (grip < 0.333f)
                {
                    grip += 5f * Time.deltaTime; //get into range
                }
            }
            if (controllerGrip == 0f && grip > 0f) //gert out of range
            {
                grip += -5f * Time.deltaTime;
            }
        }

        //trigger values
        if (inputData.rightController.TryGetFeatureValue(CommonUsages.trigger, out float controllerTrigger))
        {
            if (controllerTrigger > 0f)
            {
                if (trigger >= 0.333f)
                {
                    trigger = controllerTrigger * (1 - 0.333f) + 0.333f; //smooshed range
                }
                else if (trigger < 0.333f)
                {
                    trigger += 5f * Time.deltaTime; //get into range
                }
            }
            if (controllerTrigger == 0f && trigger > 0f) //gert out of range
            {
                trigger += -5f * Time.deltaTime;
            }
        }

        //thumb values
        if (inputData.rightController.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out bool controllerThumbTouch))
        {
            if (controllerThumbTouch && thumb < 1f)
            {
                thumb += 10f * Time.deltaTime;
            }
            if (!controllerThumbTouch && thumb > 0f)
            {
                thumb += -10f * Time.deltaTime;
            }
        }
    }

    private void UpdateLeftController()
    {
        //grip values
        if (inputData.leftController.TryGetFeatureValue(CommonUsages.grip, out float controllerGrip))
        {
            if (controllerGrip > 0f)
            {
                if (grip >= 0.333f)
                {
                    grip = controllerGrip * (1 - 0.333f) + 0.333f; //smooshed range
                }
                else if (grip < 0.333f)
                {
                    grip += 5f * Time.deltaTime; //get into range
                }
            }
            if (controllerGrip == 0f && grip > 0f) //gert out of range
            {
                grip += -5f * Time.deltaTime;
            }
        }

        //trigger values
        if (inputData.leftController.TryGetFeatureValue(CommonUsages.trigger, out float controllerTrigger))
        {
            if (controllerTrigger > 0f)
            {
                if (trigger >= 0.333f)
                {
                    trigger = controllerTrigger * (1 - 0.333f) + 0.333f; //smooshed range
                }
                else if (trigger < 0.333f)
                {
                    trigger += 5f * Time.deltaTime; //get into range
                }
            }
            if (controllerTrigger == 0f && trigger > 0f) //gert out of range
            {
                trigger += -5f * Time.deltaTime;
            }
        }

        //thumb values
        if (inputData.leftController.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out bool controllerThumbTouch))
        {
            if (controllerThumbTouch && thumb < 1f)
            {
                thumb += 10f * Time.deltaTime;
            }
            if (!controllerThumbTouch && thumb > 0f)
            {
                thumb += -10f * Time.deltaTime;
            }
        }
    }
}

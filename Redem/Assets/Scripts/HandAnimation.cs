using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandAnimation : MonoBehaviour
{
    [SerializeField] bool isRightController;
    [SerializeField] Transform handRoot;
    //[SerializeField] Transform indexFinger;
    //[SerializeField] Transform middleFinger;
    //[SerializeField] Transform ringFinger;
    //[SerializeField] Transform pinkyFinger;
    //[SerializeField] Transform thumbFinger;

    private Transform[] boneArr;
    private Vector3[] interArr;

    //"ideal" rotations
    private Vector3[] thumbArr;
    private Vector3[] indexArr;
    private Vector3[] middleArr;
    private Vector3[] ringArr;
    private Vector3[] pinkyArr;
    private Vector3[][] gripArr;
    private Vector3[][] handArr;

    private InputData inputData;

    [SerializeField] [Range(0.0f, 1.0f)] private float grip;
    [SerializeField] [Range(0.0f, 1.0f)] private float trigger;
    [SerializeField] [Range(0.0f, 1.0f)] private float thumb;

    [SerializeField] public bool Gripping { get; set; }
    [SerializeField] public int GripState{ get; set; }

    /*
    0 - flatGrip
    1 - cornerGrip
    2 - cylinderGrip
    3 - sphereGrip
    4 - planeGrip
    */

    // Start is called before the first frame update
    void Start()
    {
        thumbArr = new Vector3[4];
        indexArr = new Vector3[4];
        middleArr = new Vector3[4];
        ringArr = new Vector3[4];
        pinkyArr = new Vector3[4];
        gripArr = new Vector3[][] { middleArr, ringArr, pinkyArr };
        handArr = new Vector3[][] { thumbArr, indexArr, middleArr, ringArr, pinkyArr, };

        //large boneArr for every single bone Transform in the hand
        boneArr = new Transform[5*4]; // hard coded size of 20
        for(int x = 0; x < 5; x++) //5 is hard coded for 5 digits
        {
            Transform[] fingerArr = GetChildren(handRoot.GetChild(x));
            for(int y = 0; y < 4; y++)
            {
                boneArr[4 * x + y] = fingerArr[y];
            }
        }

        interArr = new Vector3[4 * 5]; //hard coded for size 20;

        inputData = gameObject.GetComponent<InputData>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isRightController)
        {
            UpdateRightController();
        }
        else
        {
            UpdateLeftController();
        }

        if (Gripping)
        {
            switch(GripState)
            {
                case 0: FlatGrip(); break;
                case 1: CornerGrip(); break;
                case 2: CylinderGrip(); break;
                case 3: SphereGrip(); break;
                case 4: PlaneGrip(); break;
                default: FlatGrip(); break;
            }
        }
        else
        {
            HandPose();
        }

        //smoothly transform the finger rotations into real Rotations
        InterpolateRotation(); //calculate transitional rotations for "smoothness"
        TransformRotation(); //using interpolated values
    }

    private void InterpolateRotation()
    {
        for(int x = 0; x < 5; x++) //hard-coded
        {
            for(int y = 0; y < 4; y++) // hard-coded
            {
                interArr[x * 4 + y].x = Mathf.Lerp(interArr[x * 4 + y].x, handArr[x][y].x, 25f * Time.deltaTime);
                interArr[x * 4 + y].y = Mathf.Lerp(interArr[x * 4 + y].y, handArr[x][y].y, 25f * Time.deltaTime);
                interArr[x * 4 + y].z = Mathf.Lerp(interArr[x * 4 + y].z, handArr[x][y].z, 25f * Time.deltaTime);
            }
        }
    }

    private void TransformRotation()
    {
        for(int i = 0; i < boneArr.Length; i++)
        {
            boneArr[i].localRotation = Quaternion.Euler(interArr[i]);
        }
    }

    private void HandPose()
    {
        //pose grip
        float gripRotation = grip * 90f;
        for (int x = 0; x < gripArr.Length; x++)
        {
            for(int y = 0; y < gripArr[x].Length; y++)
            {
                gripArr[x][y] = new Vector3(gripRotation, 0f, 0f);
            }
        }

        //pose trigger
        float indexRotation = trigger * 90f;
        for (int i = 0; i < indexArr.Length; i++)
        {
            indexArr[i] = new Vector3(indexRotation, 0f, 0f);
        }

        //pose thumb
        float thumbRotation = thumb * -60f;
        if(isRightController) 
        {
            thumbArr[0] = new Vector3(thumb * 20f + 20f, 15f, 25f); //may need to be controlled by another float for grabs
            for (int i = 1; i < thumbArr.Length; i++) // note the int i = 1
            {
                thumbArr[i] = new Vector3(0f, 0f, thumbRotation + 15f);
            }
        }
        else
        {
            thumbArr[0] = new Vector3(thumb * 20f + 20f, -15f, -25f); //may need to be controlled by another float for grabs
            for (int i = 1; i < thumbArr.Length; i++) // note the int i = 1
            {
                thumbArr[i] = new Vector3(0f, 0f, -thumbRotation - 15f);
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

    /*
    Every method past this point is essentially a custom hand animation,
    each one haveing a parameter indicating handdedness
    */

    private void FlatGrip()
    {
        //pose flat grip
        for (int x = 0; x < gripArr.Length; x++)
        {
            for (int y = 0; y < gripArr[x].Length; y++)
            {
                if(y == 2)
                {
                    gripArr[x][y] = new Vector3(-30f, 0f, 0f);
                }
                else
                {
                    gripArr[x][y] = new Vector3(15f, 0f, 0f);
                }
            }
        }

        //pose flat trigger
        for (int i = 0; i < indexArr.Length; i++)
        {
            if (i == 2)
            {
                indexArr[i] = new Vector3(-30f, 0f, 0f);
            }
            else
            {
                indexArr[i] = new Vector3(15f, 0f, 0f);
            }
        }

        //pose flat thumb
        if (isRightController)
        {
            thumbArr[0] = new Vector3(5f, 15f, 25f); //may need to be controlled by another float for grabs
            for (int i = 1; i < thumbArr.Length; i++) // note the int i = 1
            {
                thumbArr[i] = new Vector3(0f, 0f, 15f);
            }
        }
        else
        {
            thumbArr[0] = new Vector3(5f, -15f, -25f); //may need to be controlled by another float for grabs
            for (int i = 1; i < thumbArr.Length; i++) // note the int i = 1
            {
                thumbArr[i] = new Vector3(0f, 0f, -15f);
            }
        }
    }

    private void CornerGrip()
    {
        //pose corner grip
        for (int x = 0; x < gripArr.Length; x++)
        {
            for (int y = 0; y < gripArr[x].Length; y++)
            {
                gripArr[x][y] = new Vector3(45f, 0f, 0f);
            }
        }

        //pose corner trigger
        for (int i = 0; i < indexArr.Length; i++)
        {
            indexArr[i] = new Vector3(45f, 0f, 0f);
        }

        //pose corner thumb
        if (isRightController)
        {
            thumbArr[0] = new Vector3(5f, -25f, 15f); //may need to be controlled by another float for grabs
            for (int i = 1; i < thumbArr.Length; i++) // note the int i = 1
            {
                thumbArr[i] = new Vector3(0f, 0f, 0);
            }
        }
        else
        {
            thumbArr[0] = new Vector3(5f, 25f, 15f); //may need to be controlled by another float for grabs
            for (int i = 1; i < thumbArr.Length; i++) // note the int i = 1
            {
                thumbArr[i] = new Vector3(0f, 0f, 0f);
            }
        }
    }

    private void CylinderGrip()
    {

    }

    private void SphereGrip()
    {

    }

    private void PlaneGrip()
    {

    }
}

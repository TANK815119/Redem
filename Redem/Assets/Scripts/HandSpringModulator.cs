using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSpringModulator : MonoBehaviour
{
    [SerializeField] private Transform hand;
    [SerializeField] private Transform controller;
    [SerializeField] private Transform shoulder;
    [SerializeField] private float springMax = 99999f;
    [SerializeField] private float springMin = 500f;
    [SerializeField] private float angularSpringMax = 9999f;
    [SerializeField] private float angularSpringMin = 50f;
    [SerializeField] private float modulatorMult = 2f;
    [SerializeField] private bool useShoulderDistance = true;

    private ConfigurableJoint joint;
    private Vector3 offset;
    private float maxArmDistance;
    // Start is called before the first frame update
    void Start()
    {
        joint = hand.gameObject.GetComponent<ConfigurableJoint>();
        joint.rotationDriveMode = RotationDriveMode.Slerp; //force slerp
        offset = joint.connectedAnchor;
        maxArmDistance = Vector3.Distance(controller.position, shoulder.position);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
       if(useShoulderDistance)
       {
            ShoulderControllerModulator();
       }
       else
       {
            HandCOntrollerModulator();
       }
    }

    private void ShoulderControllerModulator()
    {
        //fetch the drives
        JointDrive drive = joint.xDrive;
        JointDrive angularDrive = joint.slerpDrive;

        float relativeDistance = Vector3.Distance(controller.position, shoulder.position) - maxArmDistance;
        if(relativeDistance < 0)
        {
            relativeDistance = 0f;
        }
        else
        {
            //Debug.Log("modulating");
        }

        float modulator = 2f * modulatorMult * (float)System.Math.Tanh(relativeDistance);
        drive.positionSpring = springMax - springMax * modulator;
        angularDrive.positionSpring = angularSpringMax - angularSpringMax * modulator;

        if (drive.positionSpring < springMin)
        {
            drive.positionSpring = springMin;
            angularDrive.positionSpring = angularSpringMin;
        }

        //apply to the joint
        joint.xDrive = drive;
        joint.yDrive = drive;
        joint.zDrive = drive;
        joint.slerpDrive = angularDrive;
    }

    private void HandCOntrollerModulator()
    {
        //fetch the drives
        JointDrive drive = joint.xDrive;
        JointDrive angularDrive = joint.slerpDrive;

        //manipulate the spring force based on distance
        float modulator = modulatorMult * (float)System.Math.Tanh(Vector3.Distance(controller.position, hand.position + offset));
        drive.positionSpring = springMax - springMax * modulator;
        angularDrive.positionSpring = angularSpringMax - angularSpringMax * modulator;

        if (drive.positionSpring < springMin)
        {
            drive.positionSpring = springMin;
            angularDrive.positionSpring = angularSpringMin;
        }

        //apply to the joint
        joint.xDrive = drive;
        joint.yDrive = drive;
        joint.zDrive = drive;
        joint.slerpDrive = angularDrive;
    }
}

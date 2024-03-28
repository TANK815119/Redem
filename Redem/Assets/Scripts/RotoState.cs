using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this script will store a rotation, the current rotation, and how to match
//unlike a simple quaternion, this script will allows multiple rotation to achieve and end.
//# rotations will be stores on x and z axis
//inputs of distance will change the idealRot
//inputs of (delta)rotation will change the realRot;
//another method will return the values on the x and z axis the ball must rotate to achieve the ideal
//a 1 on either axis indicates one full rotation
public class RotoState : MonoBehaviour
{
    [SerializeField] float proportionalGain;
    [SerializeField] float derrivativeGain;
    [SerializeField] float integralGain;

    public Vector3 idealRot = Vector3.zero; // x is around the x-axis, z is around the z axis, y is unused
    public Vector3 realRot = Vector3.zero;

    //take distance and convert it to
    //# of rotations to get there
    private float lastError;
    private float integrationStored;
    public void IdealRotDelta(float xDistDelta, float zdistDelta, float radius)
    {
        //convert distance to portion of circumferece
        float circumX = xDistDelta / (2 * Mathf.PI * radius);
        float circumZ = zdistDelta / (2 * Mathf.PI * radius);

        //add that circumference portion to the idealRotation
        idealRot.x += circumZ; //movement on the z axis is rotation on the x
        idealRot.z += circumX; //movement on the X axis is rotation on the Z
    }

    //will take the change in rotation every update
    //this is then translated into the realRotation
    //this method should only be called in one update, likely roomspaceLocomotion
    public void RealRotDelta(float xAnlgeDelta, float zAnlgeDelta, float radius)
    {
        //more transformation may be nessesary when the 180 to -180 threshold is passed as\
        //this may spit out a deltaAngle which is far too large.
        
        realRot.x += xAnlgeDelta / 360f;
        realRot.z += -zAnlgeDelta / 360f;
    }

    public Vector3 RotationDirection()
    {
        //find difference
        float xRotDiff = idealRot.x - realRot.x;
        float zRotDiff = idealRot.z - realRot.z;

        //get proportion so each is between 0 and 1
        Vector3 rotDirection = new Vector3(xRotDiff, 0f, zRotDiff).normalized;

        //Debug.Log(rotDirection);

        return rotDirection;
    }

    public float PIDTorque(float fixedDeltaTime)
    {
        //calculate the error
        float error = (idealRot - realRot).magnitude;

        //calculate proportional
        float propotional = error * proportionalGain;

        //calculate derrivate
        //i probably dont have to worry about derrivative kick
        float errorDerrivative = (error - lastError) / fixedDeltaTime;
        lastError = error;
        float derrivative = errorDerrivative * derrivativeGain;

        //calculate integral
        integrationStored += error * fixedDeltaTime;
        float integral = integrationStored * integralGain;

        return propotional + derrivative + integral; // likely make this absolute values as direction is already known
    }
}

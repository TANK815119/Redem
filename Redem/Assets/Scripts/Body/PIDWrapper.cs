using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PIDWrapper : MonoBehaviour
{
    [SerializeField] private float proportionalGain;
    [SerializeField] private float derrivativeGain;
    [SerializeField] private float integralGain;
    [SerializeField] private bool useVelocity = true;

    private PIDController PIDX;
    private PIDController PIDZ;
    // Start is called before the first frame update
    void Start()
    {
        PIDX = gameObject.AddComponent<PIDController>();
        PIDZ = gameObject.AddComponent<PIDController>();

        PIDX.setPIDProperties(proportionalGain, -derrivativeGain, integralGain, useVelocity);
        PIDZ.setPIDProperties(proportionalGain, derrivativeGain, integralGain, useVelocity);
    }

    // Update is called once per frame
    void Update()
    {
        // will delte latter for performance
        PIDX.setPIDProperties(proportionalGain, -derrivativeGain, integralGain, useVelocity);
        PIDZ.setPIDProperties(proportionalGain, derrivativeGain, integralGain, useVelocity);
    }

    public void IdealRotDelta(float xDistDelta, float zDistDelta, float radius)
    {
        PIDX.IdealDelta(-xDistDelta, radius);
        PIDZ.IdealDelta(zDistDelta, radius);
    }

    public void RealRotDelta(float xAngleDelta, float zAngleDelta, float radius)
    {
        PIDX.RealDelta(zAngleDelta, radius);
        PIDZ.RealDelta(xAngleDelta, radius);
    }

    public Vector3 PIDRotTorque(float fixedDeltaTime, float xVelocity, float zVelocity)
    {
        return new Vector3(PIDZ.PIDTorque(fixedDeltaTime, zVelocity), 0f, PIDX.PIDTorque(fixedDeltaTime, xVelocity));
    }
}

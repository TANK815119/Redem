using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rekabsen
{
    public class TargetLimb : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float innitialDisableMoment = 0.1f;
        private ConfigurableJoint configurableJoint;
        private Quaternion initial;

        void Start()
        {
            this.configurableJoint = this.GetComponent<ConfigurableJoint>();
            this.initial = this.target.transform.localRotation;
        }

        private void FixedUpdate()
        {
            if (innitialDisableMoment <= 0f)
            {
                configurableJoint.targetRotation = copyLimb();
            }
            else
            {
                innitialDisableMoment -= Time.fixedDeltaTime;
            }
        }

        private Quaternion copyLimb()
        {
            return Quaternion.Inverse(this.target.localRotation) * this.initial;
        }

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Firearm : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool triggerPresssed;
    [SerializeField] private bool automatic;
    [SerializeField] private float recoil = 20f;
    [SerializeField] private float bulletMassScale = 1f;
    [SerializeField] private List<GrabPoint> pistolGrips;
    [SerializeField] private Transform recoilOrigin;
    [SerializeField] private Transform casingOrigin;
    [SerializeField] private GameObject spentCasing;
    [SerializeField] private GameObject bullet;
    

    private InputData inputData;
    private Rigidbody gunBody;

    private bool hammerBack = true;
    private bool hammerReleased = false;
    // Start is called before the first frame update
    void Start()
    {
        inputData = gameObject.GetComponent<InputData>();
        gunBody = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        triggerPresssed = TriggerPressState();

        //get animation state
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        //state.IsName("m1a1_fire")
        if (triggerPresssed)
        {
            //update animations
            animator.SetBool("fire", true);
            animator.SetBool("bolt open", false);

            //firing logic
            if (state.IsName("m1a1_fire"))
            {
                FireUpdate(state.normalizedTime % 1f);
            }
        }
        else
        {
            //update animations
            animator.SetBool("bolt open", true);
            animator.SetBool("fire", false);
            hammerBack = true;
        }
        
    }

    private void FireUpdate(float boltDist) // bolt distance back; 0=min; 1=max
    {
        //release hammer if bolt is forward, the hammer is back, and not released
        if (hammerBack && !hammerReleased && boltDist < 0.70f && boltDist > 0.40f)
        {
            hammerReleased = true;
        }

        //fire the weapon if the hammer is released
        if (hammerReleased)
        {
            FireBullet();
            hammerBack = false;
            hammerReleased = false;
        }

        //recharge the hammer if it goes sufficiently back(automatic firing group)
        if (automatic && !hammerBack && boltDist >= 0.70f)
        {
            hammerBack = true;
        }
    }

    private bool TriggerPressState()
    {
        //check if one of the grips is doin stuff
        bool trigPres = false;
        for (int i = 0; i < pistolGrips.Count; i++)
        {
            if (pistolGrips[i].Grabbed && pistolGrips[i].IsRightController)
            {
                if (inputData.rightController.TryGetFeatureValue(CommonUsages.trigger, out float controllerTrigger) && controllerTrigger > 0.75f)
                {
                    trigPres = true;
                }
            }
            else if (pistolGrips[i].Grabbed && !pistolGrips[i].IsRightController)
            {
                if (inputData.leftController.TryGetFeatureValue(CommonUsages.trigger, out float controllerTrigger) && controllerTrigger > 0.75f)
                {
                    trigPres = true;
                }
            }
        }
        return trigPres;
    }

    private void FireBullet()
    {
        //audio
        audioSource.PlayOneShot(audioSource.clip);

        //fire projectile
        Rigidbody bulletBody = Instantiate(bullet, recoilOrigin.position, recoilOrigin.rotation).GetComponent<Rigidbody>();
        bulletBody.velocity = gunBody.velocity;
        bulletBody.mass *= bulletMassScale;
        bulletBody.AddForce(bulletBody.transform.forward * recoil, ForceMode.Impulse);

        //recoil
        //Vector3 force = (recoilOrigin.position - gunBody.transform.position).normalized * -recoil;
        Vector3 force = gunBody.transform.forward * -recoil;
        Vector3 position = recoilOrigin.position;
        gunBody.AddForceAtPosition(force, position, ForceMode.Impulse);

        //spent casing
        Rigidbody casingBody =  Instantiate(spentCasing, casingOrigin.position, casingOrigin.rotation).GetComponent<Rigidbody>();
        casingBody.velocity = gunBody.velocity;
        casingBody.AddForce(casingBody.transform.right * 0.03f + casingBody.transform.up * 0.03f, ForceMode.Impulse);
        //casingBody.AddExplosionForce(0.03f, gunBody.position, 1f, 0f, ForceMode.Impulse);
    }
}

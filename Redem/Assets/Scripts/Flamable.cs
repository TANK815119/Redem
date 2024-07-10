using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Rekabsen
{
    [RequireComponent(typeof(Collider))] //so that it may be detected by any Triboluminescent objects
    public class Flamable : NetworkBehaviour
    {
        [SerializeField] private GameObject flame;
        [SerializeField] private List<Transform> burnPoints;
        [SerializeField] private AudioSource fireCrackle;
        [SerializeField] private AudioClip fireLite;
        private List<Transform> flames;

        private bool burning = false;
        private NetworkVariable<bool> lit = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        // Start is called before the first frame update
        void Start()
        {
            flames = new List<Transform>();

            //check the object has a collider
            GetComponent<Collider>();
        }

        // Update is called once per frame
        void Update()
        {
            if (lit.Value && !burning)
            {
                Combust();
            }

            if (burning)
            {
                //"parent" burning stuff
                for (int i = 0; i < burnPoints.Count; i++)
                {
                    flames[i].position = burnPoints[i].position;
                }
            }
        }

        private void Combust()
        {
            AudioSource.PlayClipAtPoint(fireLite, transform.position, 1f);
            fireCrackle.Play();
            for (int i = 0; i < burnPoints.Count; i++)
            {
                GameObject fire = Instantiate(flame);
                flames.Add(fire.transform);
                burning = true;
            }
        }

        public void Light()
        {
            lit.Value = true;
        }

        public bool IsBurning()
        {
            return burning;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Flamable flamable = collision.gameObject.GetComponent<Flamable>();
            if (flamable != null && !flamable.IsBurning() && burning)
            {
                flamable.Light();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.TryGetComponent(out Mouth mouth))
            {
                mouth.GetStats().NearFire = true;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent(out Mouth mouth))
            {
                mouth.GetStats().NearFire = false;
            }
        }
    }
}
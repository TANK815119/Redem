using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Rekabsen
{
    public class Mouth : NetworkBehaviour
    {
        [SerializeField] private PlayerStatsNetwork stats; 
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float eatTime = 3f;
        private List<Edible> inMouth;
        private float eaten = 0f; //seconds eaten

        private NetworkVariable<bool> objectConsumed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private void Start()
        {
            inMouth = new List<Edible>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Edible edible))
            {
                inMouth.Add(edible);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out Edible edible) && inMouth.Contains(edible))
            {
                inMouth.Remove(edible);
            }
        }

        private void Update()
        {
            //try to eat the first object in the list
            if(inMouth.Count > 0)
            {
                //increment eating
                eaten += Time.deltaTime;

                //sound
                if(!audioSource.isPlaying)
                {
                    audioSource.Play();
                }

                //check if eaten
                if(eaten >= eatTime)
                {
                    objectConsumed.Value = true;

                    //attempt consumption
                    if (objectConsumed.Value && IsHost)
                    {
                        AttemptConsumption(inMouth[0]);
                    }

                    eaten = 0f;
                }
            }
            else
            {
                eaten = 0f;

                //sound
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }

        private void AttemptConsumption(Edible edible)
        {
            //increase player hunger stat
            stats.Feed(edible.Nourishment);

            //spawn leftowvers
            GameObject leftovers = Instantiate(edible.Leftovers, transform.position, transform.rotation);
            if (leftovers.TryGetComponent(out NetworkObject netLeftover))
            {
                netLeftover.Spawn();
            }

            //destroy the first object
            inMouth.Remove(edible);
            if (edible.gameObject.TryGetComponent(out NetworkObject netEdible))
            {
                netEdible.Despawn(true);
                Destroy(edible.gameObject);
            }

            objectConsumed.Value = false;
        }

        public PlayerStatsNetwork GetStats()
        {
            return stats;
        }
    }
}
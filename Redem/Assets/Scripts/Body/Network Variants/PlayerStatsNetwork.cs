using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Rekabsen
{
    public class PlayerStatsNetwork : NetworkBehaviour
    {
        [SerializeField] private NetworkVariable<float> health = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        [SerializeField] private NetworkVariable<float> hunger = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        [SerializeField] private NetworkVariable<float> temp = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


        [SerializeField] private DisplayStatsOnTexture statDisplay;

        private float maxHealth;
        private float maxHunger;
        private float maxTemp;

        //health stats
        [SerializeField] private float hurtPerSecond = 100f / (6f * 60f); //lose all health every 6 minutes
        [SerializeField] private float hungerPerSecond = 100f / (12f * 60f); //lose all health every 12 minutes

        [SerializeField] AudioSource hungerGrowl;


        // Start is called before the first frame update
        void Start()
        {
            maxHealth = health.Value;
            maxHunger = hunger.Value;
            maxTemp = temp.Value;
        }

        // Update is called once per frame
        void Update()
        {
            bool starving = HungerUpdate();
            bool cold = TempUpdate();
            HealthUpdate(starving, cold);
            CheckDisplay();
        }

        private bool HungerUpdate()
        {
            bool starving = false;

            if (hunger.Value > 0f)
            {
                hunger.Value -= hungerPerSecond * Time.deltaTime;
                starving = false;
            }
            else
            {
                starving = true;
            }

            return starving;
        }

        private bool TempUpdate()
        {
            return false;
        }

        private void HealthUpdate(bool starving, bool cold)
        {
            if (health.Value > 0f)
            {
                float totalHurt = 0f;
                if (starving)
                {
                    totalHurt += hurtPerSecond * Time.deltaTime;
                }
                if (cold)
                {
                    totalHurt += hurtPerSecond * Time.deltaTime;
                }

                health.Value -= totalHurt;
            }
            else
            {
                Die();
            }
        }

        private void Die()
        {
            //make player go completely limp
            Debug.Log("player is dead!");
        }

        private int lastHealthSegments = 0;
        private int lastHungerSegments = 0;
        private int lastTempSegments = 0;
        private void CheckDisplay() //uodate display when a segment is lost
        {
            //calculate segments
            int healthSegments = (int)(health.Value / (maxHealth / 8f));
            int hungerSegments = (int)(hunger.Value / (maxHealth / 8f));
            int tempSegments = (int)(temp.Value / (maxHealth / 8f));

            //check for change
            bool segmentChange = false;
            if (healthSegments != lastHealthSegments) { segmentChange = true; }
            if (hungerSegments != lastHungerSegments) { segmentChange = true; hungerGrowl.Play(); }
            if (tempSegments != lastTempSegments) { segmentChange = true; }

            //update stats if changed
            if (segmentChange)
            {
                statDisplay.UpdateStatsSegments(healthSegments, hungerSegments, tempSegments);
                statDisplay.UpdateStatshDisplay();
            }

            //update last values
            lastHealthSegments = healthSegments;
            lastHungerSegments = hungerSegments;
            lastTempSegments = tempSegments;
        }

        public void Feed(float noursishment)
        {
            if(hunger.Value + noursishment <= maxHunger)
            {
                hunger.Value += noursishment;
            }
            else //extra goes to health
            {
                float healthIncrease = hunger.Value + noursishment - 100f;
                hunger.Value = 100f; //top off

                //icrease health
                if(health.Value + healthIncrease <= maxHealth)
                {
                    health.Value += healthIncrease;
                }
                else
                {
                    health.Value = 100f;
                }
            }
        }
    }
}
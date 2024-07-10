using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//changes the skybox every 12 minutes
//and dims the sun
namespace Rekabsen
{
    public class DayNightCycle : MonoBehaviour
    {
        [SerializeField] private List<MeshRenderer> holoWalls;
        [SerializeField] private Material dayMaterial;
        [SerializeField] private Material nightMaterial;

        [SerializeField] private Light sun;

        private bool daytime = true;
        [SerializeField] private float dayLength = 60f * 12f; //12 minutes
        private float timeCounter;

        [SerializeField] AudioSource switchSound;

        // Start is called before the first frame update
        void Start()
        {
            timeCounter = dayLength;
        }

        // Update is called once per frame
        void Update()
        {
            timeCounter -= Time.deltaTime;

            if(timeCounter <= 0f)
            {
                if(daytime)
                {
                    MakeNightTime();
                }
                else
                {
                    MakeDayTime();
                }

                //audio
                switchSound.Play();

                timeCounter = dayLength;
            }
        }

        private void MakeDayTime()
        {
            for(int i = 0; i < holoWalls.Count; i++)
            {
                holoWalls[i].material = dayMaterial;
            }

            sun.intensity = 1f;
            SetAllPlayersDay(true);


            daytime = true;
        }

        private void MakeNightTime()
        {
            for (int i = 0; i < holoWalls.Count; i++)
            {
                holoWalls[i].material = nightMaterial;
            }

            sun.intensity = 0.05f;
            SetAllPlayersDay(false);

            daytime = false;
        }

        private void SetAllPlayersDay(bool day)
        {
            PlayerStatsNetwork[] playerStats = Object.FindObjectsByType<PlayerStatsNetwork>(FindObjectsSortMode.None);
            for(int i = 0; i < playerStats.Length; i++)
            {
                playerStats[i].IsDay = day;
            }
        }
    }
}
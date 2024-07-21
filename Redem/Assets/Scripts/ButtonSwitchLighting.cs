using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rekabsen
{
    public class ButtonSwitchLighting : MonoBehaviour, ButtonActionInterface
    {
        //dark
        [SerializeField] private Texture2D[] darkLightMapDir;
        [SerializeField] private Texture2D[] darkLightMapColor;
        [SerializeField] private LightProbeDataAsset darkLightProbes;

        //bright
        [SerializeField] private Texture2D[] brightLightMapDir;
        [SerializeField] private Texture2D[] brightLightMapColor;
        [SerializeField] private LightProbeDataAsset brightLightProbes;

        [SerializeField] private bool debugSwitch = false;
        private bool lastDebugSwitch = false;

        private bool bright = true;
        private LightmapData[] darkLightmap;
        private LightmapData[] brightLightmap;

        // Start is called before the first frame update
        void Start()
        {
            darkLightmap = LightmapDataExtract(darkLightMapDir, darkLightMapColor);
            brightLightmap = LightmapDataExtract(brightLightMapDir, brightLightMapColor);
        }

        void Update()
        {
            if(debugSwitch != lastDebugSwitch)
            {
                Play();
                lastDebugSwitch = debugSwitch;
            }
        }

        public void Play()
        {
            if(bright)
            {
                //switch to dark
                LightmapSettings.lightmaps = darkLightmap;
                LightmapSettings.lightProbes.bakedProbes = darkLightProbes.coefficients;
                Debug.Log("darkLightProbes contained " + darkLightProbes.coefficients.Length + " coefficients");
            }
            else
            {
                //switch to light
                LightmapSettings.lightmaps = brightLightmap;
                LightmapSettings.lightProbes.bakedProbes = brightLightProbes.coefficients;
                Debug.Log("brightLightProbes contained " + brightLightProbes.coefficients.Length + " coefficients");
            }

            bright = !bright;
        }

        private LightmapData[] LightmapDataExtract(Texture2D[] lightmapDir, Texture2D[] lightmapColor)
        {
            List<LightmapData> lightmapData = new List<LightmapData>();

            for (int i = 0; i < darkLightMapDir.Length; i++)
            {
                LightmapData mapData = new LightmapData();

                mapData.lightmapDir = lightmapDir[i];
                mapData.lightmapColor = lightmapColor[i];

                lightmapData.Add(mapData);
            }

            return lightmapData.ToArray();
        }    

        //public void UpdateDarkProbes()
        //{
        //    darkLightProbes.coefficients = LightmapSettings.lightProbes.bakedProbes;
        //    Debug.Log("Updated " + darkLightProbes.coefficients.Length + " probes");
        //}

        //public void UpdateBrightProbes()
        //{
        //    brightLightProbes.coefficients = LightmapSettings.lightProbes.bakedProbes;
        //    Debug.Log("Updated " + brightLightProbes.coefficients.Length + " probes");
        //}
    }
}
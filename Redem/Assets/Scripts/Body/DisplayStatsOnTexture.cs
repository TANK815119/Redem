using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rekabsen
{
    public class DisplayStatsOnTexture : MonoBehaviour
    {
        [SerializeField] private Texture2D baseTexture; // Base texture of the player's wrist
        [SerializeField] private Material playerMaterial; // Material applied to the player's wrist
        private Texture2D staticTexture; // Texture that will be modified

        [SerializeField] private int posX = 204;
        [SerializeField] private int posY = 1720;
        [SerializeField] private int width = 64;
        [SerializeField] private int height = 32;
        private int lostHealthSegments = 0;
        private int lostHungerSegments = 0;
        private int lostTempSegments = 0;

        void Start()
        {
            //Texture2D texture = playerMaterial.mainTexture as Texture2D;

            //if (texture == null || !texture.isReadable)
            //{
            //    Debug.LogError("Texture is not readable/writable.");
            //    return;
            //}

            //Texture2D newTexture = new Texture2D(texture.width, texture.height);
            //newTexture.SetPixels(texture.GetPixels());
            //staticTexture = newTexture;

            //// Create a new uncompressed texture the same size as the base texture
            staticTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.RGBA32, false);
            //Graphics.CopyTexture(playerMaterial.mainTexture, 0, 0, staticTexture, 0, 0);

            //copy every single picure
            Color[] colors = baseTexture.GetPixels();
            staticTexture.SetPixels(colors);

            //for (int x = 0; x < baseTexture.width; x++)
            //{
            //    for(int y = 0; y < baseTexture.height; y++)
            //    {
            //    }
            //}

            UpdateStatshDisplay();
        }

        private void Update()
        {

        }

        public void UpdateStatshDisplay()
        {
            //create a new texture based on static texure
            Texture2D dynamicTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.RGBA32, false);
            Graphics.CopyTexture(staticTexture, dynamicTexture);

            // Draw the health value on the texture
            DrawHealth(dynamicTexture);// 
            DrawHunger(dynamicTexture);
            DrawTemp(dynamicTexture);

            // Apply the changes
            dynamicTexture.Compress(true); //IDK if high quality

            dynamicTexture.Apply();

            playerMaterial.mainTexture = dynamicTexture;
        }

        private void DrawHealth(Texture2D dynamicTexture)
        {
            //black
            DrawSquare(dynamicTexture, new Vector2Int(posX, posY), new Vector2Int(width, height), Color.black);

            //red
            int buffer = 1;
            DrawSquare(dynamicTexture, new Vector2Int(posX + buffer, posY + buffer), new Vector2Int(width - buffer * 2, height - buffer * 2), Color.red);

            //8 black lines
            int thickness = 1;
            int segments = 8;
            for (int i = 0; i < segments; i++)
            {
                int localX = posX + (i + 1) * width / segments;
                DrawSquare(dynamicTexture, new Vector2Int(localX, posY), new Vector2Int(thickness, height), Color.black);
            }

            //overdraw lost health
            for (int i = 0; i < lostHealthSegments; i++)
            {
                int localX = posX + (i) * width / segments;
                DrawSquare(dynamicTexture, new Vector2Int(localX, posY), new Vector2Int(width / segments, height), Color.black);
            }
        }

        private void DrawHunger(Texture2D dynamicTexture)
        {
            int seperation = height + 2;

            //black
            DrawSquare(dynamicTexture, new Vector2Int(posX, posY + seperation), new Vector2Int(width, height), Color.black);

            //green
            int buffer = 1;
            DrawSquare(dynamicTexture, new Vector2Int(posX + buffer, posY + seperation + buffer), new Vector2Int(width - buffer * 2, height - buffer * 2), Color.green);

            //8 black lines
            int thickness = 1;
            int segments = 8;
            for (int i = 0; i < segments; i++)
            {
                int localX = posX + (i + 1) * width / segments;
                DrawSquare(dynamicTexture, new Vector2Int(localX, posY + seperation), new Vector2Int(thickness, height), Color.black);
            }

            //overdraw lost health
            for (int i = 0; i < lostHungerSegments; i++)
            {
                int localX = posX + (i) * width / segments;
                DrawSquare(dynamicTexture, new Vector2Int(localX, posY + seperation), new Vector2Int(width / segments, height), Color.black);
            }
        }

        private void DrawTemp(Texture2D dynamicTexture)
        {
            int seperation = height * 2 + 4;

            //black
            DrawSquare(dynamicTexture, new Vector2Int(posX, posY + seperation), new Vector2Int(width, height), Color.black);

            //green
            int buffer = 1;
            DrawSquare(dynamicTexture, new Vector2Int(posX + buffer, posY + seperation + buffer), new Vector2Int(width - buffer * 2, height - buffer * 2), Color.blue);

            //8 black lines
            int thickness = 1;
            int segments = 8;
            for (int i = 0; i < segments; i++)
            {
                int localX = posX + (i + 1) * width / segments;
                DrawSquare(dynamicTexture, new Vector2Int(localX, posY + seperation), new Vector2Int(thickness, height), Color.black);
            }

            //overdraw lost health
            for (int i = 0; i < lostTempSegments; i++)
            {
                int localX = posX + (i) * width / segments;
                DrawSquare(dynamicTexture, new Vector2Int(localX, posY + seperation), new Vector2Int(width / segments, height), Color.black);
            }
        }

        private void DrawSquare(Texture2D dynamicTexture, Vector2Int position, Vector2Int dimensions, Color color)
        {
            // Use a nested for loop to set each pixel to red
            for (int y = position.y; y < position.y + dimensions.y; y++)
            {
                for (int x = position.x; x < position.x + dimensions.x; x++)
                {
                    dynamicTexture.SetPixel(x, y, color);
                }
            }
        }

        public void UpdateStatsSegments(int health, int hunger, int temp)
        {
            lostHealthSegments = 8 - health;
            lostHungerSegments = 8 - hunger;
            lostTempSegments = 8 - temp;
        }
    }
}
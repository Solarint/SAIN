using UnityEngine;
using System.IO;
using static SAIN.Editor.EditorParameters;
using static SAIN.Editor.StyleOptions;
using static SAIN.Editor.RectLayout;

namespace SAIN.Editor
{
    internal class UITextures
    {
        public static Texture2D[] Backgrounds { get; private set; } = new Texture2D[TabCount];

        public static void LoadTextures()
        {
            string imagePath = "BepInEx/plugins/SAINUI/background.png"; // Specify the path of the saved texture

            byte[] textureBytes = File.ReadAllBytes(imagePath);
            Texture2D loadedTexture = new Texture2D(1296, 1728); // AddToScheme a new Texture2D

            loadedTexture.LoadImage(textureBytes);
            for (int i = 0; i < TabCount; i++)
            {
                var rotated = RotateTexture(loadedTexture);
                var recolored = RecolorTexture(rotated, -0.05f, -0.15f, -0.15f);
                Backgrounds[i] = recolored;
            }
        }

        private static Texture2D RotateTexture(Texture2D originalTexture)
        {
            int width = originalTexture.width;
            int height = originalTexture.height;

            Texture2D rotatedTexture = new Texture2D(width, height);

            // Generate a random rotation angle in degrees
            float rotationAngle = Random.Range(0f, 360f);

            // Calculate the pivot point for rotation
            Vector2 pivot = new Vector2(width / 2f, height / 2f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Calculate the DrawPosition relative to the pivot
                    Vector2 position = new Vector2(x, y) - pivot;

                    // Apply rotation to the DrawPosition
                    Vector2 rotatedPosition = Quaternion.Euler(0f, 0f, rotationAngle) * position;

                    // Calculate the final DrawPosition after rotation
                    Vector2 finalPosition = rotatedPosition + pivot;

                    // Get the pixel color from the original texture
                    Color pixel = originalTexture.GetPixel((int)finalPosition.x, (int)finalPosition.y);

                    // Set the pixel color in the rotated texture
                    rotatedTexture.SetPixel(x, y, pixel);
                }
            }

            rotatedTexture.Apply();
            return rotatedTexture;
        }

        private static Texture2D RecolorTexture(Texture2D originalTexture, float R, float G, float B)
        {
            int width = originalTexture.width;
            int height = originalTexture.height;

            Texture2D recoloredTexture = new Texture2D(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color originalPixel = originalTexture.GetPixel(x, y);

                    // Apply the color offsets to the RGB channels
                    float adjustedRed = originalPixel.r + R;
                    float adjustedGreen = originalPixel.g + G;
                    float adjustedBlue = originalPixel.b + B;

                    // Clamp the adjusted values to the range [0, 1]
                    adjustedRed = Mathf.Clamp01(adjustedRed);
                    adjustedGreen = Mathf.Clamp01(adjustedGreen);
                    adjustedBlue = Mathf.Clamp01(adjustedBlue);

                    // AddToScheme a new Color with adjusted channel values
                    Color adjustedPixel = new Color(adjustedRed, adjustedGreen, adjustedBlue, originalPixel.a);

                    recoloredTexture.SetPixel(x, y, adjustedPixel);
                }
            }

            recoloredTexture.Apply();

            return recoloredTexture;
        }

        public static Texture2D CreateTexture(int width, int height, int borderSize = 5, Color fillColor = default, Color borderColor = default)
        {
            fillColor = fillColor == default ? Color.white : fillColor;
            borderColor = borderColor == default ? Color.red : borderColor;

            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = texture.GetPixels();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x < borderSize || x >= width - borderSize || y < borderSize || y >= height - borderSize)
                    {
                        pixels[y * width + x] = borderColor;
                    }
                    else
                    {
                        pixels[y * width + x] = fillColor;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        public static Rect DrawSliderBackGrounds(float progress)
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            float lineHeight = 5f;
            float filledY = lastRect.y + (lastRect.height - lineHeight * 2f) * 0.5f;
            float sliderY = lastRect.y + (lastRect.height - lineHeight) * 0.5f;
            Rect Filled = new Rect(lastRect.x, filledY, lastRect.width * progress, lineHeight * 2f);
            Rect Background = new Rect(lastRect.x, sliderY, lastRect.width, lineHeight);

            GUI.DrawTexture(Background, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Color.white, 0, 0);
            GUI.DrawTexture(Filled, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(1f, 0.25f, 0.25f), 0, 0);
            return lastRect;
        }
    }
}

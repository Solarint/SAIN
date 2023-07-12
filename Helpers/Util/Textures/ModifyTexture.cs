using SAIN.Editor;
using UnityEngine;

namespace SAIN.Helpers.Textures
{
    internal static class ModifyTexture
    {
        public static Texture2D[] CreateNewBackgrounds(Recolor recolor, string path, int count = 4)
        {
            Texture2D texture = LoadTexture.Single(path);
            Texture2D[] Backgrounds = new Texture2D[count];

            for (int i = 0; i < count; i++)
            {
                float angle = Random.Range(45f, 290f);
                var rotated = RotateTexture(texture, angle);
                Backgrounds[i] = RecolorTexture(rotated, recolor.R, recolor.G, recolor.B);
            }
            return Backgrounds;
        }

        public static Texture2D RotateTexture(Texture2D originalTexture, float rotationAngle)
        {
            int width = originalTexture.width;
            int height = originalTexture.height;

            Texture2D rotatedTexture = new Texture2D(width, height);

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

        public static Texture2D RecolorTexture(Texture2D originalTexture, Color color)
        {
            return RecolorTexture(originalTexture, color.r, color.g, color.b);
        }

        public static Texture2D RecolorTexture(Texture2D originalTexture, float R, float G, float B)
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

                    // AddorUpdateColorScheme a new Color with adjusted channel values
                    Color adjustedPixel = new Color(adjustedRed, adjustedGreen, adjustedBlue, originalPixel.a);

                    recoloredTexture.SetPixel(x, y, adjustedPixel);
                }
            }

            recoloredTexture.Apply();

            return recoloredTexture;
        }

        public static Texture2D CreateTextureWithBorder(int width, int height, int borderSize = 5, Color fillColor = default, Color borderColor = default)
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

    }
}

using UnityEngine;

namespace SAIN.Editor.Util
{
    internal class Colors
    {
        static Colors()
        {
            LightRed = new Color(0.8f, 0.4f, 0.4f);
            TextureLightRed = UITextures.CreateTexture(2, 2, 0, LightRed);

            MidRed = new Color(0.7f, 0.3f, 0.3f);
            TextureMidRed = UITextures.CreateTexture(2, 2, 0, MidRed);

            DarkRed = new Color(0.6f, 0.2f, 0.2f);
            TextureDarkRed = UITextures.CreateTexture(2, 2, 0, DarkRed);

            LightGray = CreateGray(0.5f);
            TexLightGray = UITextures.CreateTexture(2, 2, 0, LightGray);

            MidGray = CreateGray(0.35f);
            TexMidGray = UITextures.CreateTexture(2, 2, 0, MidGray);

            TexDarkGray = UITextures.CreateTexture(2, 2, 0, DarkGray);
            DarkGray = CreateGray(0.2f);

            VeryDarkGray = CreateGray(0.1f);
            TexVeryDarkGray = UITextures.CreateTexture(2, 2, 0, VeryDarkGray);

            LightBlue = new Color(0.55f, 0.65f, 0.9f);
            TextureLightBlue = UITextures.CreateTexture(2, 2, 0, LightBlue);

            MidBlue = new Color(0.4f, 0.5f, 0.8f);
            TextureMidBlue = UITextures.CreateTexture(2, 2, 0, MidBlue);

            DarkBlue = new Color(0.3f, 0.4f, 0.6f);
            TextureDarkBlue = UITextures.CreateTexture(2, 2, 0, DarkBlue);
        }

        public static Texture2D TextureLightRed { get; private set; }
        public static Color LightRed { get; private set; }

        public static Texture2D TextureMidRed { get; private set; }
        public static Color MidRed { get; private set; }

        public static Texture2D TextureDarkRed { get; private set; }
        public static Color DarkRed { get; private set; }

        public static Texture2D TexLightGray { get; private set; }
        public static Color LightGray { get; private set; }

        public static Texture2D TexMidGray { get; private set; }
        public static Color MidGray { get; private set; }

        public static Texture2D TexDarkGray { get; private set; }
        public static Color DarkGray { get; private set; }

        public static Texture2D TexVeryDarkGray { get; private set; }
        public static Color VeryDarkGray { get; private set; }

        public static Texture2D TextureLightBlue { get; private set; }
        public static Color LightBlue { get; private set; }

        public static Texture2D TextureMidBlue { get; private set; }
        public static Color MidBlue { get; private set; }

        public static Texture2D TextureDarkBlue { get; private set; }
        public static Color DarkBlue { get; private set; }

        public static Color CreateGray(float brightness)
        {
            return new Color(brightness, brightness, brightness);
        }
    }
}
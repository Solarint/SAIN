using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using Font = UnityEngine.Font;
using UnityEngine;
using System;
using SAIN.Helpers;
using static EFT.SpeedTree.TreeWind;

namespace SAIN.Editor.Util
{
    internal class Fonts
    {
        public static Font[] AvailableFonts;
        public static string[] FontNames;
        public static int FontIndex = 0;
        public static Font SelectedFont => AvailableFonts[FontIndex];


        public static void Init()
        {

            List<Font> availableFonts = GetAvailableFonts();
            AvailableFonts = availableFonts.ToArray();
            FontNames = new string[AvailableFonts.Length];
            for (int i = 0; i < AvailableFonts.Length; i++)
            {
                FontNames[i] = AvailableFonts[i].name;
            }
        }

        private static List<Font> GetAvailableFonts()
        {
            List<Font> fonts = new List<Font>();

            InstalledFontCollection installedFonts = new InstalledFontCollection();
            FontFamily[] fontFamilies = installedFonts.Families;

            foreach (FontFamily fontFamily in fontFamilies)
            {
                try
                {
                    Font font = Font.CreateDynamicFontFromOSFont(fontFamily.Name, 12);
                    fonts.Add(font);
                }
                catch (Exception e)
                {
                    // Handle any exceptions that may occur
                    Console.WriteLine("Failed to create font for " + fontFamily.Name + ": " + e.Message);
                }
            }

            return fonts;
        }
    }
}

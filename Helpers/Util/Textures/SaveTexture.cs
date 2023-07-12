using BepInEx.Logging;
using SAIN.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static SAIN.Logger;

namespace SAIN.Helpers.Textures
{
    internal static class SaveTexture
    {
        public static void Textures(string path, Dictionary<string, Texture2D> dict)
        {
            foreach (var kvp in dict)
            {
                Single(path, kvp.Key, kvp.Value);
            }
        }

        public static void Textures(string path, Dictionary<int, Texture2D> dict)
        {
            foreach (var kvp in dict)
            {
                Single(path, kvp.Key.ToString(), kvp.Value);
            }
        }

        public static void Textures(string path, string name, Texture2D[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                SaveTexturei(i, path, name, array[i]);
            }
        }

        public static void Textures(string path, string name, List<Texture2D> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                SaveTexturei(i, path, name, list[i]);
            }
        }

        private static void SaveTexturei(int i, string path, string name, Texture2D texture)
        {
            name += i.ToString();
            Single(path, name, texture);
        }

        public static void Single(string path, string name, Texture2D texture)
        {
            byte[] pngData = texture.EncodeToPNG();
            string filename = Path.Combine(path, name + ".png");
            File.WriteAllBytes(filename, pngData);
        }
    }
}

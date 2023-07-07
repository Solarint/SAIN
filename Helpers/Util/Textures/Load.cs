using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static SAIN.Logger;

namespace SAIN.Helpers.Textures
{
    internal static class Load
    {
        private static void LogMessage(string message)
        {
            LogWarning(message, typeof(Load), true);
        }

        public static Texture2D Single(string path)
        {
            Texture2D texture = null;
            if (File.Exists(path))
            {
                byte[] textureBytes = File.ReadAllBytes(path);
                texture = new Texture2D(2, 2);
                texture.LoadImage(textureBytes);
            }
            else
            {
                LogMessage($"[{path}] does not exist");
            }
            return texture;
        }

        public static bool Textures(string path, Dictionary<string, Texture2D> dict)
        {
            if (Directory.Exists(path))
            {
                string[] fileNames = Directory.GetFiles(path, "*.png");
                if (fileNames.Length == 0)
                {
                    LogMessage($"No Files Found");
                    return false;
                }

                foreach (string filePath in fileNames)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    Texture2D texture = Single(filePath);
                    dict.Add(fileName, texture);
                }
                return dict.Count > 0;
            }
            else
            {
                LogMessage($"[{path}] does not exist");
                Directory.CreateDirectory(path);
                return false;
            }
        }

        public static bool Textures(string path, Dictionary<int, Texture2D> dict)
        {
            if (Directory.Exists(path))
            {
                string[] fileNames = Directory.GetFiles(path, "*.png");
                if (fileNames.Length == 0)
                {
                    LogMessage($"No Files Found");
                    return false;
                }

                for (int i = 0; i < fileNames.Length; i++)
                {
                    string fileName = Path.GetFileNameWithoutExtension(fileNames[i]);
                    Texture2D texture = Single(fileName);
                    dict.Add(i, texture);
                }
                return dict.Count > 0;
            }
            else
            {
                LogMessage($"[{path}] does not exist");
                Directory.CreateDirectory(path);
                return false;
            }
        }

        public static Texture2D[] Textures(string path)
        {
            if (Directory.Exists(path))
            {
                string[] fileNames = Directory.GetFiles(path, "*.png");
                if (fileNames.Length == 0)
                {
                    LogMessage($"No Files Found");
                    return null;
                }

                Texture2D[] array = new Texture2D[fileNames.Length];

                for (int i = 0; i < fileNames.Length; i++)
                {
                    string fileName = Path.GetFileNameWithoutExtension(fileNames[i]);
                    Texture2D texture = Single(fileNames[i]);
                    texture.name = fileName;
                    array[i] = texture;
                }
                return array;
            }
            else
            {
                LogMessage($"[{path}] does not exist");
                Directory.CreateDirectory(path);
                return null;
            }
        }

        public static List<Texture2D> TexturesList(string path)
        {
            if (Directory.Exists(path))
            {
                string[] fileNames = Directory.GetFiles(path, "*.png");
                if (fileNames.Length == 0)
                {
                    LogMessage($"No Files Found");
                    return null;
                }

                List<Texture2D> List = new List<Texture2D>();
                for (int i = 0; i < fileNames.Length; i++)
                {
                    string fileName = Path.GetFileNameWithoutExtension(fileNames[i]);
                    Texture2D texture = Single(fileName);
                    List.Add(texture);
                }
                return List;
            }
            else
            {
                LogMessage($"[{path}] does not exist");
                Directory.CreateDirectory(path);
                return null;
            }
        }
    }
}

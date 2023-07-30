using SAIN.BotSettings;
using System.Collections.Generic;

namespace SAIN.Helpers
{
    public class SAINDictionary<T>
    {
        public SAINDictionary(string fileName, params string[] folders)
        {
            Name = fileName;
            FoldersDirectory = folders;
            if (Import(out var dict))
            {
                Dictionary = dict;
            }
            else
            {
                NewFileCreated = true;
                Dictionary = new Dictionary<string, T>();
                Export();
            }
        }

        public readonly bool NewFileCreated = false;

        public SAINDictionary()
        {
            Dictionary = new Dictionary<string, T>();
        }

        readonly string Name;
        readonly string[] FoldersDirectory;

        const string ErrorAlreadyExist = "Key already exists in Settings Dictionary";
        const string ErrorDoesNotExist = "Key does not exist in Settings Dictionary";

        public void Add(string key, T value)
        {
            if (!Dictionary.ContainsKey(key))
            {
                Dictionary.Add(key, value);
            }
            else
            {
                LogError(ErrorAlreadyExist);
            }
        }

        public void Add(object key, T value)
        {
            string keyStr = key.ToString();
            if (!Dictionary.ContainsKey(keyStr))
            {
                Dictionary.Add(keyStr, value);
            }
            else
            {
                LogError(ErrorAlreadyExist);
            }
        }

        public void Modify(string key, T value)
        {
            if (Dictionary.ContainsKey(key))
            {
                Dictionary[key] = value;
            }
            else
            {
                LogError(ErrorDoesNotExist);
            }
        }

        public bool Get(string key, out T value)
        {
            if (Dictionary.ContainsKey(key))
            {
                value = Dictionary[key];
                return true;
            }
            else
            {
                LogError(ErrorDoesNotExist);
                value = default;
                return false;
            }
        }

        public T Get(string key)
        {
            if (Dictionary.ContainsKey(key))
            {
                return Dictionary[key];
            }
            else
            {
                LogError(ErrorDoesNotExist);
                return default;
            }
        }

        public T Get(object key)
        {
            string keyStr = key.ToString();
            if (Dictionary.ContainsKey(keyStr))
            {
                return Dictionary[keyStr];
            }
            else
            {
                LogError(ErrorDoesNotExist);
                return default;
            }
        }

        public void Remove(string key)
        {
            if (Dictionary.ContainsKey(key))
            {
                Dictionary.Remove(key);
            }
            else
            {
                LogError(ErrorDoesNotExist);
            }
        }

        static void LogError(string message)
        {
            Logger.LogError(message, typeof(BotSettingsDictionary), true);
        }

        public void Export()
        {
            if (Dictionary != null && FoldersDirectory != null)
            {
                JsonUtility.Save.SaveJson(Dictionary, Name, FoldersDirectory);
            }
        }

        public bool Import(out Dictionary<string, T> dictionary)
        {
            if (FoldersDirectory == null)
            {
                dictionary = null;
                return false;
            }
            return JsonUtility.Load.LoadObject(out dictionary, Name, FoldersDirectory);
        }

        public readonly Dictionary<string, T> Dictionary;
    }
}

using System;
using UnityEngine;

namespace SAIN.Editor.Util
{
    internal class InputField
    {
        private readonly string Blank = "";
        private string userInput = "";

        public float? Float(string name)
        {
            Handler(name);
            // Perform validation and convert to float
            if (float.TryParse(userInput, out float floatValue))
            {
                return floatValue;
            }
            else
            {
                userInput = Blank;
                return null;
            }
        }

        public int? Int(string name)
        {
            Handler(name);

            // Perform validation and convert to float
            if (int.TryParse(userInput, out int value))
            {
                return value;
            }
            else
            {
                userInput = Blank;
                return null;
            }
        }

        public string Text(string name)
        {
            Handler(name);
            return userInput;
        }

        private string Handler(string name)
        {
            Event e = Event.current;

            string controlName = "TextField" + name;
            if (e.type == EventType.KeyDown && e.keyCode != KeyCode.None)
            {
                if (GUI.GetNameOfFocusedControl() == controlName)
                {
                    e.Use();
                }
            }

            GUI.SetNextControlName(controlName);

            GUILayout.BeginHorizontal();
            GUILayout.Label(name);
            userInput = GUILayout.TextField(userInput);
            GUILayout.EndHorizontal();

            if (GUI.GetNameOfFocusedControl() != controlName)
            {
                GUI.FocusControl(controlName);
            }
            return userInput;
        }
    }
}

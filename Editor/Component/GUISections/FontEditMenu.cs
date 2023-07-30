using SAIN.Editor.Util;
using UnityEngine;

namespace SAIN.Editor.GUISections
{
    internal class FontEditMenu
    {
        public static void OpenMenu()
        {
            GUILayout.BeginVertical();
            if (FontButtonStyle == null)
            {
                FontButtonStyle = new GUIStyle(GUI.skin.button);
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset", GUILayout.Height(35)))
            {
                //GUIModify.skin.font = StyleOptions.DefaultFont;
            }
            if (GUILayout.Button("Save Font", GUILayout.Height(35)))
            {
                //EditorGUI.Dictionary.FontName = GUIModify.skin.font.name;
                //EditorGUI.Dictionary.FontSize = GUIModify.skin.font.fontSize;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            int count = 0;
            for (int i = 0; i < Fonts.AvailableFonts.Length; i++)
            {
                if (count == 4)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    count = 0;
                }
                Font font = Fonts.AvailableFonts[i];
                FontButtonStyle.font = font;
                if (GUI.skin.font == font)
                {
                    //FontButtonStyle.Normal.background = ColorsClass.TextureDarkRed;
                }
                else
                {
                    //FontButtonStyle.Normal.background = ColorsClass.TexMidGray;
                }
                if (GUILayout.Button(font.name, FontButtonStyle, GUILayout.Height(25), GUILayout.Width(125)))
                {
                    GUI.skin.font = font;
                }
                count++;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private static GUIStyle FontButtonStyle;
    }
}

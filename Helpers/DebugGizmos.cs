using EFT;
using SAIN.Editor.Util;
using SAIN.Plugin;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;

namespace SAIN.Helpers;

public sealed class DebugLabel
{
    public Vector3 WorldPos;
    public string Text;
    public GUIStyle Style;
    public float Scale = 1;
    public StringBuilder StringBuilder = new();
    public bool Enabled = true;
}

public struct DebugGizmo
{
    public GameObject GameObject;
    public DebugLabel Label;
    public float ExpireTime;
}

public class DebugGizmos
{
    static DebugGizmos()
    {
        GameWorld.OnDispose += ClearGizmos;
        PresetHandler.OnPresetUpdated += PresetUpdated;
    }

    public static Color RandomColor => new(RandomFloat, RandomFloat, RandomFloat);
    public static bool DrawGizmos => SAINPlugin.DrawDebugGizmos;

    public static void ManualUpdate()
    {
        float currentTime = Time.time;
        if (_gizmos.Count == 0) return;
        for (int i = _gizmos.Count - 1; i >= 0; i--)
        {
            DebugGizmo gizmo = _gizmos[i];
            float expireTime = gizmo.ExpireTime;
            if (expireTime > 0 && expireTime < currentTime)
            {
                if (gizmo.GameObject != null)
                {
                    Object.Destroy(gizmo.GameObject);
                    gizmo.GameObject = null;
                }
                _gizmos.RemoveAt(i);
            }
        }
    }

    public static void OnGUI()
    {
        if (DebugSettings.Instance.Logs.DrawDebugLabels)
        {
            foreach (DebugGizmo gizmo in _gizmos)
            {
                DebugLabel label = gizmo.Label;
                if (label == null || !label.Enabled) continue;
                string text = label.Text.IsNullOrEmpty() ? label.StringBuilder.ToString() : label.Text;
                OnGUIDrawLabel(label.WorldPos, text, label.Style, label.Scale);
            }
        }
    }

    public static DebugLabel CreateLabel(Vector3 worldPos, string text, GUIStyle guiStyle = null, float scale = 1f)
    {
        DebugLabel obj = new() { WorldPos = worldPos, Text = text, Style = guiStyle, Scale = scale };
        AddGUIObject(obj);
        return obj;
    }

    public static void DestroyLabel(DebugLabel obj)
    {
        for (int i = _gizmos.Count - 1; i >= 0; i--)
        {
            if (_gizmos[i].Label == obj)
            {
                if (_gizmos[i].GameObject != null)
                    Object.Destroy(_gizmos[i].GameObject);
                _gizmos.RemoveAt(i);
                break;
            }
        }
    }

    public static void OnGUIDrawLabel(Vector3 worldPos, string text, GUIStyle guiStyle = null, float scale = 1f)
    {
        if (Camera.main == null)
        {
            return;
        }
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        if (screenPos.z <= 0)
        {
            return;
        }

        if (guiStyle == null)
        {
            if (_defaultStyle == null)
            {
                _defaultStyle = new GUIStyle(GUI.skin.box) {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 20,
                    margin = new RectOffset(3, 3, 3, 3)
                };
                ApplyToStyle.BackgroundAllStates(null, _defaultStyle);
            }
            guiStyle = _defaultStyle;
        }

        int origFontSize = guiStyle.fontSize;
        if (scale < 1)
        {
            int newFontSize = Mathf.RoundToInt(origFontSize * scale);
            guiStyle.fontSize = newFontSize;
        }

        GUIContent content = new(text);

        float screenScale = GetScreenScale();
        Vector2 guiSize = guiStyle.CalcSize(content);
        float x = (screenPos.x * screenScale) - (guiSize.x / 2);
        float y = Screen.height - ((screenPos.y * screenScale) + guiSize.y);
        Rect rect = new(new Vector2(x, y), guiSize);
        GUI.Label(rect, content, guiStyle);
        guiStyle.fontSize = origFontSize;
    }

    private static float GetScreenScale()
    {
        if (_nextCheckScreenTime < Time.time && CameraClass.Instance.SSAA.isActiveAndEnabled)
        {
            _nextCheckScreenTime = Time.time + 10f;
            _screenScale = (float)CameraClass.Instance.SSAA.GetOutputWidth() / (float)CameraClass.Instance.SSAA.GetInputWidth();
        }
        return _screenScale;
    }

    public static GameObject DrawSphere(Vector3 position, float size, Color color, float expiretime = -1f, string label = null)
    {
        if (DrawGizmos)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.GetComponent<Collider>().enabled = false;
            UpdateSphere(sphere, position, size, color);
            AddGizmo(sphere, expiretime, label);
            return sphere;
        }
        return null;
    }

    public static void UpdateSphere(GameObject Sphere, Vector3 position, float size, Color color)
    {
        if (Sphere != null)
        {
            Sphere.GetComponent<Renderer>().material.color = color;
            Sphere.transform.position = new Vector3(position.x, position.y, position.z);
            Sphere.transform.localScale = new Vector3(size, size, size);
        }
    }

    public static GameObject DrawBox(Vector3 position, float length, float height, Color color, float expiretime = -1f)
    {
        if (!DrawGizmos)
        {
            return null;
        }
        if (!SAINPlugin.DebugMode)
        {
            return null;
        }

        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);

        box.GetComponent<Renderer>().material.color = color;
        box.GetComponent<Collider>().enabled = false;
        box.transform.position = position;
        box.transform.localScale = new Vector3(length * 2, height * 2, length * 2);
        AddGizmo(box, expiretime);

        return box;
    }

    public static GameObject DrawBox(Vector3 position, Vector3 size, Color color, float expiretime = -1f)
    {
        if (!DrawGizmos)
        {
            return null;
        }
        if (!SAINPlugin.DebugMode)
        {
            return null;
        }

        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);

        box.GetComponent<Renderer>().material.color = color;
        box.GetComponent<Collider>().enabled = false;
        box.transform.position = position;
        box.transform.localScale = size;
        AddGizmo(box, expiretime);

        return box;
    }

    public static GameObject Ray(Vector3 startPoint, Vector3 direction, Color color, float length = 1f, float lineWidth = 0.1f, float expiretime = 1f, bool taperLine = false, string label = null)
    {
        Vector3 endPoint = startPoint + direction.normalized * length;
        return DrawLine(startPoint, endPoint, color, lineWidth, expiretime, taperLine, label);
    }

    public static GameObject DrawLine(Vector3 startPoint, Vector3 endPoint, Color color, float lineWidth, float expiretime = -1, bool taperLine = false, string label = null)
    {
#if DEBUG
        if (DrawGizmos && SAINPlugin.DebugMode)
        {
            GameObject gameObject = new();
            gameObject.transform.position = startPoint;
            SetLineWidth(gameObject, lineWidth, taperLine);
            SetLinePositions(gameObject, startPoint, endPoint);
            SetGizmoColor(gameObject, color);
            AddGizmo(gameObject, expiretime, label);
            return gameObject;
        }
#endif
        return null;
    }

    public static void SetLinePositions(GameObject gameObject, params Vector3[] positions)
    {
        if (gameObject != null && positions != null)
        {
            int count = positions.Length;
            if (count > 0)
            {
                LineRenderer lineRenderer = gameObject.GetOrAddComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    lineRenderer.positionCount = positions.Length;
                    for (int i = 0; i < count; i++)
                    {
                        lineRenderer.SetPosition(i, positions[i]);
                    }
                }
            }
        }
    }

    public static void SetGizmoColor(GameObject gameObject, Color color)
    {
        if (gameObject != null &&
            gameObject.TryGetComponent<LineRenderer>(out var linerenderer))
        {
            linerenderer.material.color = color;
            return;
        }
        if (gameObject != null &&
            gameObject.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = color;
        }
    }

    public static void SetLineWidth(GameObject gameObject, float lineWidth, bool taperLine = false)
    {
        const float MIN_LINE_WIDTH = 0.0025f;
        const float MAX_LINE_WIDTH = 1;
        const float LINE_TAPER_START_COEF = 1.25f;
        const float LINE_TAPER_END_COEF = 0.15f;
        LineRenderer lineRenderer = gameObject?.GetOrAddComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = Mathf.Clamp(taperLine ? lineWidth * LINE_TAPER_START_COEF : lineWidth, MIN_LINE_WIDTH, MAX_LINE_WIDTH);
            lineRenderer.endWidth = Mathf.Clamp(taperLine ? lineWidth * LINE_TAPER_END_COEF : lineWidth, MIN_LINE_WIDTH, MAX_LINE_WIDTH);
        }
    }

    public static void UpdateLinePosition(Vector3 a, Vector3 b, GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }
        var lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer?.SetPosition(0, a);
        lineRenderer?.SetPosition(1, b);
    }

    private static void PresetUpdated(SAINPresetClass preset)
    {
        if (!preset.GlobalSettings.General.Debug.Gizmos.DrawDebugGizmos)
        {
            ClearGizmos();
        }
    }

    private static void AddGizmo(GameObject obj, float expireTime, string label = null)
    {
        DebugGizmo gizmo = new() { GameObject = obj };
        if (label != null)
        {
            gizmo.Label = new() { WorldPos = obj.transform.position, Text = label };
        }
        if (expireTime > 0) gizmo.ExpireTime = Time.time + expireTime;
        _gizmos.Add(gizmo);
    }

    private static void AddGUIObject(DebugLabel obj)
    {
        _gizmos.Add(new() { Label = obj });
    }

    private static void ClearGizmos()
    {
        for (int i = 0; i < _gizmos.Count; i++)
        {
            if (_gizmos[i].GameObject != null)
            {
                Object.Destroy(_gizmos[i].GameObject);
            }
        }
        _gizmos.Clear();
    }

    private static float RandomFloat => Random.Range(0.2f, 1f);
    private static float _screenScale = 1.0f;
    private static float _nextCheckScreenTime;
    private static GUIStyle _defaultStyle;
    private static readonly List<DebugGizmo> _gizmos = [];
}
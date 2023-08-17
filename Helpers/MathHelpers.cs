using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SAIN.Helpers
{
    public static class MathHelpers
    {
        public static float ClampObject(object value, float min, float max)
        {
            if (value != null)
            {
                Type type = value.GetType();
                if (type == typeof(float))
                {
                    return Mathf.Clamp((float)value, min, max);
                }
                else if (type == typeof(int))
                {
                    return Mathf.Clamp(
                        Mathf.RoundToInt((float)value),
                        Mathf.RoundToInt(min),
                        Mathf.RoundToInt(max));
                }
                else
                {
                    Logger.LogError($"{type}");
                }
            }
            else
            {
                Logger.LogError($"Null!?");
            }
            return default;
        }

        public static float FloatClamp(this object value, float min, float max)
        {
            if (value != null)
            {
                Type type = value.GetType();
                if (type == typeof(float))
                {
                    return Mathf.Clamp((float)value, min, max);
                }
                else if (type == typeof(int))
                {
                    return Mathf.Clamp(
                        Mathf.RoundToInt((float)value),
                        Mathf.RoundToInt(min),
                        Mathf.RoundToInt(max));
                }
                else
                {
                    Logger.LogError($"{type}");
                }
            }
            else
            {
                Logger.LogError($"Null!?");
            }
            return default;
        }

        public static float InverseScaleWithLogisticFunction(float originalValue, float k, float x0 = 20f)
        {
            float scaledValue = 1f - 1f / (1f + Mathf.Exp(k * (originalValue - x0)));
            return scaledValue.Round1000();
        }


        public static Vector3 VectorClamp
            (Vector3 vector, float min, float max)
        {
            vector.x = Mathf.Clamp(vector.x, -min, max);
            vector.y = Mathf.Clamp(vector.y, -min, max);
            vector.z = Mathf.Clamp(vector.z, -min, max);
            return vector;
        }
    }
}

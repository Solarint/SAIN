using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SAIN.Helpers
{
    public class MathHelpers
    {
        public static float InverseScaleWithLogisticFunction(float originalValue, float k, float x0 = 20f)
        {
            float scaledValue = 1f - 1f / (1f + Mathf.Exp(k * (originalValue - x0)));
            return (float)System.Math.Round(scaledValue, 3);
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

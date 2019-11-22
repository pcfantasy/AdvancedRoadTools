using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AdvancedRoadTools.NewData
{
    public static class CustomNetSegment
    {
        public static void CalculateMiddlePoints(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, bool smoothStart, bool smoothEnd, out Vector3 middlePos1, out Vector3 middlePos2)
        {
            if (IsStraight(startPos, startDir, endPos, endDir, out float distance))
            {
                middlePos1 = startPos + startDir * (distance * ((!smoothStart) ? 0.15f : 0.276f));
                middlePos2 = endPos + endDir * (distance * ((!smoothEnd) ? 0.15f : 0.276f));
            }
            else
            {
                float num = startDir.x * endDir.x + startDir.z * endDir.z;
                if (num >= -0.999f && Line2.Intersect(VectorUtils.XZ(startPos), VectorUtils.XZ(startPos + startDir), VectorUtils.XZ(endPos), VectorUtils.XZ(endPos + endDir), out float u, out float v))
                {
                    u = Mathf.Clamp(u, distance * 0.1f, distance);
                    v = Mathf.Clamp(v, distance * 0.1f, distance);
                    float num2 = u + v;
                    middlePos1 = startPos + startDir * Mathf.Min(u, num2 * 0.276f);
                    middlePos2 = endPos + endDir * Mathf.Min(v, num2 * 0.276f);
                }
                else
                {
                    middlePos1 = startPos + startDir * (distance * 0.276f);
                    middlePos2 = endPos + endDir * (distance * 0.276f);
                }
            }
        }

        public static bool IsStraight(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, out float distance)
        {
            Vector3 vector = VectorUtils.NormalizeXZ(endPos - startPos, out distance);
            float num = startDir.x * endDir.x + startDir.z * endDir.z;
            float num2 = startDir.x * vector.x + startDir.z * vector.z;
            return num < -0.999f && num2 > 0.999f;
        }
    }
}

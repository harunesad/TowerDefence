using UnityEngine;

namespace TowerDefence.Core
{
    public static class WalkSurface
    {
        const float FloorNormalY = 0.55f;
        const float SpikeHeight = 2f;
        const int OuterSamples = 12;
        const float OuterRadius = 10f;
        const int InnerSamples = 8;
        const float InnerRadius = 4f;

        public static bool TryGetHeight(float x, float z, int mask, out float height)
        {
            height = 0f;
            bool hasHere = LowestFloor(x, z, mask, out float here);

            float[] around = new float[OuterSamples + InnerSamples];
            int count = 0;
            count = CollectRing(x, z, mask, OuterRadius, OuterSamples, around, count);
            count = CollectRing(x, z, mask, InnerRadius, InnerSamples, around, count);
            if (count == 0)
            {
                if (!hasHere) return false;
                height = here;
                return true;
            }

            System.Array.Sort(around, 0, count);
            int index = Mathf.Clamp(Mathf.FloorToInt((count - 1) * 0.25f), 0, count - 1);
            float ground = around[index];
            height = hasHere && here <= ground + SpikeHeight ? here : ground;
            return true;
        }

        static int CollectRing(float x, float z, int mask, float radius, int samples, float[] into, int count)
        {
            for (int i = 0; i < samples; i++)
            {
                float angle = i * Mathf.PI * 2f / samples;
                float sx = x + Mathf.Cos(angle) * radius;
                float sz = z + Mathf.Sin(angle) * radius;
                if (LowestFloor(sx, sz, mask, out float sample))
                    into[count++] = sample;
            }
            return count;
        }

        static bool LowestFloor(float x, float z, int mask, out float height)
        {
            height = 0f;
            RaycastHit[] hits = Physics.RaycastAll(
                new Vector3(x, 80f, z), Vector3.down, 160f, mask, QueryTriggerInteraction.Ignore);
            bool found = false;
            float lowest = float.PositiveInfinity;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].normal.y <= FloorNormalY) continue;
                if (hits[i].point.y >= lowest) continue;
                lowest = hits[i].point.y;
                found = true;
            }
            if (!found) return false;
            height = lowest;
            return true;
        }
    }
}

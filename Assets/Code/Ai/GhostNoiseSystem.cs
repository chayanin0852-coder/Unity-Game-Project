
using System;
using UnityEngine;

public static class GhostNoiseSystem
{
    public static event Action<Vector3, float> NoiseEmitted;

    public static void EmitNoise(
        Vector3 worldPosition,
        float audibleRadius)
    {
        if (audibleRadius <= 0f)
            return;

        NoiseEmitted?.Invoke(
            worldPosition,
            audibleRadius
        );
    }
}

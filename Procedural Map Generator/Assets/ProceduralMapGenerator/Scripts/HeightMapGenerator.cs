using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator
{

    static float[,] falloffMap;

    public static HeightMap GenerateHeightMap(int size, HeightMapSettings settings, Vector2 sampleCentre)
    {
        AnimationCurve threadSafeCurve = new AnimationCurve(settings.heightCurve.keys);

        float[,] values = Noise.GenerateNoiseMap(size, settings.noiseSettings, sampleCentre);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                values[i, j] *= threadSafeCurve.Evaluate(values[i,j]) * settings.heightMultiplier;

                if(values[i,j] > maxValue)
                {
                    maxValue = values[i, j];
                }
                if(values[i,j] < minValue)
                {
                    minValue = values[i, j];
                }

            }
        }

        // WORK IN PROGRESS
        /*
        if (settings.useFalloff)
        {
            if (falloffMap == null)
            {
                falloffMap = FalloffGenerator.GenerateFallOffMap(size, settings.falloffCurve);
            }

            for (int y = 0; y < size + 2; y++)
            {
                for (int x = 0; x < size + 2; x++)
                {
                    if (settings.useFalloff)
                    {
                        values[x, y] = Mathf.Clamp01(values[x, y] - falloffMap[x, y]);
                    }
                }
            }
        }
        */

        return new HeightMap(values, minValue, maxValue);
    }


}


public struct HeightMap
{
    public readonly float[,] values;
    public readonly float minValue;
    public readonly float maxValue;

    public HeightMap(float[,] values, float minValue, float maxValue)
    {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }

}


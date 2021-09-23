using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TextureSettings : UpdatableData
{

    public Region[] regions;


    float savedMinHeight;
    float savedMaxHeight;


    public void ApplyToMaterial(Material material)
    {
        Color[] colorArray = new Color[regions.Length];
        float[] heightArray = new float[regions.Length];
        float[] blendArray = new float[regions.Length];

        for (int i = 0; i < regions.Length; i++) 
        {
            colorArray[i] = regions[i].baseColour;
            heightArray[i] = regions[i].baseStartHeight;
            blendArray[i] = regions[i].baseBlend;
        }

        material.SetInt("baseColourCount", regions.Length);
        material.SetColorArray("baseColours", colorArray);
        material.SetFloatArray("baseStartHeights", heightArray);
        material.SetFloatArray("baseBlends", blendArray);


        UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
    }

    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
    {
        savedMaxHeight = maxHeight;
        savedMinHeight = minHeight;

        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
    }

    [System.Serializable]
    public struct Region
    {
        public Color baseColour;
        [Range(0f, 1f)]
        public float baseStartHeight;
        [Range(0f, 1f)]
        public float baseBlend;
    }

}

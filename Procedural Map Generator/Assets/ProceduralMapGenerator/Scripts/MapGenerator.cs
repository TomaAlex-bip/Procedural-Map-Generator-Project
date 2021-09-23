using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        FalloffMap,
        Mesh
    };


    [SerializeField] private Material terrainMaterial;

    [SerializeField] private TextureSettings textureSettings;
    [SerializeField] private MeshSettings meshSettings;
    [SerializeField] private HeightMapSettings heightMapSettings;

    [Range(0, 4)]
    [SerializeField] private int editorLOD;
    [SerializeField] private bool autoUpdate;
    [SerializeField] private DrawMode drawMode;


    public bool AutoUpdate { get => autoUpdate; private set => autoUpdate = value; }
    public MeshSettings MeshSettings { get => meshSettings; private set => meshSettings = value; }
    public HeightMapSettings HeightMapSettings { get => heightMapSettings; private set => heightMapSettings = value; }



    


    private void Start()
    {
        textureSettings.ApplyToMaterial(terrainMaterial);
        textureSettings.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
    }



    void OnValuesUpdated()
    {
        if(!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureSettings.ApplyToMaterial(terrainMaterial);
    }


    public void DrawMapInEditor()
    {
        textureSettings.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVerticesPerLine, heightMapSettings, Vector2.zero);

        MapDisplay display = FindObjectOfType<MapDisplay>();

        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap.values));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorLOD));
        }
        else if(drawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFallOffMap(meshSettings.numVerticesPerLine, heightMapSettings.falloffCurve)));
        }

        textureSettings.ApplyToMaterial(terrainMaterial);
    }

    


    private void OnValidate()
    {
        if(meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if(heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if(textureSettings != null)
        {
            textureSettings.OnValuesUpdated -= OnValuesUpdated;
            textureSettings.OnValuesUpdated += OnValuesUpdated;
        }

    }

    


}




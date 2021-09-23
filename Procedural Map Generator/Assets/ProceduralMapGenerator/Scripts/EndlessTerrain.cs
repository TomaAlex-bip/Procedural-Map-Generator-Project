using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    

    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float collisionDistanceUpdateThreshold = 10f;
    const float sqrCollisionDistanceUpdateThreshold = collisionDistanceUpdateThreshold * collisionDistanceUpdateThreshold;
    const float sqrviewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;


    public static float maxViewDistance;
    [SerializeField] private LODInfo[] detailLevels;

    [SerializeField] private int colliderLOD;

    [SerializeField] private Transform viewer;
    [SerializeField] private Material mapMaterial;
    [SerializeField] public LayerMask layerMask;

    [HideInInspector]
    public static Vector2 viewerPosition;
    private static Vector2 viewerPositionOld;

    float meshWorldSize;
    int chunksVisibleInViewDistance;

    static MapGenerator mapGenerator;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();

    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;

        meshWorldSize = mapGenerator.MeshSettings.meshWorldSize;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / meshWorldSize);


        UpdateVizibleChunks();
    }


    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);


        if(viewerPosition != viewerPositionOld)
        {
            foreach(TerrainChunk chunk in terrainChunksVisibleLastUpdate)
            {
                chunk.UpdateCollisionMesh();
            }
        }


        if((viewerPositionOld - viewerPosition).sqrMagnitude > sqrviewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVizibleChunks();
        }

    }


    private void UpdateVizibleChunks()
    {

        foreach(TerrainChunk chunk in terrainChunksVisibleLastUpdate)
        {
            chunk.SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);


        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if(terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, mapGenerator.HeightMapSettings, mapGenerator.MeshSettings, detailLevels, colliderLOD, transform, mapMaterial, layerMask));
                }

            }

        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;

        Vector2 sampleCentre;

        Bounds bounds;

        HeightMap heightMap;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailLevels;

        LODMesh[] lodMeshes;

        int colliderLOD;

        bool heightMapReceived;

        int previousLODIndex = -1;

        bool hasSetCollider;

        HeightMapSettings heightMapSettings;
        MeshSettings meshSettings;


        public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLOD, Transform parent, Material material, LayerMask layer)
        {
            this.heightMapSettings = heightMapSettings;
            this.meshSettings = meshSettings;

            this.detailLevels = detailLevels;
            this.colliderLOD = colliderLOD;

            sampleCentre = coord * meshSettings.meshWorldSize / mapGenerator.MeshSettings.meshScale;
            Vector2 position = coord * meshSettings.meshWorldSize;
            bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

            meshObject = new GameObject("Terrain Chunk");

            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();

            meshObject.transform.position = new Vector3(position.x, 0f, position.y);

            meshObject.transform.parent = parent;
            meshRenderer.material = material;
            meshObject.layer = 6; // aparent nu vrea cu layer.value

            lodMeshes = new LODMesh[detailLevels.Length];

            SetVisible(false);


            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod);
                lodMeshes[i].updateCallback += UpdateTerrainChunk;

                if(i == colliderLOD)
                {
                    lodMeshes[i].updateCallback += UpdateCollisionMesh;
                }
            }

            //mapGenerator.RequestHeightMap(sampleCentre, OnHeightMapReceived);

            ThreadedDataRequester.RequestHeightMap(sampleCentre, meshSettings, heightMapSettings, OnHeightMapReceived);
        
        }

        private void OnHeightMapReceived(HeightMap heightMap)
        {
            //print("Map data received");
            //mapGenerator.RequestMeshData(heightMap, OnMeshDataReceived);

            this.heightMap = heightMap;
            heightMapReceived = true;

            UpdateTerrainChunk();
        }

        private void OnMeshDataReceived(MeshData meshData)
        {
            //print("Mesh data created");
            meshFilter.mesh = meshData.CreateMesh();
        }


        public void UpdateTerrainChunk()
        {
            if (heightMapReceived)
            {
                float viwerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

                bool visible = viwerDistanceFromNearestEdge <= maxViewDistance;

                if (visible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viwerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasReqestedMesh)
                        {
                            lodMesh.RequestMesh(heightMap, meshSettings);
                        }
                    }


                    terrainChunksVisibleLastUpdate.Add(this);

                }

                SetVisible(visible);
            }


        }

        public void UpdateCollisionMesh()
        {
            if(!hasSetCollider)
            {
                float sqrDistanceFromViewerToEdge = bounds.SqrDistance(viewerPosition);

                if(sqrDistanceFromViewerToEdge < detailLevels[colliderLOD].sqrVisibleDistanceThreshold)
                {
                    if(!lodMeshes[colliderLOD].hasReqestedMesh)
                    {
                        lodMeshes[colliderLOD].RequestMesh(heightMap, meshSettings);
                    }
                }

                if(sqrDistanceFromViewerToEdge < sqrCollisionDistanceUpdateThreshold)
                {
                    if(lodMeshes[colliderLOD].hasMesh)
                    {
                        meshCollider.sharedMesh = lodMeshes[colliderLOD].mesh;
                        hasSetCollider = true;
                    }
                }
            }
        }

        public void SetVisible(bool visible) => meshObject.SetActive(visible);

        public bool IsVisible() => meshObject.activeSelf;


    }


    class LODMesh
    {
        public Mesh mesh;
        public bool hasReqestedMesh;
        public bool hasMesh;
        int lod;

        public event System.Action updateCallback;

        public LODMesh(int lod)
        {
            this.lod = lod;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
        {
            hasReqestedMesh = true;
            //mapGenerator.RequestMeshData(heightMap, lod, OnMeshDataReceived);
            ThreadedDataRequester.RequestMeshData(heightMap, lod, meshSettings, OnMeshDataReceived);
        }

    }

    [System.Serializable]
    public struct LODInfo
    {
        [Range (0, MeshSettings.numSupportedLODs-1)]
        public int lod;
        public float visibleDistanceThreshold;

        public float sqrVisibleDistanceThreshold { get => visibleDistanceThreshold * visibleDistanceThreshold; }

    }



}




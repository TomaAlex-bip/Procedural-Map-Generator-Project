using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;

public class ThreadedDataRequester : MonoBehaviour
{

    static ThreadedDataRequester instance;

    Queue<MapThreadInfo<HeightMap>> heightMapThreadInfoQueue = new Queue<MapThreadInfo<HeightMap>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();


    public void Awake()
    {
        instance = FindObjectOfType<ThreadedDataRequester>();
    }


    public static void RequestHeightMap(Vector2 centre, MeshSettings meshSettings, HeightMapSettings heightMapSettings, Action<HeightMap> callback)
    {
        ThreadStart threadStart = delegate
        {
            instance.HeightMapThread(centre, meshSettings, heightMapSettings, callback);
        };

        new Thread(threadStart).Start();
    }

    private void HeightMapThread(Vector2 centre, MeshSettings meshSettings, HeightMapSettings heightMapSettings, Action<HeightMap> callback)
    {
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVerticesPerLine, heightMapSettings, centre);

        lock (heightMapThreadInfoQueue)
        {
            heightMapThreadInfoQueue.Enqueue(new MapThreadInfo<HeightMap>(callback, heightMap));
        }

    }

    public static void RequestMeshData(HeightMap heightMap, int lod, MeshSettings meshSettings, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            instance.MeshDataThread(heightMap, lod, meshSettings, callback);
        };

        new Thread(threadStart).Start();
    }

    private void MeshDataThread(HeightMap heightMap, int lod, MeshSettings meshSettings, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }


    private void Update()
    {
        if (heightMapThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < heightMapThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<HeightMap> threadInfo = heightMapThreadInfoQueue.Dequeue();

                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();

                threadInfo.callback(threadInfo.parameter);
            }
        }
    }




    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

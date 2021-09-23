using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : UpdatableData
{
    public float meshScale = 1;

    public bool useFlatShading;

    public const int numSupportedLODs = 5;
    public const int numSupportedChunkSizes = 9;
    public static readonly int[] supportedChunkSizes = {48, 72, 96, 120, 144, 168, 192, 216, 240};


    [Range(0, numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;


    // number of vertices per line of mesh rendered at LOD = 0
    //Includes the 2 extra verices that are excluded from the final mesh, but used for calculating normals
    public int numVerticesPerLine { get => supportedChunkSizes[chunkSizeIndex] + 1; }

    public float meshWorldSize { get => (numVerticesPerLine - 3) * meshScale; }


}

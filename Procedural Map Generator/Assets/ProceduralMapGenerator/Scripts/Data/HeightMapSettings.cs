using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData
{

    public NoiseSettings noiseSettings;

    public float heightMultiplier;
    public AnimationCurve heightCurve;

    public bool useFalloff;
    public AnimationCurve falloffCurve;


    public float minHeight { get => heightMultiplier * heightCurve.Evaluate(0); }
    public float maxHeight { get => heightMultiplier * heightCurve.Evaluate(1); }



    protected override void OnValidate()
    {
        noiseSettings.ValidateValues();
        base.OnValidate();
    }


}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ErosionParams : AgentParams
{
    [Space(5)]
    [Range(-1f, 1f)] 
    public float Leniency = -0.1f;
    public float InverseStrength = 13.0f;
    public int AreaOfEffect = 5;

    public override void Randomize()
    {
        Tokens = StaticRandom.Range(30, 100);
        Leniency = StaticRandom.Range(-0.3f, 0.8f);
        InverseStrength = StaticRandom.Range(1.0f, 50.0f);
        AreaOfEffect = StaticRandom.Range(1, 7);
    }
}

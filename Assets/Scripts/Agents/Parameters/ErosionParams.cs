using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ErosionParams : AgentParams
{
    [Space(5)]
    [Range(-1f, 1f)] 
    public float Leniency = -0.08f;
    public float Strength = 0.15f;
    public int AreaOfEffect = 6;
    public AnimationCurve Diminish;

    public override void Randomize()
    {
        Tokens = StaticRandom.Range(30, 100);
        Leniency = StaticRandom.Range(-0.3f, 0.8f);
        Strength = StaticRandom.Range(0.05f, 1.5f);
        AreaOfEffect = StaticRandom.Range(1, 7);
    }
}

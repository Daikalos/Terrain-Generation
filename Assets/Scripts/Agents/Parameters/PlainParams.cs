using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlainParams : AgentParams
{
    [Space(5)]
    [Range(0.0f, 1.0f)]
    public float Smoothing = 0.05f;
    [Range(0.0f, 1.0f)]
    public float SwitchDirection = 0.09f;
    public float MoveMagnitude = 5.0f;
    public int AreaOfEffect = 10;
    public AnimationCurve DiminishingCurve;

    public override void Randomize()
    {
        Tokens = StaticRandom.Range(50, 200);
        Smoothing = StaticRandom.Range(0.01f, 1.0f);
        MoveMagnitude = StaticRandom.Range(1, 3);
        SwitchDirection = StaticRandom.Range(0.01f, 0.3f);
        AreaOfEffect = StaticRandom.Range(5, 15);
    }
}

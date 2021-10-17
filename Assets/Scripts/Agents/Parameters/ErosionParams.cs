using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ErosionParams : AgentParams
{
    [Space(5)]
    [Range(-1f, 1f)] 
    public float Leniency = 0.30f;
    public float InverseStrength = 7.5f;
    public int AreaOfEffect = 8;
}

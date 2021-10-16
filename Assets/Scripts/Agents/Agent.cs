using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Agent : ICloneable
{
    [SerializeField]
    protected int _Lifetime = 0;

    protected Graph _Graph;

    public virtual void Initialize(ref Graph graph)
    {
        _Graph = graph;
    }

    /// <summary>
    /// update the agent
    /// </summary>
    /// <returns>if completed</returns>
    public abstract bool Update();

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

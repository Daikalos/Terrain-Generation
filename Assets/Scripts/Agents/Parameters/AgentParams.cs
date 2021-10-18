using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class AgentParams
{
    public int Tokens = 0;

    public abstract void Randomize();
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : MonoBehaviour, IAgent
{ 

    public abstract bool UpdateAgent(float modelTime);
}

public interface IAgent
{
    bool UpdateAgent(float modelTime);
}

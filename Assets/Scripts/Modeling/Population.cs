using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts;
using UnityEngine;

public abstract class Population<T> : Agent where T : Agent
{
    public List<T> Agents { get; protected set; }

    public virtual void Start()
    {
        Agents = new List<T>();
    }

    public override bool UpdateAgent(float modelTime)
    {
        var toRemove = new List<T>();
        foreach (var agent in Agents)
        {
            if (agent.UpdateAgent(modelTime)) toRemove.Add(agent);
            if (agent is DrawableAgent drawableAgent)
                drawableAgent.Draw();
        }
        foreach (var agent in toRemove)
        {
            Agents.Remove(agent);
        }

        return false;
    }
}

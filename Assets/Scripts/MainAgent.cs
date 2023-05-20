using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Assets.Scripts;
using Assets.Scripts.Jobs;
using Assets.Scripts.Modeling;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Windows;

public class MainAgent : MonoBehaviour
{
    public List<Agent> Agents;

    public UIBase uIBase;

    private bool _isActive = false;
    private Schedule _schedule;
    private PedestrianPopulation _population;

    public TextAsset defaultSchedule;

    public float ModelTime { get; private set; }
    public float ModelTimeDelta { get; private set; }

    private float _modelSpeed;
    public float ModelSpeed
    {
        get => _modelSpeed;
        set
        {
            _modelSpeed = value;
            uIBase.SetSpeed(value);
        }
    }

    public static List<Job> Jobs { get; } = new List<Job>();

    // Start is called before the first frame update
    void Start()
    {
        ModelTime = 0.0f;
        ModelSpeed = 1f;

        StartCoroutine(getSchedule());
    }

    private IEnumerator getSchedule()
    {
        var wr = UnityWebRequest.Get("localhost:8080/");
        yield return wr.SendWebRequest();

        if (wr.result == UnityWebRequest.Result.Success)
        {
            _schedule = JsonUtility.FromJson<Schedule>(wr.downloadHandler.text);
        }
        else
        {
            _schedule = JsonUtility.FromJson<Schedule>(defaultSchedule.text);
            Debug.Log("Error While Sending: " + wr.error);
        }

        _population = Agents.Where(x => x is PedestrianPopulation).FirstOrDefault() as PedestrianPopulation;
        _population.schedule = _schedule;
        _population.Init();
        _isActive = true;
    }

   

    // Update is called once per frame
    void Update()
    {
        if (!_isActive) return;

        ModelTimeDelta = Time.deltaTime * ModelSpeed;
        ModelTime += ModelTimeDelta;

        var toRemoveAgent = new List<Agent>();
        foreach (var agent in Agents)
        {
            if (agent.UpdateAgent(ModelTime)) toRemoveAgent.Add(agent);
            if (agent is IDrawable drawable) drawable.Draw();
        }
        foreach (var agent in toRemoveAgent)
        {
            Agents.Remove(agent);
        }

        var toRemove = new List<Job>();
        for (int i = 0; i < Jobs.Count; i++)
        {
            var job = Jobs[i];
            if (job.Run(ModelTime)) toRemove.Add(job);
        }
        foreach (var job in toRemove)
        {
            Jobs.Remove(job);
        }

        uIBase.SetTime(ModelTime);
    }
}


[Serializable]
public class Schedule
{
    public List<ScheduleRecord> records;
}

[Serializable]
public class ScheduleRecord
{
    public string from;
    public string to;
    public float fromTime;
    public float toTime;
    public float rate;
}
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

    private float _sinceLastUpdate = 0f;
    private int _frames = 0;
    private float _cumulativeFPS = 0f;
    private int _pedestrianSamples = 0;
    private float _avgPedestrians = 0f;

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

    private void UpdateAgents()
    {
        ModelTime += ModelTimeDelta;
        var toRemoveAgent = new List<Agent>();
        foreach (var agent in Agents)
        {
            if (agent.UpdateAgent(ModelTime)) toRemoveAgent.Add(agent);
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
    }

    private void Update()
    {
        Time.fixedDeltaTime = 1f / 60f;
        if (Time.fixedDeltaTime * ModelSpeed > 0.1f)
        {
            Time.fixedDeltaTime = 0.1f / ModelSpeed;
        }

        _sinceLastUpdate += Time.unscaledDeltaTime;
        if (_sinceLastUpdate > 1f)
        {
            Debug.Log($"P: {_avgPedestrians}; FPS: {_cumulativeFPS}");
            _frames = 0;
            _cumulativeFPS = 0;
            _sinceLastUpdate = 0f;
            _pedestrianSamples = 0;
            _avgPedestrians = 0f;
        }
        else
        {
            var curFps = 1f / Time.unscaledDeltaTime;
            _frames++;
            _cumulativeFPS = (_cumulativeFPS * (_frames - 1) + curFps) / _frames;

            _pedestrianSamples++;
            _avgPedestrians = (_avgPedestrians * (_pedestrianSamples - 1) + Pedestrian.PedestrianCount) / _pedestrianSamples;
        }

        foreach (var agent in Agents)
        {
            if (agent is IDrawable drawable) drawable.Draw();
        }

        uIBase.SetTime(ModelTime);
    }

    void FixedUpdate()
    {
        if (!_isActive) return;

        var shouldBe = Time.deltaTime * ModelSpeed;
        var prevTime = ModelTime;
        //for (int i = 1; i < shouldBe / 0.1f; i++)
        //{
        //    ModelTimeDelta = 0.1f;
        //    UpdateAgents();
        //}
        ModelTime = prevTime;
        ModelTimeDelta = shouldBe;// % 0.1f;
        UpdateAgents();

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
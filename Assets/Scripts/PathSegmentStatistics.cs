using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts;
using Assets.Scripts.PathFinding;
using UnityEngine;
using UnityEngine.InputSystem;

public class PathSegmentStatistics : MonoBehaviour
{
    public PathSegment pathSegment;
    private LineRenderer lineRenderer;
    private Vector3[] points;
    private MeshCollider meshCollider;

    public int pedestrianCount;

    private UIBase uiBase;
    private MainAgent mainAgent;
    private PedestrianPopulation pedestrianPopulation;

    private float _busyTime;
    private float _freeTime;

    public float Busyness => _busyTime / (_busyTime + _freeTime);

    private readonly GlobalSettings _settings = GlobalSettings.Default;

    private Dictionary<Pedestrian, float> _trafficLightsWaitingTimes = new Dictionary<Pedestrian, float>();
    private int _waitingTimes;
    private float _oldAvg;
    public float TrafficLightWaitingTime { get; private set; }
    private int _queueLengthSamples;
    public float TrafficLightQueueLength { get; private set; }
    public int CurrentQueueLength { get; private set; }

    private int _lastQueueLength;

    private bool _isSegmentBusy;

    private void SetLineColor()
    {
        if (_isSegmentBusy)
        {
            lineRenderer.material.color = Color.yellow;
        }
        else lineRenderer.material.color = Color.green;
    }

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 10f;
        lineRenderer.endWidth = 10f;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.material = Resources.Load<Material>("Materials/Colored");
        lineRenderer.material.color = Color.green;

        var vertexPath = pathSegment.PathCreator.EditorData.GetVertexPath(transform);
        points = vertexPath.localPoints;
        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points.Select(x => new Vector3(x.x, 0.5f, x.z)).ToArray());

        meshCollider = gameObject.AddComponent<MeshCollider>();
        var mesh = new Mesh();
        lineRenderer.BakeMesh(mesh);
        meshCollider.sharedMesh = mesh;

        uiBase = GameObject.Find("Canvas").GetComponent<UIBase>();
        mainAgent = GameObject.Find("World").GetComponent<MainAgent>();
        pedestrianPopulation = mainAgent.GetComponent<PedestrianPopulation>();

        _settings.OnVisualizeRoadsChanged += (s, e) => HandleVisualizeRoadsChanged(e.NewValuse);
        HandleVisualizeRoadsChanged(_settings.VisualizeRoads);
    }

    private void HandleVisualizeRoadsChanged(bool value)
    {
        lineRenderer.forceRenderingOff = !value;
    }

    public void RemovePedestrianFromTrackingWaitingTime(object sender, EventArgs e)
    {
        var ped = sender as Pedestrian;
        ped.Destroyed -= RemovePedestrianFromTrackingWaitingTime;

        if (!_trafficLightsWaitingTimes.ContainsKey(ped)) return;

        _oldAvg = TrafficLightWaitingTime;
        _waitingTimes++;
        if (_trafficLightsWaitingTimes[ped] != 0)
            CurrentQueueLength--;
        _trafficLightsWaitingTimes.Remove(ped);
    }

    public void AppendTrafficLightWaitingTime(Pedestrian ped, float delta)
    {
        if (_trafficLightsWaitingTimes.ContainsKey(ped))
            _trafficLightsWaitingTimes[ped] += delta;
        else
        {
            ped.Destroyed += RemovePedestrianFromTrackingWaitingTime;
            _trafficLightsWaitingTimes.Add(ped, delta);

            if (delta != 0)
            {
                CurrentQueueLength++;
            }
        }
        ComputeWaitingTime();
    }

    private void ComputeWaitingTime()
    {
        var i = 1;
        TrafficLightWaitingTime = _oldAvg;
        foreach ((var _, var time) in _trafficLightsWaitingTimes)
        {
            TrafficLightWaitingTime = ((_waitingTimes + i - 1) * TrafficLightWaitingTime + time) / (_waitingTimes + i);
            i++;
        }
    }

    private string GetStatistics()
    {
        var builder = new StringBuilder();
        builder.Append($"N: {pedestrianCount}\n");
        builder.Append($"K: {Busyness}");

        if (TrafficLightWaitingTime != 0)
        {
            builder.Append($"\nQTavg: {TrafficLightWaitingTime * 60}\n" +
                $"QLavg: {TrafficLightQueueLength}\n" +
                $"QL: {CurrentQueueLength}");
        }

        return builder.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        _isSegmentBusy = false;
        foreach (var ped in pedestrianPopulation.Agents)
        {
            if (ped.CurrentSegmentDescription?.segment == pathSegment && ped.IsSlowed)
            {
                _isSegmentBusy = true;
                break;
            }    
        }
        SetLineColor();

        if (_isSegmentBusy) _busyTime += mainAgent.ModelTimeDelta;
        else _freeTime += mainAgent.ModelTimeDelta;

        if (CurrentQueueLength > 0 && CurrentQueueLength != _lastQueueLength)
        {
            _queueLengthSamples++;
            _lastQueueLength = CurrentQueueLength;
            TrafficLightQueueLength = ((_queueLengthSamples - 1) * TrafficLightQueueLength + CurrentQueueLength) / _queueLengthSamples;
        }

        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hitInfo) && hitInfo.collider == meshCollider)
        {
            uiBase.AddStatisticsTracking(this, hitInfo.point + new Vector3(0f, 20f, 0f), GetStatistics);
            lineRenderer.material.color = Color.black;

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                uiBase.FlipLockStatisticTracking(this);
            }
        }
        else
        {
            uiBase.RemoveStatisticsTracking(this, false);
            SetLineColor();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Assets.Scripts;
using Assets.Scripts.PathFinding;
using PathCreation;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class Pedestrian : DrawableAgent
{
    public Path Path;
    public float Speed;

    private float _effectiveSpeed;

    private float? _lastTime;
    public PathSegmentDescription CurrentSegmentDescription { get; private set; }
    private float _startDistance;
    private Dictionary<PathSegmentDescription, float> _cumulativeDistanceAtEachSegment;
    private Dictionary<PathSegmentDescription, PathSegmentDescription> _nextSegment;

    private static int s_no = 0;

    private PedestrianPopulation _population;

    private bool _isActive = true;
    private bool _isStopped = false;

    public bool IsSlowed { get; private set; }

    public static Pedestrian Create(GameObject gameObject, Func<GameObject, Path> getPath, float speed, PedestrianPopulation population)
    {
        var pd = new GameObject($"Pedestrian {s_no}", typeof(Pedestrian));
        s_no++;
        pd.transform.SetParent(gameObject.transform);
        var ret = pd.GetComponent<Pedestrian>();
        var path = getPath(pd);
        var first = path.SegmentsDescriptions[0];
        if (path.isReversed ^ first.traverseReversed)
            ret.Position = first.segment.PathCreator.path.GetPointAtDistance(first.segment.PathCreator.path.length, EndOfPathInstruction.Stop);
        else
            ret.Position = first.segment.PathCreator.path.GetPointAtDistance(0f);

        ret.Path = path;
        ret.Speed = speed;
        ret._population = population;
        return ret;
    }

    public Vector3 Position;

    private float _distanceTravelled;

    public override Matrix4x4 Matrix => Matrix4x4.Translate(Position) * Matrix4x4.Scale(new Vector3(100f, 100f, 100f));

    private void Start()
    {
        _cumulativeDistanceAtEachSegment = new Dictionary<PathSegmentDescription, float>();
        _nextSegment = new Dictionary<PathSegmentDescription, PathSegmentDescription>();
        CurrentSegmentDescription = Path.SegmentsDescriptions[0];
        CurrentSegmentDescription.segment.Statistics.pedestrianCount++;
        _effectiveSpeed = Speed;

        var cumulativeDistance = 0f;
        for (int i = 0; i < Path.SegmentsDescriptions.Count; i++)
        {
            var segment = Path.SegmentsDescriptions[i];
            if (i + 1 < Path.SegmentsDescriptions.Count) _nextSegment[segment] = Path.SegmentsDescriptions[i + 1];

            cumulativeDistance += segment.segment.PathCreator.path.length;
            _cumulativeDistanceAtEachSegment[segment] = cumulativeDistance;
        }
    }

    public override void Draw()
    {

    }

    public event EventHandler Destroyed;

    public override bool UpdateAgent(float modelTime)
    {
        if (!_isActive) return true;
        if (_isStopped) return false;

        _lastTime ??= modelTime;
        var delta = modelTime - _lastTime.Value;
        _distanceTravelled += _effectiveSpeed * delta;
        _lastTime = modelTime;

        if (_distanceTravelled > _cumulativeDistanceAtEachSegment[CurrentSegmentDescription])
        {
            if (!_nextSegment.TryGetValue(CurrentSegmentDescription, out var nextSegment))
            {
                CurrentSegmentDescription.segment.Statistics.pedestrianCount--;
                Destroyed?.Invoke(this, EventArgs.Empty);

                Path.to.Enqueue(() =>
                {
                    _isActive = false;
                    Destroy(gameObject);
                });

                _isStopped = true;
                return false;
            }
            var stat = CurrentSegmentDescription.segment.Statistics;

            if (nextSegment.segment.TrafficLight != null && !nextSegment.segment.TrafficLight.IsOpen)
            {
                stat.AppendTrafficLightWaitingTime(this, delta);
                _distanceTravelled = _cumulativeDistanceAtEachSegment[CurrentSegmentDescription];
            }
            else
            {
                CurrentSegmentDescription.segment.Statistics.pedestrianCount--;
                CurrentSegmentDescription.segment.Statistics.RemovePedestrianFromTrackingWaitingTime(this, EventArgs.Empty);
                _startDistance = _cumulativeDistanceAtEachSegment[CurrentSegmentDescription];
                CurrentSegmentDescription = nextSegment;
                CurrentSegmentDescription.segment.Statistics.pedestrianCount++;
            }
            if (nextSegment.segment.TrafficLight != null && nextSegment.segment.TrafficLight.IsOpen)
                stat.AppendTrafficLightWaitingTime(this, 0f);
        }

        float distance;
        if (Path.isReversed ^ CurrentSegmentDescription.traverseReversed)
            distance = _startDistance + CurrentSegmentDescription.segment.PathCreator.path.length - _distanceTravelled;
        else
            distance = _distanceTravelled - _startDistance;

        Position = CurrentSegmentDescription.segment.PathCreator.path.GetPointAtDistance(distance, EndOfPathInstruction.Stop);

        var count = 0;
        foreach (var agent in _population.Agents)
        {
            if (Vector3.Distance(agent.Position, Position) < 0.5f)
                count++;
        }

        if (count > 4)
        {
            _effectiveSpeed = Speed / 2f;
            IsSlowed = true;
        }
        else
        {
            IsSlowed = false;
            _effectiveSpeed = Speed;
        }

        return false;
    }

}
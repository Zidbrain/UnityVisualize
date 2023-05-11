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
    public PathSegment CurrentSegment { get; private set; }
    private float _startDistance;
    private Dictionary<PathSegment, float> _cumulativeDistanceAtEachSegment;
    private Dictionary<PathSegment, PathSegment> _nextSegment;

    private static int s_no = 0;

    private PedestrianPopulation _population;

    public bool IsSlowed { get; private set; }

    public static Pedestrian Create(GameObject gameObject, Path path, float speed, PedestrianPopulation population)
    {
        var pd = new GameObject($"Pedestrian {s_no}", typeof(Pedestrian));
        s_no++;
        pd.transform.SetParent(gameObject.transform);
        var ret = pd.GetComponent<Pedestrian>();
        if (path.isReversed ^ path.traverseReversed.Contains(path.Segments[0]))
            ret.Position = path.Segments[0].PathCreator.path.GetPointAtDistance(path.Segments[0].PathCreator.path.length, EndOfPathInstruction.Stop);
        else
            ret.Position = path.Segments[0].PathCreator.path.GetPointAtDistance(0f);

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
        _cumulativeDistanceAtEachSegment = new Dictionary<PathSegment, float>();
        _nextSegment = new Dictionary<PathSegment, PathSegment>();
        CurrentSegment = Path.Segments[0];
        CurrentSegment.Statistics.pedestrianCount++;
        _effectiveSpeed = Speed;

        var cumulativeDistance = 0f;
        for (int i = 0; i < Path.Segments.Count; i++)
        {
            var segment = Path.Segments[i];
            if (i + 1 < Path.Segments.Count) _nextSegment[segment] = Path.Segments[i + 1];

            cumulativeDistance += segment.PathCreator.path.length;
            _cumulativeDistanceAtEachSegment[segment] = cumulativeDistance;
        }
    }

    public override void Draw()
    {

    }

    public event EventHandler Destroyed;

    public override bool UpdateAgent(float modelTime)
    {
        _lastTime ??= modelTime;
        var delta = modelTime - _lastTime.Value;
        _distanceTravelled += _effectiveSpeed * delta;
        _lastTime = modelTime;

        if (_distanceTravelled > _cumulativeDistanceAtEachSegment[CurrentSegment])
        {
            if (!_nextSegment.TryGetValue(CurrentSegment, out var nextSegment))
            {
                CurrentSegment.Statistics.pedestrianCount--;
                Destroyed?.Invoke(this, EventArgs.Empty);
                Destroy(gameObject);
                return true;
            }
            var stat = CurrentSegment.Statistics;

            if (nextSegment.TrafficLight != null && !nextSegment.TrafficLight.IsOpen)
            {
                stat.AppendTrafficLightWaitingTime(this, delta);
                _distanceTravelled = _cumulativeDistanceAtEachSegment[CurrentSegment];
            }
            else
            {
                CurrentSegment.Statistics.pedestrianCount--;
                CurrentSegment.Statistics.RemovePedestrianFromTrackingWaitingTime(this, EventArgs.Empty);
                _startDistance = _cumulativeDistanceAtEachSegment[CurrentSegment];
                CurrentSegment = nextSegment;
                CurrentSegment.Statistics.pedestrianCount++;
            }
            if (nextSegment.TrafficLight != null && nextSegment.TrafficLight.IsOpen)
                stat.AppendTrafficLightWaitingTime(this, 0f);
        }

        float distance;
        if (Path.isReversed ^ Path.traverseReversed.Contains(CurrentSegment))
            distance = _startDistance + CurrentSegment.PathCreator.path.length - _distanceTravelled;
        else
            distance = _distanceTravelled - _startDistance;

        Position = CurrentSegment.PathCreator.path.GetPointAtDistance(distance, EndOfPathInstruction.Stop);

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
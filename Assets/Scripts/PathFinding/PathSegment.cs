using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using PathCreation;
using UnityEngine;

namespace Assets.Scripts.PathFinding
{
    public class PathSegment : MonoBehaviour
    {
        public TrafficLight TrafficLight;

        public PathCreator PathCreator { get; set; }
        public Path Path { get; set; }

        public PathSegmentStatistics Statistics { get; set; }

        void Start()
        {
            if (Path == null)
            {
                Path = transform.parent.GetComponent<Path>();
            }
            if (PathCreator == null)
                PathCreator = GetComponent<PathCreator>();
            if (Statistics == null)
            {
                Statistics = gameObject.AddComponent<PathSegmentStatistics>();
                Statistics.pathSegment = this;
            }
        }

        public PathSegment CopyTo(GameObject parent)
        {
            var go = new GameObject(name, typeof(PathSegment));
            var stat = go.AddComponent<PathSegmentStatistics>();
            var segment = go.GetComponent<PathSegment>();
            go.transform.SetParent(parent.transform);
            segment.PathCreator = PathCreator;
            segment.TrafficLight = TrafficLight;
            segment.Statistics = stat;
            return segment;
        }
    }
}
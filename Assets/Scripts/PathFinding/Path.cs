using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.PathFinding
{
    public class Path : MonoBehaviour
    {
        public List<PathSegmentDescription> SegmentsDescriptions = new List<PathSegmentDescription>();

        public bool isReversed;

        public Node from, to;

        private void Start()
        {
            if (SegmentsDescriptions.Count == 0)
                SegmentsDescriptions = GetComponentsInChildren<PathSegment>().Select(x => new PathSegmentDescription(x, false))
                    .ToList();
        }

        public float length => SegmentsDescriptions.Sum(s => s.segment.PathCreator.path.length);

        public void Add(IEnumerable<PathSegmentDescription> descriptions) => SegmentsDescriptions.AddRange(descriptions);
    }

    [Serializable]
    public class PathSegmentDescription
    {
        public PathSegment segment;
        public bool traverseReversed;

        public PathSegmentDescription(PathSegment segment, bool traverseReversed)
        {
            this.segment = segment;
            this.traverseReversed = traverseReversed;
        }
    }
}

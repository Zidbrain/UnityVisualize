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
        public List<PathSegment> Segments = new List<PathSegment>();

        public bool isReversed;
        public List<PathSegment> traverseReversed = new List<PathSegment>();

        private void Start()
        {
            if (Segments.Count == 0)
                Segments = GetComponentsInChildren<PathSegment>()
                    .ToList();
        }

        public float length => Segments.Sum(s => s.PathCreator.path.length);

        public void Add(IEnumerable<PathSegment> segments) => Segments.AddRange(segments);
    }
}

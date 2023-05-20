using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Assets.Scripts.Jobs;
using Assets.Scripts.Modeling;
using Assets.Scripts.PathFinding;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.WSA;
using R = UnityEngine.Random;

namespace Assets.Scripts
{
    public class PedestrianPopulation : DrawablePopulation<Pedestrian>
    {
        private Heatmap _heatmap;

        private Dictionary<string, Node> _nodes;
        private Vector2 min, max;

        public Schedule schedule;

        private List<(PoissonFlow flow, float startTime, float endTime)> _flows = new List<(PoissonFlow, float, float)>();

        private void AddAgentsStrategy()
        {

            foreach (var rec in schedule.records)
            {
                var capture = rec;
                _flows.Add((new PoissonFlow(rec.rate, time =>
                {
                    AddPedestrian(_nodes[capture.from], _nodes[capture.to]);
                }), rec.fromTime, rec.toTime));
            }
        }

        public void Init()
        {
            AddAgentsStrategy();
        }

        private void AddPedestrian(Node start, Node end)
        {
            var speed = R.Range(4, 7) / 60f * 1000f;
            var pedestrian = Pedestrian.Create(gameObject, go => GetPath(start, end, go), speed, this);
            Agents.Add(pedestrian);
        }

        private Path GetPath(Node start, Node end, GameObject attachTo)
        {
            var path = new List<Node>();
            if (start == end) return null;

            var unvisited = new List<Node>();
            var previous = new Dictionary<Node, Node>();
            var distances = new Dictionary<Node, float>();
            foreach ((var name, var node) in _nodes)
            {
                unvisited.Add(node);
                distances.Add(node, float.PositiveInfinity);
            }
            distances[start] = 0f;

            while (unvisited.Count > 0)
            {
                unvisited = unvisited.OrderBy(node => distances[node]).ToList();
                var current = unvisited[0];
                unvisited.Remove(current);

                if (current == end)
                {
                    while (previous.ContainsKey(current))
                    {
                        path.Insert(0, current);
                        current = previous[current];
                    }
                    path.Insert(0, current);
                    break;
                }

                for (int i = 0; i < current.connections.Count; i++)
                {
                    Node neighbor = current.connections[i].node;

                    // Getting the distance between the current node and the connection (neighbor)
                    float length = current.connections[i].path.length;

                    // The distance from start node to this connection (neighbor) of current node
                    float alt = distances[current] + length;

                    // A shorter path to the connection (neighbor) has been found
                    if (alt < distances[neighbor])
                    {
                        distances[neighbor] = alt;
                        previous[neighbor] = current;
                        if (!unvisited.Contains(neighbor))
                            unvisited.Add(neighbor);
                    }
                }
            }

            // var go = new GameObject($"{start.name}-{end.name}", typeof(Path));
            //   go.transform.SetParent(gameObject.transform);
            var result = attachTo.AddComponent<Path>();
            result.from = start;
            result.to = end;
            for (var i = 0; i < path.Count - 1; i++)
            {
                var connection = path[i].GetConnectionWith(path[i + 1]);
                IEnumerable<PathSegmentDescription> adding = connection.path.SegmentsDescriptions;
                if (connection.reversePath)
                {
                    adding = adding.Reverse().Select(x => new PathSegmentDescription(x.segment, !x.traverseReversed)).ToList();
                }
                else
                {
                    adding = adding.ToList();
                }
                result.Add(adding);
            }
            return result;
        }

        public override void Start()
        {
            var nodes = GameObject.Find("Nodes").GetComponentsInChildren<Node>();
            _nodes = nodes.ToDictionary(n => n.name);

            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            foreach (var node in nodes)
            {
                if (node.transform.position.x < min.x) min.x = node.transform.position.x;
                if (node.transform.position.z < min.y) min.y = node.transform.position.z;
                if (node.transform.position.x > max.x) max.x = node.transform.position.x;
                if (node.transform.position.z > max.y) max.y = node.transform.position.z;
            }
            this.min = min;
            this.max = max;

            _heatmap = GameObject.Find("Heatmap").GetComponent<Heatmap>();

            base.Start();
        }

        public override bool UpdateAgent(float modelTime)
        {
            var upd = base.UpdateAgent(modelTime);

            foreach ((var flow, var startTime, var endTime) in _flows)
            {
                if (modelTime >= startTime && modelTime <= endTime) 
                    flow.Start(startTime);
                else 
                    flow.Stop();

                flow.UpdateAgent(modelTime);
            }

            var magnitude = new int[100];
            foreach (var ped in Agents)
            {
                var x = (int)((ped.Position.x - min.x) / (max.x - min.x) * 10);
                var y = (int)((ped.Position.z - min.y) / (max.y - min.y) * 10);
                if (x > 9) x = 9;
                if (y > 9) y = 9;
                magnitude[y * 10 + x]++;
            }
            _heatmap.SetMagnitudes(magnitude, min, max);

            return upd;
        }
    }
}
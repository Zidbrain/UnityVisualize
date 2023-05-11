using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Assets.Scripts.Jobs;
using Assets.Scripts.Modeling;
using Assets.Scripts.PathFinding;
using UnityEngine;

using R = UnityEngine.Random;

namespace Assets.Scripts
{
    public class PedestrianPopulation : DrawablePopulation<Pedestrian>
    {
        private Heatmap _heatmap;

        private Dictionary<string, Node> _nodes;
        private Vector2 min, max;

        public Schedule schedule;

        private List<(float time, string from, string to)> _addTimes = new List<(float time, string from, string to)>();
        private int _startIndex;

        private void AddAgentsStrategy()
        {

            foreach (var rec in schedule.records)
            {
                for (int i = 0; i < rec.amount; i++)
                {
                    _addTimes.Add((R.Range(rec.fromTime, rec.toTime), rec.from, rec.to));
                }
            }

            _addTimes = _addTimes.OrderBy(x => x.time).ToList();
        }

        public void Init()
        {
            AddAgentsStrategy();
        }

        private Path GetPath(Node start, Node end)
        {
            var donePath = GameObject.Find($"{start.name}-{end.name}");
            if (donePath != null)
            {
                return donePath.GetComponent<Path>();
            }
            donePath = GameObject.Find($"{end.name}-{start.name}");
            if (donePath != null)
            {
                var go1 = new GameObject($"{start.name}-{end.name}", typeof(Path));
                go1.transform.SetParent(gameObject.transform);
                var res = go1.GetComponent<Path>();
                var source = donePath.GetComponent<Path>();
                res.Add(source.Segments.AsEnumerable().Reverse().ToList());
                res.traverseReversed.AddRange(source.traverseReversed);
                res.isReversed = true;
                return res;
            }

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

            var go = new GameObject($"{start.name}-{end.name}", typeof(Path));
            go.transform.SetParent(gameObject.transform);
            var result = go.GetComponent<Path>();
            for (var i = 0; i < path.Count - 1; i++)
            {
                var connection = path[i].GetConnectionWith(path[i + 1]);
                IEnumerable<PathSegment> adding = connection.path.Segments;
                if (connection.reversePath)
                {
                    adding = adding.Reverse().ToList();
                    result.traverseReversed.AddRange(adding);
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

        private Job _job;
        public override bool UpdateAgent(float modelTime)
        {
            //void AddPedestrian()
            //{
            //    var speed = R.Range(4, 7) / 60f * 1000f;
            //    var destinationChance = R.value;
            //    var start = "BMSTU";
            //    string destination = "ULK";
            //    if (destinationChance <= 1f / 4f)
            //        destination = "Baumanskaya";
            //    else if (destinationChance <= 2f / 4f)
            //        destination = "Energo";
            //    else if (destinationChance <= 3f / 4f)
            //        destination = "ULK";
            //    else
            //    {
            //        start = "Baumanskaya";
            //        destination = "Energo";
            //    }
            //    for (int i = 0; i < R.Range(1, 7); i++)
            //    {
            //        var pedestrian = Pedestrian.Create(gameObject, GetPath(_nodes[start], _nodes[destination]), speed, this);
            //        Agents.Add(pedestrian);
            //    }
            //}

            //var waitTime = 1f / (Math.Pow(Math.Cos(Math.PI * modelTime / 90f), 2d) * 30d + 2f);

            //_job ??= Job.Wait(modelTime, (float)waitTime, _ =>
            //    {
            //        AddPedestrian();
            //        _job = null;
            //    }).Start();

            var upd = base.UpdateAgent(modelTime);

            for (int i = _startIndex; i < _addTimes.Count; i++)
            {
                var rec = _addTimes[i];
                if (modelTime >= rec.time)
                {
                    var toAdd = 1;
                    var speed = R.Range(4, 7) / 60f * 1000f;
                    for (int j=0; j< toAdd; j++)
                    {
                        var pedestrian = Pedestrian.Create(gameObject, GetPath(_nodes[rec.from], _nodes[rec.to]), speed, this);
                        Agents.Add(pedestrian);
                    }
                    _startIndex = i + 1;
                }
                else
                   break;
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
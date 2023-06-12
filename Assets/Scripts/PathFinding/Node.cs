using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Random;
using UnityEngine;
using UnityEngine.Video;

namespace Assets.Scripts.PathFinding
{
    [Serializable]
    public class NodeConnection
    {
        public Path path;
        public Node node;
        public bool reversePath;
        public float weight;

        public NodeConnection(Path path, Node node, bool reversePath, float weight)
        {
            this.path = path;
            this.node = node;
            this.reversePath = reversePath;
            this.weight = weight;
        }
    }

    public class Node : Agent
    {
        public List<NodeConnection> connections;

        public float serviceRate;

        private Queue<Action> _queue = new Queue<Action>();
        private PoissonFlow _serviceFlow;
        private float _currentTime;

        private NodeStatistics _statistics;

        private Dictionary<Node, List<NodeConnection>> _distributedConnections = new Dictionary<Node, List<NodeConnection>>();

        public void Enqueue(Action onPassed)
        {
            _queue.Enqueue(onPassed);
            _statistics.Queued();
            if (_queue.Count == 1)
                _serviceFlow.Start(_currentTime);
        }

        private void Start()
        {
            _statistics = gameObject.GetComponent<NodeStatistics>() ?? gameObject.AddComponent<NodeStatistics>();

            _serviceFlow = new PoissonFlow(serviceRate, time =>
            {
                    _queue.Dequeue()();
                    _statistics.Served();
                if (_queue.Count == 0)
                    _serviceFlow.Stop();
            });

            foreach (var connection in connections)
            {
                if (!_distributedConnections.ContainsKey(connection.node))
                    _distributedConnections.Add(connection.node, new NodeConnection[] { connection }.ToList());
                else
                    _distributedConnections[connection.node].Add(connection);
            }
        }

        public NodeConnection GetConnectionWith(Node node)
        {
            var connections = _distributedConnections[node];
            var value = UnityEngine.Random.Range(0f, 1f);
            var min = 0f;
            foreach (var conn in connections)
            {
                if (min + conn.weight > value) return conn;
                min += conn.weight;
            }
            return connections.Last();
        }

        public override bool UpdateAgent(float modelTime)
        {
           // if (_serviceFlow.IsStopped) _serviceFlow.Start(modelTime);

            _currentTime = modelTime;
            _serviceFlow.UpdateAgent(modelTime);

            return false;
        }
    }
}
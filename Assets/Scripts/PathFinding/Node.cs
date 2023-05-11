using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.PathFinding
{
    [Serializable]
    public class NodeConnection
    {
        public Path path;
        public Node node;
        public bool reversePath;

        public NodeConnection(Path path, Node node, bool reversePath)
        {
            this.path = path;
            this.node = node;
            this.reversePath = reversePath;
        }
    }

    public class Node : MonoBehaviour
    {
        public List<NodeConnection> connections;

        public NodeConnection GetConnectionWith(Node node) =>
            connections.First(connection => connection.node == node);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class TrafficLight : Agent
    {
        private MeshRenderer _meshRenderer;

        private void Start()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        public bool IsOpen { get; private set; }

        public bool StartsWithOpen = false;
        public float OpenTime = 1f / 3f;
        public float CloseTime = 1f;

        public override bool UpdateAgent(float modelTime)
        {
            var completeTime = OpenTime + CloseTime;
            var timeInside = modelTime % completeTime;
            if (StartsWithOpen)
                IsOpen = timeInside <= OpenTime;
            else
                IsOpen = timeInside >= CloseTime;

            _meshRenderer.material.color = IsOpen ? Color.green : Color.red;

            return false;
        }
    }
}
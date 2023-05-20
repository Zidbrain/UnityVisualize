
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.PathFinding
{
    [RequireComponent(typeof(MeshFilter))]
    public class NodeStatistics : MonoBehaviour
    {
        private Mesh _mesh;
        private MeshCollider _meshCollider;
        private Outline _outline;
        private UIBase uiBase;

        private int _servedCount;
        private int _queueCount;
        private float _avgQueueLength;
        private int _queueSamples;

        public void Served()
        {
            _queueCount--;
            _servedCount++;
        }

        public void Queued()
        {
            _queueCount++;

            _queueSamples++;
            _avgQueueLength = (_avgQueueLength * (_queueSamples - 1) + _queueCount - 1) / _queueSamples;
        }

        private void Start()
        {
            _mesh = GetComponent<MeshFilter>().mesh;
            _meshCollider = gameObject.AddComponent<MeshCollider>();
            _meshCollider.sharedMesh = _mesh;

            _outline = gameObject.AddComponent<Outline>();
            _outline.OutlineColor = new Color(0, 0, 0, 0);
            _outline.OutlineMode = Outline.Mode.OutlineVisible;
            _outline.OutlineWidth = 5;

            uiBase = GameObject.Find("Canvas").GetComponent<UIBase>();
        }

        private string GetStatistics()
        {
            return $"K: {_servedCount}\n" +
                $"QLq: {(_queueCount - 1).CoerceAtMin(0)}\n" +
                $"QLavg: {_avgQueueLength}";
        }

        private void Update()
        {
            var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out var hitInfo) && hitInfo.collider == _meshCollider)
            {
                uiBase.AddStatisticsTracking(this, hitInfo.point + new Vector3(0f, 20f, 0f), GetStatistics);

                _outline.OutlineColor = Color.green;

                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    uiBase.FlipLockStatisticTracking(this);
                }
            }
            else
            {
                uiBase.RemoveStatisticsTracking(this, false);

                _outline.OutlineColor = new Color(0, 0, 0, 0);
            }
        }
    }
}
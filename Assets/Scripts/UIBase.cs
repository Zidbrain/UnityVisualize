using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using Assets.Scripts;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Video;
using System.Text;

[RequireComponent(typeof(Canvas))]
public class UIBase : MonoBehaviour
{
    private class StatisticsUI
    {
        public GameObject go;
        public RectTransform transform;
        public TextMeshProUGUI text;
        public bool isLocked;
        public Vector3 worldPosition;

        public StatisticsUI(GameObject basedOn, Vector3 worldPosition)
        {
            transform = basedOn.GetComponent<RectTransform>();
            text = basedOn.GetComponent<TextMeshProUGUI>();
            go = basedOn;
            this.worldPosition = worldPosition;
        }
    }

    public RectTransform modelStats;
    public TextMeshProUGUI modelTime;
    public TextMeshProUGUI modelSpeed;

    private Dictionary<PathSegmentStatistics, StatisticsUI> _statistics;
    private int _id;

    private Canvas _canvas;

    private string GetStatistics(PathSegmentStatistics stats)
    {
        var builder = new StringBuilder();
        builder.Append($"N: {stats.pedestrianCount}\n");
        builder.Append($"K: {stats.Busyness}");

        if (stats.TrafficLightWaitingTime != 0)
        {
            builder.Append($"\nLtavg: {stats.TrafficLightWaitingTime * 60}\n" +
                $"Lavg: {stats.TrafficLightQueueLength}\n" +
                $"L: {stats.CurrentQueueLength}");
        }

        return builder.ToString();
    }
    
    void Update()
    {
        foreach ((var statistics, var ui) in _statistics)
        {
            ui.text.text = GetStatistics(statistics);

            if (!ui.isLocked)
            {
                var mp = Mouse.current.position.ReadValue();
                ui.transform.anchoredPosition = new Vector2(mp.x, mp.y + 20f) / _canvas.scaleFactor;
            }
            else
            {
                var mp = Camera.main.WorldToScreenPoint(ui.worldPosition);
                ui.transform.anchoredPosition = new Vector2(mp.x, mp.y + 20f) / _canvas.scaleFactor;
            }
        }
    }

    void Start()
    {
        SetTime(0f);
        SetSpeed(0f);
        _statistics = new Dictionary<PathSegmentStatistics, StatisticsUI>();
        _canvas = GetComponent<Canvas>();
    }

    public void RemoveStatisticsTracking(PathSegmentStatistics statistics, bool removeIfLocked)
    {
        if (_statistics.ContainsKey(statistics) && (removeIfLocked || !_statistics[statistics].isLocked))
        {
            Destroy(_statistics[statistics].go);
            _statistics.Remove(statistics);
        }
    }

    public void FlipLockStatisticTracking(PathSegmentStatistics statistics)
    {
        _statistics[statistics].isLocked = !_statistics[statistics].isLocked;
    }

    public void AddStatisticsTracking(PathSegmentStatistics statistics, Vector3 worldPoint)
    {
        if (_statistics.ContainsKey(statistics))
        {
            if (!_statistics[statistics].isLocked)
                _statistics[statistics].worldPosition = worldPoint;
            return;
        }

        var go = new GameObject(_id.ToString(), typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(gameObject.transform, false);
        var ui = new StatisticsUI(go, worldPoint);
        var tf = ui.transform;
        tf.pivot = new Vector2(0.5f, 0f);
        tf.anchorMin = Vector2.zero;
        tf.anchorMax = Vector2.zero;
        tf.sizeDelta = new Vector2(600, 300);
        ui.text.alignment = TextAlignmentOptions.Bottom;
        ui.text.outlineColor = new Color32(0, 0, 0, 255);
        ui.text.outlineWidth = 0.2f;
        ui.text.fontSize = 22f;
        _statistics.Add(statistics, ui);
        _id++;
    }

    public void SetTime(float time)
    {
        var hours = (7 + (int)time / 60) % 24;
        var minutes = (int)time % 60;
        modelTime.text = $"Модельное время: {hours:00}:{minutes:00}";
    }

    public void SetSpeed(float speed)
    {
        modelSpeed.text = $"Скорость модели: {speed}x";
    }
}

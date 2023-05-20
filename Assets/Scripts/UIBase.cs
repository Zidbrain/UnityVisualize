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
using System;
using MathNet.Numerics.Statistics;

[RequireComponent(typeof(Canvas))]
public class UIBase : MonoBehaviour
{
    public class StatisticsUI
    {
        public GameObject go;
        public RectTransform transform;
        public TextMeshProUGUI text;
        public bool isLocked;
        public Vector3 worldPosition;

        public Func<string> GetText { get; }

        private static int _id;

        public StatisticsUI(GameObject basedOn, Vector3 worldPosition, Func<string> getText)
        {
            transform = basedOn.GetComponent<RectTransform>();
            text = basedOn.GetComponent<TextMeshProUGUI>();
            go = basedOn;
            this.worldPosition = worldPosition;
            GetText = getText;
        }

        public static StatisticsUI Create(GameObject parent, Vector3 worldPoint, Func<string> getText)
        {
            var go = new GameObject(_id.ToString(), typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent.transform, false);
            var ui = new StatisticsUI(go, worldPoint, getText);
            var tf = ui.transform;
            tf.pivot = new Vector2(0.5f, 0f);
            tf.anchorMin = Vector2.zero;
            tf.anchorMax = Vector2.zero;
            tf.sizeDelta = new Vector2(600, 300);
            ui.text.alignment = TextAlignmentOptions.Bottom;
            ui.text.outlineColor = new Color32(0, 0, 0, 255);
            ui.text.outlineWidth = 0.2f;
            ui.text.fontSize = 22f;
            _id++;

            return ui;
        }
    }

    public RectTransform modelStats;
    public TextMeshProUGUI modelTime;
    public TextMeshProUGUI modelSpeed;

    private Dictionary<object, StatisticsUI> _statistics;

    private Canvas _canvas;
    
    void Update()
    {
        foreach ((var _, var ui) in _statistics)
        {
            ui.text.text = ui.GetText();

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
        _statistics = new Dictionary<object, StatisticsUI>();
        _canvas = GetComponent<Canvas>();
    }

    public void RemoveStatisticsTracking(object reference, bool removeIfLocked)
    {
        if (_statistics.ContainsKey(reference) && (removeIfLocked || !_statistics[reference].isLocked))
        {
            Destroy(_statistics[reference].go);
            _statistics.Remove(reference);
        }
    }

    public void FlipLockStatisticTracking(object reference)
    {
        _statistics[reference].isLocked = !_statistics[reference].isLocked;
    }

    public void AddStatisticsTracking(object reference, Vector3 worldPoint, Func<string> getStatisticsText)
    {
        if (_statistics.ContainsKey(reference))
        {
            if (!_statistics[reference].isLocked)
                _statistics[reference].worldPosition = worldPoint;
            return;
        }

        _statistics.Add(reference, StatisticsUI.Create(gameObject, worldPoint, getStatisticsText));
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

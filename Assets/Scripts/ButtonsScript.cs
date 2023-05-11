using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ButtonsScript : MonoBehaviour
{
    private MainAgent _mainAgent;
    private Text _buttonText;
    private bool _isPaused = false;
    private float _lastSpeed;

    private readonly GlobalSettings _settings = GlobalSettings.Default;
    private TextMeshProUGUI _visualizeText;
    private TextMeshProUGUI _heatmapText;
    private GameObject _heatmap;

    private void Start()
    {
        _mainAgent = GameObject.Find("World").GetComponent<MainAgent>();
        _buttonText = GameObject.Find("Buttons/PauseResume/Text").GetComponent<Text>();
        _visualizeText = GameObject.Find("EnableButtons/RoadsEnable/RoadsText").GetComponent<TextMeshProUGUI>();
        _heatmapText = GameObject.Find("EnableButtons/HeatmapEnable/HeatmapText").GetComponent<TextMeshProUGUI>();
        _heatmap = GameObject.Find("Heatmap");

        _lastSpeed = _mainAgent.ModelSpeed;
        HandlePause();

        if (_settings.VisualizeRoads)
            _visualizeText.text = "Выкл дороги";
        else _visualizeText.text = "Вкл дороги";
    }

    private void HandlePause()
    {
        if (_isPaused)
        {
            _lastSpeed = _mainAgent.ModelSpeed;
            _mainAgent.ModelSpeed = 0f;
            _buttonText.text = ">";
        }
        else
        {
            _mainAgent.ModelSpeed = _lastSpeed;
            _buttonText.text = "| |";
        }
    }

    public void SlowDown()
    {
        _mainAgent.ModelSpeed /= 2f;
    }

    public void SpeedUp()
    {
        if (_mainAgent.ModelSpeed < 32)
            _mainAgent.ModelSpeed *= 2f;
    }

    public void PauseOrResume()
    {
        _isPaused = !_isPaused;
        HandlePause();
    }

    public void SwitchVisualizeRoads()
    {
        _settings.VisualizeRoads = !_settings.VisualizeRoads;

        if (_settings.VisualizeRoads)
            _visualizeText.text = "Выкл дороги";
        else _visualizeText.text = "Вкл дороги";
    }

    public void SwitchHeatmap()
    {
        _heatmap.SetActive(!_heatmap.activeSelf);
        if (_heatmap.activeSelf)
            _heatmapText.text = "Выкл теп. карту";
        else
            _heatmapText.text = "Вкл теп. карту";
    }
}

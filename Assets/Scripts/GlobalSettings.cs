using System;

public class GlobalSettings
{
    private GlobalSettings() { }

    public static GlobalSettings Default { get; } = new GlobalSettings();

    private bool _visualizeRoads = true;
    public bool VisualizeRoads
    {
        get => _visualizeRoads;
        set
        {
            if (_visualizeRoads == value) return;
            _visualizeRoads = value;
            OnVisualizeRoadsChanged(this, new VisualizeRoadsChangedEventArgs(value));
        }
    }

    public event EventHandler<VisualizeRoadsChangedEventArgs> OnVisualizeRoadsChanged;
}

public class VisualizeRoadsChangedEventArgs : EventArgs
{
    public bool NewValuse { get; }

    public VisualizeRoadsChangedEventArgs(bool newValue) : base()
    {
        NewValuse = newValue;
    }
}
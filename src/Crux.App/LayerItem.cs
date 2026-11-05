using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Crux.App;

internal enum LayerKind
{
    Classic,
    Shape,
    Text,
    Image,
}

internal sealed class LayerItem : INotifyPropertyChanged
{
    private readonly Action<LayerItem, bool> visibilityChanged;
    private string title = string.Empty;
    private string subtitle = string.Empty;
    private LayerKind kind;
    private bool visible;

    public LayerItem(Action<LayerItem, bool> visibilityChanged)
    {
        this.visibilityChanged = visibilityChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public int DesignIndex { get; set; }

    public string Title
    {
        get => title;
        set => SetField(ref title, value);
    }

    public string Subtitle
    {
        get => subtitle;
        set => SetField(ref subtitle, value);
    }

    public LayerKind Kind
    {
        get => kind;
        set => SetField(ref kind, value);
    }

    public bool Visible
    {
        get => visible;
        set
        {
            if (SetField(ref visible, value))
            {
                visibilityChanged(this, value);
            }
        }
    }

    public void SetVisibleSilently(bool value) => SetField(ref visible, value, nameof(Visible));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}

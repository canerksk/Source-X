namespace SphereServer.Core;

/// <summary>
/// Base class for all game entities (items and characters).
/// Based on Source-X CObjBase.
/// </summary>
public abstract class Entity
{
    private static uint _nextSerial = Serial.CharMin;
    private static readonly object _serialLock = new();

    protected Entity(bool isItem)
    {
        Serial = AllocateSerial(isItem);
        Location = Point3D.Invalid;
        Hue = 0;
        Created = DateTime.UtcNow;
        IsDeleted = false;
        Properties = new Dictionary<string, object>();
        Tags = new Dictionary<string, object>();
    }

    public Serial Serial { get; private set; }
    public Point3D Location { get; set; }
    public ushort Hue { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public bool IsDeleted { get; protected set; }

    // Properties and Tags (similar to m_BaseDefs and m_TagDefs)
    public Dictionary<string, object> Properties { get; }
    public Dictionary<string, object> Tags { get; }

    public bool IsItem => Serial.IsItem;
    public bool IsChar => Serial.IsChar;

    private static Serial AllocateSerial(bool isItem)
    {
        lock (_serialLock)
        {
            if (isItem)
            {
                if (_nextSerial < Serial.ItemMin)
                    _nextSerial = Serial.ItemMin;
            }
            else
            {
                if (_nextSerial >= Serial.ItemMin)
                    _nextSerial = Serial.CharMin;
            }

            uint serial = _nextSerial++;

            // Wrap around if needed
            if (!isItem && serial >= Serial.ItemMin)
                serial = _nextSerial = Serial.CharMin;

            return new Serial(serial);
        }
    }

    public virtual void Delete()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        OnDelete();
    }

    protected virtual void OnDelete()
    {
        // Override in derived classes
    }

    public virtual bool MoveTo(Point3D location)
    {
        if (!location.IsValid)
            return false;

        Point3D oldLocation = Location;
        Location = location;
        OnLocationChanged(oldLocation, location);
        return true;
    }

    protected virtual void OnLocationChanged(Point3D oldLocation, Point3D newLocation)
    {
        // Override in derived classes
    }

    public virtual void Update()
    {
        // Override in derived classes to handle entity updates
    }

    public int GetDistance(Entity other)
    {
        return Location.GetDistance(other.Location);
    }

    public int GetDistance(Point3D point)
    {
        return Location.GetDistance(point);
    }

    public Direction GetDirection(Entity other)
    {
        return Location.GetDirection(other.Location);
    }

    public Direction GetDirection(Point3D point)
    {
        return Location.GetDirection(point);
    }

    // Property helpers
    public T? GetProperty<T>(string key, T? defaultValue = default)
    {
        if (Properties.TryGetValue(key, out object? value) && value is T typedValue)
            return typedValue;
        return defaultValue;
    }

    public void SetProperty(string key, object value)
    {
        Properties[key] = value;
    }

    public void DeleteProperty(string key)
    {
        Properties.Remove(key);
    }

    // Tag helpers
    public T? GetTag<T>(string key, T? defaultValue = default)
    {
        if (Tags.TryGetValue(key, out object? value) && value is T typedValue)
            return typedValue;
        return defaultValue;
    }

    public void SetTag(string key, object value)
    {
        Tags[key] = value;
    }

    public void DeleteTag(string key)
    {
        Tags.Remove(key);
    }

    public override string ToString()
    {
        return $"{GetType().Name} '{Name}' [{Serial}] at {Location}";
    }
}

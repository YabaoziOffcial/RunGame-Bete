public abstract class DataBase
{
    public T GetValue<T>(string key, T defaultValue = default)
    {
        if (!Values.TryGetValue(key, out object value) || value == null) return defaultValue;
        if (value is T typedValue) return typedValue;
        return defaultValue;
    }

    public void SetValue<T>(string key, T value)
    {
        Values[key] = value;
    }
    public System.Collections.Generic.Dictionary<string, object> Values { get; } = new();

    public abstract bool Save();
}
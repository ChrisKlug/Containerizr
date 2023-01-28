namespace Containerizr;

public class ContainerImageItems
{
    private Dictionary<string, object> items = new Dictionary<string, object>();

    public void SetItem(string key, object value)
    {
        items[key] = value;
    }
    public T? GetItem<T>(string key)
    {
        if (items.ContainsKey(key))
        {
            var item = items[key];
            if (item is T)
            {
                return (T)item;
            }
        }
        return default;
    }
}
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Serialization<TKey, TValue>
{
    public List<TKey> keys = new List<TKey>();
    public List<TValue> values = new List<TValue>();

    public Serialization(Dictionary<TKey, TValue> dict)
    {
        foreach (var pair in dict)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }
    
    public Serialization(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }

    public Dictionary<TKey, TValue> ToDictionary()
    {
        var dict = new Dictionary<TKey, TValue>();
        int count = Mathf.Min(keys.Count, values.Count);
        for (int i = 0; i < count; i++)
        {
            dict[keys[i]] = values[i];
        }
        return dict;
    }
}

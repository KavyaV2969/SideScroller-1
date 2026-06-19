using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    [SerializeField] private List<string> itemIds = new List<string>
    {
        "small_token"
    };

    private readonly HashSet<string> knownItems = new HashSet<string>();

    private void Awake()
    {
        foreach (string itemId in itemIds)
        {
            if (!string.IsNullOrWhiteSpace(itemId))
            {
                knownItems.Add(itemId);
            }
        }
    }

    public bool ContainsItem(string itemId)
    {
        return !string.IsNullOrWhiteSpace(itemId) && knownItems.Contains(itemId);
    }
}

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItemSnapshot
{
    public string itemId;
    public int quantity;
}

public class Inventory : MonoBehaviour
{
    [System.Serializable]
    private class InventoryEntry
    {
        public string itemId;
        public int quantity;
    }

    [SerializeField] private List<InventoryEntry> startingItems = new List<InventoryEntry>();

    private readonly Dictionary<string, int> items = new Dictionary<string, int>();

    private void Awake()
    {
        foreach (InventoryEntry entry in startingItems)
        {
            if (entry != null)
            {
                AddItem(entry.itemId, entry.quantity);
            }
        }
    }

    public void AddItem(string itemId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            Debug.LogWarning("Tried to add an item with an empty id.");
            return;
        }

        if (quantity <= 0)
        {
            Debug.LogWarning($"Tried to add a non-positive quantity of item '{itemId}'.");
            return;
        }

        if (!items.ContainsKey(itemId))
        {
            items[itemId] = 0;
        }

        items[itemId] += quantity;
    }

    public int GetQuantity(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId) || !items.ContainsKey(itemId))
        {
            return 0;
        }

        return items[itemId];
    }

    public bool HasItem(string itemId, int quantity = 1)
    {
        return GetQuantity(itemId) >= quantity;
    }

    public string[] GetItemIds()
    {
        string[] itemIds = new string[items.Count];
        int index = 0;

        foreach (string itemId in items.Keys)
        {
            itemIds[index] = itemId;
            index++;
        }

        return itemIds;
    }

    public InventoryItemSnapshot[] GetItems()
    {
        InventoryItemSnapshot[] snapshots = new InventoryItemSnapshot[items.Count];
        int index = 0;

        foreach (KeyValuePair<string, int> item in items)
        {
            snapshots[index] = new InventoryItemSnapshot
            {
                itemId = item.Key,
                quantity = item.Value
            };

            index++;
        }

        return snapshots;
    }
}

using UnityEngine;

public class GameStateProvider : MonoBehaviour
{
    [Header("Temporary Test Data")]
    [SerializeField] private string currentLocationId = "north_village";

    [SerializeField] private string[] activeQuestFlags;
    [SerializeField] private string[] completedQuestFlags;
    [SerializeField] private string[] inventoryItemIds;

    public PlayerStateSnapshot BuildSnapshotForNPC(string npcId)
    {
        return new PlayerStateSnapshot
        {
            currentLocationId = currentLocationId,
            activeQuestFlags = activeQuestFlags,
            completedQuestFlags = completedQuestFlags,
            inventoryItemIds = inventoryItemIds
        };
    }

    public bool HasFlag(string flagId)
    {
        if (string.IsNullOrWhiteSpace(flagId))
        {
            return false;
        }

        return HasActiveFlag(flagId) || HasCompletedFlag(flagId);
    }

    public bool HasActiveFlag(string flagId)
    {
        return Contains(activeQuestFlags, flagId);
    }

    public bool HasCompletedFlag(string flagId)
    {
        return Contains(completedQuestFlags, flagId);
    }

    public bool HasInventoryItem(string itemId)
    {
        return Contains(inventoryItemIds, itemId);
    }

    private bool Contains(string[] values, string target)
    {
        if (values == null || string.IsNullOrWhiteSpace(target))
        {
            return false;
        }

        foreach (string value in values)
        {
            if (value == target)
            {
                return true;
            }
        }

        return false;
    }
}
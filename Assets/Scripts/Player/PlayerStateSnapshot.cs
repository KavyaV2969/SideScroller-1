[System.Serializable]
public class PlayerStateSnapshot
{
    public string currentLocationId;
    public float playerX;
    public float playerY;
    public float moveInputX;
    public float moveInputY;
    public bool movementEnabled;
    public string[] activeQuestFlags;
    public string[] completedQuestFlags;
    public string[] inventoryItemIds;
    public InventoryItemSnapshot[] inventoryItems;
}

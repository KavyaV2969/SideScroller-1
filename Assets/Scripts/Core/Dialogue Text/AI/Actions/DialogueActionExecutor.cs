using UnityEngine;

public class DialogueActionExecutor : MonoBehaviour
{
    public void Execute(AIDialogueAction[] actions)
    {
        if (actions == null || actions.Length == 0)
        {
            return;
        }

        foreach (AIDialogueAction action in actions)
        {
            ExecuteAction(action);
        }
    }

    private void ExecuteAction(AIDialogueAction action)
    {
        if (action == null)
        {
            return;
        }

        switch (action.actionType)
        {
            case "give_item":
                GiveItem(action.itemId, action.quantity);
                break;

            case "set_flag":
                SetFlag(action.flagId);
                break;

            case "start_quest":
                StartQuest(action.questId);
                break;

            case "complete_quest":
                CompleteQuest(action.questId);
                break;

            default:
                Debug.LogWarning($"Unknown dialogue action type: {action.actionType}");
                break;
        }
    }

    private void GiveItem(string itemId, int quantity)
    {
        Debug.Log($"[DialogueActionExecutor] Give item: {itemId} x{quantity}");

        // Later:
        // inventory.AddItem(itemId, quantity);
    }

    private void SetFlag(string flagId)
    {
        Debug.Log($"[DialogueActionExecutor] Set flag: {flagId}");

        // Later:
        // questManager.SetFlag(flagId);
    }

    private void StartQuest(string questId)
    {
        Debug.Log($"[DialogueActionExecutor] Start quest: {questId}");

        // Later:
        // questManager.StartQuest(questId);
    }

    private void CompleteQuest(string questId)
    {
        Debug.Log($"[DialogueActionExecutor] Complete quest: {questId}");

        // Later:
        // questManager.CompleteQuest(questId);
    }
}
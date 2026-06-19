using UnityEngine;

public class DialogueActionValidator : MonoBehaviour
{
    [SerializeField] private GameStateProvider gameStateProvider;

    public bool AreActionsValid(string npcId, AIDialogueResponse response)
    {
        if (response == null)
        {
            return false;
        }

        if (response.proposedActions == null || response.proposedActions.Length == 0)
        {
            return true;
        }

        if (response.responseType == "free_chat")
        {
            Debug.LogWarning("Free-chat response tried to include actions. Rejected.");
            return false;
        }

        foreach (AIDialogueAction action in response.proposedActions)
        {
            if (!IsActionValid(npcId, response.intent, action))
            {
                Debug.LogWarning($"Rejected dialogue action: {action.actionType}");
                return false;
            }
        }

        return true;
    }

    private bool IsActionValid(string npcId, string intent, AIDialogueAction action)
    {
        if (action == null)
        {
            return false;
        }

        if (npcId == "mira_village_lass" &&
            intent == "mention_brother_helped")
        {
            return IsValidMiraBrotherRewardAction(action);
        }

        return false;
    }

    private bool IsValidMiraBrotherRewardAction(AIDialogueAction action)
    {
        bool helpedTomas = gameStateProvider.HasFlag("helped_tomas");
        bool rewardAlreadyClaimed = gameStateProvider.HasFlag("lass_reward_claimed");

        if (!helpedTomas || rewardAlreadyClaimed)
        {
            return false;
        }

        if (action.actionType == "give_item" &&
            action.itemId == "small_token" &&
            action.quantity == 1)
        {
            return true;
        }

        if (action.actionType == "set_flag" &&
            action.flagId == "lass_reward_claimed")
        {
            return true;
        }

        return false;
    }
}
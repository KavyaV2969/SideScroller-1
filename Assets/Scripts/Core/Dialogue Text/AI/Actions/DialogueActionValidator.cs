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

        if (response.responseType == "free_chat")
        {
            bool hasActions = response.proposedActions != null && response.proposedActions.Length > 0;
            if (hasActions)
            {
                Debug.LogWarning("Free-chat response tried to include actions. Rejected.");
            }

            return !hasActions;
        }

        if (response.proposedActions == null || response.proposedActions.Length == 0)
        {
            return true;
        }

        if (gameStateProvider == null)
        {
            Debug.LogWarning("DialogueActionValidator has no GameStateProvider assigned. Rejected.");
            return false;
        }

        if (npcId == "mira_village_lass" &&
            response.intent == "mention_brother_helped")
        {
            return AreMiraBrotherRewardActionsValid(response.proposedActions);
        }

        Debug.LogWarning($"Rejected actions for npc '{npcId}' and intent '{response.intent}'.");
        return false;
    }

    private bool AreMiraBrotherRewardActionsValid(AIDialogueAction[] actions)
    {
        bool helpedTomas = gameStateProvider.HasFlag("helped_tomas");
        bool rewardAlreadyClaimed = gameStateProvider.HasFlag("lass_reward_claimed");

        if (!helpedTomas || rewardAlreadyClaimed)
        {
            return false;
        }

        if (actions == null || actions.Length != 2)
        {
            return false;
        }

        bool foundGiveItem = false;
        bool foundSetFlag = false;

        foreach (AIDialogueAction action in actions)
        {
            if (action == null)
            {
                return false;
            }

            if (action.actionType == "give_item" &&
                action.itemId == "small_token" &&
                action.quantity == 1 &&
                string.IsNullOrWhiteSpace(action.flagId) &&
                string.IsNullOrWhiteSpace(action.questId))
            {
                if (foundGiveItem)
                {
                    return false;
                }

                foundGiveItem = true;
                continue;
            }

            if (action.actionType == "set_flag" &&
                action.flagId == "lass_reward_claimed" &&
                string.IsNullOrWhiteSpace(action.itemId) &&
                action.quantity == 0 &&
                string.IsNullOrWhiteSpace(action.questId))
            {
                if (foundSetFlag)
                {
                    return false;
                }

                foundSetFlag = true;
                continue;
            }

            Debug.LogWarning($"Rejected dialogue action: {action.actionType}");
            return false;
        }

        return foundGiveItem && foundSetFlag;
    }
}

using UnityEngine;
using MeanAlchemy.Dialog;

public class ConversationRunner : MonoBehaviour
{
    [SerializeField] private DialogController controller;
    [SerializeField] private ConversationAsset conversation;

    private int _idx;

    public void Begin()
    {
        if (!controller || !conversation || conversation.steps == null || conversation.steps.Length == 0)
        {
            Debug.LogError("ConversationRunner not set up.");
            return;
        }
        _idx = 0;
        Render();
    }

    private void Render()
    {
        if (_idx < 0 || _idx >= conversation.steps.Length) { controller.End(); return; }
        var step = conversation.steps[_idx];

        if (step.choices != null && step.choices.Length > 0)
        {
            var opts = new (string, System.Action)[step.choices.Length];
            for (int i = 0; i < step.choices.Length; i++)
            {
                int target = step.choices[i].gotoIndex; // capture for closure
                string label = step.choices[i].label;
                opts[i] = (label, () =>
                {
                    if (target >= 0) { _idx = target; Render(); }
                    else controller.End();
                });
            }

            controller.ShowChoices(step.speaker, step.text, opts);
        }
        else
        {
            controller.ShowLine(step.speaker, step.text, () =>
            {
                if (step.nextIndex >= 0) { _idx = step.nextIndex; Render(); }
                else controller.End();
            });
        }
    }
}
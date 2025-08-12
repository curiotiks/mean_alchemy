using UnityEngine;

namespace MeanAlchemy.Dialog
{
    public class DialogController : MonoBehaviour
    {
        [SerializeField] private DialogUI ui;
        [SerializeField] private MonoBehaviour playerControllerToDisable; // optional

        private bool _active;

        void Awake()
        {
            if (ui != null) ui.ShowPanel(false); //Start with panel hidden
        }

        // Show a single line with a Next button
        public void ShowLine(string speaker, string text, System.Action onNext)
        {
            EnsureOpen();
            ui.ClearChoices();
            ui.SetSpeaker(speaker);
            ui.SetLine(text);
            ui.ShowNext(true);
            ui.BindNext(() => onNext?.Invoke());
        }

        // Show choices (spawns buttons under ChoicesGroup)
        public void ShowChoices(string speaker, string prompt, (string label, System.Action onPick)[] options)
        {
            EnsureOpen();
            ui.SetSpeaker(speaker);
            ui.SetLine(prompt);
            ui.ClearChoices();
            ui.ShowNext(false);

            foreach (var opt in options)
                ui.AddChoice(opt.label, () => opt.onPick?.Invoke());
        }

        // Close the panel
        public void End()
        {
            if (!_active) return;
            _active = false;
            ui.ShowPanel(false);
            TogglePlayerControl(true);
        }

        // ---------- Helpers ----------
        private void EnsureOpen()
        {
            if (_active) return;
            _active = true;
            ui.ShowPanel(true);
            TogglePlayerControl(false);
        }

        private void TogglePlayerControl(bool enabled)
        {
            if (playerControllerToDisable != null)
                playerControllerToDisable.enabled = enabled;
        }
    }
}
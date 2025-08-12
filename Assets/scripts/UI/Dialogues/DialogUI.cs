using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace MeanAlchemy.Dialog
{
    public class DialogUI : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject panel;

        [Header("Text")]
        [SerializeField] private TMP_Text speakerName;
        [SerializeField] private TMP_Text lineText;

        [Header("Controls")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Transform choicesGroup;     // parent of spawned buttons
        [SerializeField] private Button choiceButtonPrefab;  // your ChoiceButton prefab

        // System.Action is a delegate type for a method with no parameters and no return value (void()).
        // We store the current Next-button callback here; BindNext wires it to nextButton.onClick
        // so clicking Next invokes this action. Example usage: BindNext(() => AdvanceToNextLine());
        private System.Action _onNext;

        public void ShowPanel(bool show) => panel.SetActive(show);

        public void SetSpeaker(string name)
        {
            if (!speakerName) return;
            if (string.IsNullOrWhiteSpace(name))
                speakerName.gameObject.SetActive(false);
            else { speakerName.gameObject.SetActive(true); speakerName.text = name; }
        }

        public void SetLine(string text) => lineText.text = text ?? string.Empty;


        //BindNext tells the Next button what to do when it’s clicked—nothing more.
        // It clears any old click actions and hooks up the new one you pass in.
        // It does not show/hide the button (use ShowNext(true/false) for that).
        public void BindNext(System.Action onNext)
        {
            _onNext = onNext;
            nextButton.onClick.RemoveAllListeners();
            if (_onNext != null) nextButton.onClick.AddListener(() => _onNext.Invoke());
        }

        public void ShowNext(bool show) => nextButton.gameObject.SetActive(show);

        public void ClearChoices()
        {
            if (!choicesGroup) return;
            for (int i = choicesGroup.childCount - 1; i >= 0; i--)
                Destroy(choicesGroup.GetChild(i).gameObject);
            choicesGroup.gameObject.SetActive(false);
        }

        public Button AddChoice(string label, System.Action onClick)
        {
            if (!choicesGroup || !choiceButtonPrefab) return null;
            if (!choicesGroup.gameObject.activeSelf) choicesGroup.gameObject.SetActive(true);

            var btn = Instantiate(choiceButtonPrefab, choicesGroup);
            var txt = btn.GetComponentInChildren<TMP_Text>();
            if (txt) txt.text = label ?? "";
            btn.onClick.AddListener(() => onClick?.Invoke());
            return btn;
        }
    }
}
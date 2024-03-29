using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cherrydev
{
    public class SentencePanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI dialogNameText;
        [SerializeField] private TextMeshProUGUI dialogText;
        [SerializeField] private Image dialogCharacterImage;
        [SerializeField] private Button nextBtn;
        public DialogBehaviour dialogBehaviour;

        private void Start()
        {
            dialogText.text = string.Empty;

            nextBtn.onClick.AddListener(() =>
            {
                dialogBehaviour.callNextNode_force();
            });
        }

        /// <summary>
        /// Setting dialogText text to empty string
        /// </summary>
        public void ResetDialogText()
        {
            dialogText.text = string.Empty;
            dialogCharacterImage.sprite = null;
        }

        /// <summary>
        /// Assigning dialog name text and character iamge sprite
        /// </summary>
        /// <param name="name"></param>
        public void AssignDialogNameTextAndSprite(string name, Sprite sprite)
        {
            dialogNameText.text = name;

            if (sprite == null)
            {
                dialogCharacterImage.color = new Color(dialogCharacterImage.color.r,
                    dialogCharacterImage.color.g, dialogCharacterImage.color.b, 0);
                return;
            }

            dialogCharacterImage.color = new Color(dialogCharacterImage.color.r,
                    dialogCharacterImage.color.g, dialogCharacterImage.color.b, 255);
            dialogCharacterImage.sprite = sprite;
        }

        /// <summary>
        /// Adding char to dialog text
        /// </summary>
        /// <param name="textChar"></param>
        public void AddCharToDialogText(char textChar)
        {
            dialogText.text += textChar;
        }
    }
}
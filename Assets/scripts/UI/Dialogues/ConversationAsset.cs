using UnityEngine;

[CreateAssetMenu(menuName = "Dialog/Conversation")]
public class ConversationAsset : ScriptableObject
{
    public Step[] steps;

    [System.Serializable] public class Step
    {
        public string speaker = "Mentor";
        [TextArea(2,5)] public string text;

        public Choice[] choices;     // if null/empty → show Next
        public int nextIndex = -1;   // -1 ends dialog

        [System.Serializable] public class Choice
        {
            public string label;
            public int gotoIndex = -1; // -1 ends dialog
        }
    }
}
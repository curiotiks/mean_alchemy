
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cherrydev;
using UnityEngine.UI;


public class Dialog_instance : MonoBehaviour
{
    // [SerializeField]
    // public string dialogName = null;
    // add "Hello" as explanation inspect text on the inspector for dialogName string field


    [SerializeField]
    public DialogNodeGraph dialogGraph;

    void Start()
    {
        // if (dialogName == null)
        // {
        //     Debug.LogError("Dialog uid is null");
        //     // dialogGraph = null;
        //     return;
        // }
        // //find the dialog graph from Resources/Dialogues folder
        // dialogGraph = Resources.Load<DialogNodeGraph>("Dialogues/" + dialogName);
    }

    public DialogNodeGraph getDialogGraph()
    {
        if (dialogGraph == null)
        {
            Debug.LogError("Dialog graph is null");
            return null;
        }

        return dialogGraph;
    }
}

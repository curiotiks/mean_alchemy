using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cherrydev;
using UnityEngine.UI;

public class Dialogue_Manager : MonoBehaviour
{
    public static Dialogue_Manager _instance;
    [SerializeField] private DialogBehaviour dialogBehaviour;
    [SerializeField] public List<string> dialogGraphs_list;


    void Awake()
    {
        _instance = this;
    }

    public void startDialog(string dialogName)
    {
        //find dialogname from dislogGraphs_list. If not found, show error message
        if (!dialogGraphs_list.Contains(dialogName))
        {
            Debug.LogError("Dialog name " + dialogName + " not found in dialogGraphs_list");
            return;
        }
        //find the dialog graph from Resources/Dialogues folder
        DialogNodeGraph dialogGraph = Resources.Load<DialogNodeGraph>("Dialogues/" + dialogName);
        dialogBehaviour.StartDialog(dialogGraph);
    }

    void Start()
    {
        Dialogue_Manager._instance.startDialog("Dialogue_test1");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Btn_num : MonoBehaviour
{
    [HideInInspector]
    public TextMeshProUGUI text_ui;
    public Table_Control_Panel table_control_panel;
    private Button btn;
    void Awake()
    {
        #region set buttons text refering to the gameobject name

        string name = gameObject.name;
        int num = -1;
        try{
            num = int.Parse(name.Split('_')[name.Split('_').Length-1]);
        }catch{
            Debug.Log("parsing error: "+name);
            return;
        }
        text_ui = GetComponentInChildren<TextMeshProUGUI>(true) as TextMeshProUGUI;
        text_ui.enableAutoSizing = true;
        text_ui.text = num+"";

        #endregion

        btn = GetComponent<Button>();
        btn.onClick.AddListener( delegate{buttonHandler(num);} );
        
    } 

    void buttonHandler(int num){
        this.transform.localScale = Vector3.one;
        this.transform.DOScale(Vector3.one * 1.2f, 0.05f).SetLoops(2, LoopType.Yoyo).SetId(GetHashCode());

        //re-check
        if (num < 1)    return;
        table_control_panel.updateInput(num);
    }
 
}

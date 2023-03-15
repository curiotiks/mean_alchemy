using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Table_Plot_Item : MonoBehaviour
{
    private Button btn;
    private Image img;
    public Table_Control_Panel control_panel; 
    private int num_id;
    void Start()
    {
        btn = GetComponent<Button>();
        img = GetComponent<Image>();
        num_id = int.Parse(gameObject.name);

        btn.onClick.AddListener( delegate{clickListener(num_id);} );
    }

    public void clickListener(int num){
        control_panel.updateInput(num, false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Table_Plot_Item : MonoBehaviour
{
    private Button btn;
    private Image img;
    private TextMeshProUGUI text;
    public Table_Control_Panel control_panel; 
    public int stacked_count = 0;
    void Start()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        btn = GetComponent<Button>();
        img = GetComponent<Image>();
        int num_id = int.Parse(gameObject.name);

        btn.onClick.AddListener( delegate{clickListener(num_id);} );
    }

    public void clickListener(int num){
        control_panel.updateInput(num, false); 
    }

    public void incrementStackedCount(){
        stacked_count++;
        text.text = stacked_count.ToString();
    }

    public void decrementStackedCount(){
        stacked_count--;
        text.text = stacked_count.ToString();
    }
}

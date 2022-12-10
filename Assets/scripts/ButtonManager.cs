using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ButtonManager : MonoBehaviour
{
    public List<Button> numButtons;
    public statsTable stats_table;

    void Start()
    {  
        for (int index=0; index < numButtons.Count; index++)
        {   
            int buttonValue = int.Parse(numButtons[index].name); // Uses object's name to assign value

            numButtons[index].onClick.AddListener( ()=>{
                stats_table.addValue(buttonValue);
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

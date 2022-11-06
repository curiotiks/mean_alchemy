using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ButtonManager : MonoBehaviour
{
    public List<Button> buttons;
    public Button x0, x1, x2, x3, x4, x5, x6, x7, x8, x9;
    public statsTable stats_table;

    void Start()
    {  
        x0.onClick.AddListener( ()=>{
            Debug.Log("hahaha");
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

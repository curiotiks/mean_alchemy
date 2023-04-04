using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 
using System;
using UnityEngine.UI;
using TMPro;

public class Table_Control_Panel : MonoBehaviour
{
    public static Table_Control_Panel instance;
    public TextMeshProUGUI mean_text; 
    public TextMeshProUGUI sd_text;
    public Table_Plot_Panel plot_panel; 
    [HideInInspector]
    public List<int> numbers_list {get; set;}
    public double mean;
    public double sd;
    

    void Awake() {
        numbers_list = new List<int>();
        instance = this;
    }
    
    public void updateInput(int num, bool isAdded = true){
        if (isAdded){
            //if the numbers_list contains the certain number more than 10 elements, return
            if(numbers_list.Count(x => x == num) >= plot_panel.max_stacked_count){
                Debug.Log("numbers_list contains "+num +"more than "+ plot_panel.max_stacked_count +" elements");
                return;
            }


            Debug.Log("new input: "+num);
            numbers_list.Add( num );
        }else{
            //cases when numbers are excluded by clicking bar items on table_plot_panel
            if(numbers_list.Contains(num)){
                numbers_list.Remove(num);
            }
        }
        
        Debug.Log( string.Join('_', numbers_list.ToArray()) );

        try{ 
            mean = numbers_list.Average(); 
            sd = standardDeviation(numbers_list);
        }catch(InvalidOperationException e){
            mean = 0;
            sd = 0;
        } 

        Debug.Log("new mean: "+mean);
        Debug.Log("new sd: "+sd);
        mean_text.text = "Mean: "+ Math.Round(mean, 2);
        sd_text.text = "SD: "+ Math.Round(sd, 2);
        plot_panel.drawPlot(num, isAdded);
    }

    public double standardDeviation(IEnumerable<int> values)
    {
        double avg = values.Average();
        return Math.Sqrt(values.Average(v=>Math.Pow( (double)(v)-avg,2)));
    }

    public void resetNumbers(){
        numbers_list.Clear();
        mean = 0;
        sd = 0;
        plot_panel.resetPlot();
    }
}

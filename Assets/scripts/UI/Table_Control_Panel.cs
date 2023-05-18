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
    public GameObject meterArrow;
    public float rotationRange = 180f; // The maximum rotation angle for the object (in degrees) 
    [HideInInspector]
    public List<int> numbers_list {get; set;}
    public float mean;
    public float sd;
    public float skew;
    

    void Awake() {
        numbers_list = new List<int>();
        instance = this;
    }

    void Update() {
        UpdateMeter();
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
            mean = (float)numbers_list.Average(); 
            sd = (float)standardDeviation(numbers_list);
            skew = (float)CalculateSkewnessCoefficient();
            UpdateMeter();
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

    // Adding method to calculate skewness coefficient
    private double CalculateSkewnessCoefficient()
    {
        float sumCubedDeviations = 0f;
        foreach (float number in numbers_list)
        {
            float deviation = number - mean;
            sumCubedDeviations += deviation * deviation * deviation;
        }
        float n = numbers_list.Count;
        float numerator = (n / ((n - 1) * (n - 2))) * sumCubedDeviations;
        float denominator = (float)System.Math.Pow(sd, 3);
        return numerator / denominator;
    }

    private void UpdateMeter()
    {
        float rotationAngle = skew * rotationRange;
        Quaternion targetRotation = Quaternion.Euler(0f, rotationAngle, 0f);
        meterArrow.transform.Rotate(0f, 0f, rotationAngle);
    }

    public void resetNumbers(){
        numbers_list.Clear();
        mean = 0;
        sd = 0;
        skew = 0;
        plot_panel.resetPlot();
    }
}

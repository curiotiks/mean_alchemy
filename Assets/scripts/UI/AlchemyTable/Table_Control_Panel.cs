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
    public TextMeshProUGUI skew_text;
    public TextMeshProUGUI sd_text;
    public Table_Plot_Panel plot_panel;
    public GameObject meterArrow;
    public float rotationRange = 180f,rotationFactor = -30f; // The maximum rotation angle for the object (in degrees) 
    [SerializeField]
    float rotationAngle = 0f;
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
    
    public int updateInput(int num, bool isAdded = true){
        if (isAdded){
            //if the numbers_list contains the certain number more than 10 elements, return
            if(numbers_list.Count(x => x == num) >= plot_panel.max_stacked_count){
                Debug.Log("numbers_list contains "+num +"more than "+ plot_panel.max_stacked_count +" elements");
                // GET THE COUNT OF STONES IN THE LIST
                return numbers_list.Count(x => x == num);
            }


            Debug.Log("new input: "+num);
                numbers_list.Add( num );
        }
        else
        {
            //cases when numbers are excluded by clicking bar items on table_plot_panel
            if (numbers_list.Contains(num))
            {
                numbers_list.Remove(num);
            }
            Btn_num btnNum = Table_Elements_Panel.instance.GetButtonComponent(num);
            btnNum.UpdateItemText((btnNum.elementButtonCount-1));
        }

        Debug.Log( string.Join('_', numbers_list.ToArray()) );

        try{ 
            mean = (float)numbers_list.Average(); 
            sd = (float)standardDeviation(numbers_list);
            skew = (float)CalculateSkewnessCoefficient();
            UpdateMeter();
        }catch(InvalidOperationException){
            mean = 0;
            sd = 0;
        }
#if UNITY_EDITOR
        Debug.Log("new mean: "+mean);
        Debug.Log("new sd: "+sd);
#endif
        mean_text.text = "Mean: "+ Math.Round(mean, 2);
        sd_text.text = "SD: "+ Math.Round(sd, 2);
        plot_panel.drawPlot(num, isAdded);
        // GET THE COUNT OF STONES IN THE LIST
        return numbers_list.Count(x => x == num);
    }

    public void RemoveAllElements(int num)
    {
        Debug.LogWarning($"{numbers_list.Count(x => x == num)} times for {num}");
        //for (int i = 0; i < numbers_list.Count(x => x == num); i++)
        //{
        //   // Debug.LogWarning($"i : {i}, num : {num}");
        //    numbers_list.Remove(num);
        //    Btn_num btnNum = Table_Elements_Panel.instance.GetButtonComponent(num);
        //    btnNum.UpdateItemText((btnNum.elementButtonCount - 1));
        //}

        for (int i = numbers_list.Count - 1; i >= 0; i--)
        {
            if (numbers_list[i] == num)
            {
                numbers_list.Remove(numbers_list[i]);
                Btn_num btnNum = Table_Elements_Panel.instance.GetButtonComponent(num);
                btnNum.UpdateItemText(btnNum.elementButtonCount - 1);
                plot_panel.drawPlot(num, false);
            }
        }   

        try
        {
            mean = (float)numbers_list.Average();
            sd = (float)standardDeviation(numbers_list);
            skew = (float)CalculateSkewnessCoefficient();
            UpdateMeter();
        }
        catch (InvalidOperationException)
        {
            mean = 0;
            sd = 0;
        }
#if UNITY_EDITOR
        Debug.Log("new mean: " + mean);
        Debug.Log("new sd: " + sd);
#endif
        mean_text.text = "Mean: " + Math.Round(mean, 2);
        sd_text.text = "SD: " + Math.Round(sd, 2);
        plot_panel.drawPlot(num, false  );
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

    //private void UpdateMeter()
    //{
    //    float rotationAngle = skew * rotationRange;
    //    Quaternion targetRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    //    meterArrow.transform.Rotate(0f, 0f, rotationAngle);
    //}
    private void UpdateMeter()
    {

        skew = Mathf.Clamp((Mathf.Round(skew *100f)*.01f), -3f, 3f);
        rotationAngle = skew* rotationFactor;
        skew_text.SetText(skew.ToString());
        if (float.IsNaN(rotationAngle) || float.IsInfinity(rotationAngle))
        {
            //Debug.LogError("Invalid rotation angle detected.");
            return;
        }
        Quaternion targetRotation = Quaternion.Euler(0, 0,rotationAngle);
        meterArrow.transform.rotation = targetRotation;
    }

    public void resetNumbers(){
        numbers_list.Clear();
        mean = 0;
        sd = 0;
        skew = 0;
        plot_panel.resetPlot();
      //  UpdateMeter();
    }
}

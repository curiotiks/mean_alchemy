using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class statsTable : MonoBehaviour{

    public List<int> mixingTable = new List<int>();
    public float meanValue;
    
    [SerializeField]
    private int _n = 0;
    [SerializeField]
    private int _sum = 0;

    public void addValue(int @value)
    {
        mixingTable.Add(@value);
        updateValues();
    }

    public void updateValues()
    {
         _n = mixingTable.Count;
        _sum = mixingTable.Sum();
        meanValue = _sum / _n;
    }
        
}
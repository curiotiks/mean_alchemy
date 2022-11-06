using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class displayController : MonoBehaviour
{
    public GameObject statsTable; 
    private statsTable stats;
    [SerializeField] private TextMeshProUGUI meanDisplay;
    
    // Start is called before the first frame update
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        stats = statsTable.GetComponent<statsTable>();
        meanDisplay.text = stats.meanValue.ToString();
        // Debug.Log(stats.meanValue);
        
    }
}

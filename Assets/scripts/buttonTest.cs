using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class buttonTest : MonoBehaviour
{
    UnityEvent m_MyEvent;
    public int button_value;
    public GameObject stats_table;

    void Start()
    {
        if (m_MyEvent == null)
            m_MyEvent = new UnityEvent();   
        
        m_MyEvent.AddListener(Ping);
    }

    void Update()
    {
        if (Input.anyKeyDown && m_MyEvent != null)
        {
            m_MyEvent.Invoke();
        }
    }

    // Using a callback function. Whatever value (int) to be returned is added to button_value.
    // Place the function you want to activate in the Ping() function below.
    
    void Ping()
    {
        stats_table.GetComponent<statsTable>().addValue(button_value);
    }
}
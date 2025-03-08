using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

public class Table_Elements_Panel : MonoBehaviour
{
    public static Table_Elements_Panel instance;

    #region Instpector

    [SerializeField] private List<Button> uniqueElements;
    public List<Button> UniqueElements
    {
        get { return uniqueElements; }
        private set { uniqueElements = value; }
    }

    [SerializeField] private List<Button> deleteButtons;
    public List<Button> DeleteButtons
{
        get { return deleteButtons; }   
        private set { deleteButtons = value; }  
    }
    #endregion

    private void Awake()
    {
        instance = this;    
    }

    private void Start()
    {
        InitializeData();
    }

    public void InitializeData()
    {
        UniqueElements = new List<Button>();
        UniqueElements = GetComponentsInChildren<Button>().ToList();
        foreach (Button b in UniqueElements)
        {
            if (!b.GetComponent<Btn_num>())
            {
                UniqueElements.Remove(b);
            }
        }
    }

    //Return btn component based on button number
    public Btn_num GetButtonComponent(int buttonNumber)
    {
        Btn_num foundBtn = null;
        foreach (Button b in UniqueElements)
        {
            string btnName = b.gameObject.name.Split('_')[2];
            if (int.Parse(btnName) == buttonNumber)
            {
                foundBtn = b.GetComponent<Btn_num>();
                break;
            }
        }
        //RETURN THE FOUND BUTTON OR NULL
        return foundBtn;
    }
}

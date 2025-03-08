using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeleteBtn_num : MonoBehaviour
{
    [SerializeField] Button btn;
    [SerializeField] int buttonNumber;

    private void Start()
    {
        buttonNumber = int.Parse(gameObject.name.Split('_')[2]);

        btn = GetComponent<Button>();
        if (btn)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(
                () => Table_Control_Panel.instance.RemoveAllElements(buttonNumber)
                );
        }
    }


}

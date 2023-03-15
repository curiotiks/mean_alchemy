using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Intro : MonoBehaviour
{
    public Button btn_login, btn_tutorial, btn_trainingGrounds;
    public CanvasGroup panel_loginManager_cg;
    // Start is called before the first frame update
    void Start()
    {
        btn_login.onClick.AddListener( ()=>{
            Utils.showTargetCanvasGroup(panel_loginManager_cg, true);
        });      
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UnityEngine.Events;

public class PressedBtn : MonoBehaviour , IPointerDownHandler  
{

    Button btn; 
    Transform myIcon;
    public CanvasGroup targetCG_tobeDisappear;
    public bool isTargetCGDisabled;
    public bool isMaskingEffectDisabledAfterFinished; 
    public UnityAction basicButtonAction;

    void Start()
    {
        btn = GetComponent<Button>(); 
        basicButtonAction = addBasicButtonAction;
        
        // Debug.Log(gameManager);

        // if(transform.childCount>0)
        //     myIcon = transform.GetChild(0);
        
        // if(transform.childCount == 0)
        myIcon = transform;
    }

    public void addBasicButtonAction(){
        if(DOTween.IsTweening(GetHashCode()))
            return;
            
        myIcon.localScale = Vector3.one;
        myIcon.DOScale(Vector3.one * 1.2f, 0.1f).SetLoops(2, LoopType.Yoyo).SetId(GetHashCode());
        // GameManager.getInstance().audioManager.playSound(SoundType.click); 
    }
 

    public void OnClick () {

        if(myIcon!=null)
        {
            basicButtonAction.Invoke();

            if(isMaskingEffectDisabledAfterFinished)
                // GameManager.getInstance().uI_Controller.mainPanelManager.setMaskedEffectForTR(null, false);

            if(isTargetCGDisabled)
                Utils.showTargetCanvasGroup(targetCG_tobeDisappear, false, 0.3f, ()=>{
                    targetCG_tobeDisappear.gameObject.SetActive(false);
                });
            else
                Utils.showTargetCanvasGroup(targetCG_tobeDisappear, false);
        }
	}

    public void OnPressed () {
        if (myIcon != null)
            myIcon.localScale = Vector3.one * 1.1f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // OnPressed();
        OnClick();
    } 
}

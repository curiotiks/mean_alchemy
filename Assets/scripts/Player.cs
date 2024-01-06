using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject onCollidingTargetObject;

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject go = other.gameObject;
        Debug.Log("OnTriggerEnter2D: " + go.name);
        //check if go contains Dialog_instance
        if (go.GetComponent<Dialog_instance>() != null)
        {
            //do nothing
        }
        else
        {
            Debug.Log("OnTriggerEnter2D: " + go.name);
            onCollidingTargetObject = other.gameObject;
            Dialog_instance di = go.GetComponent<Dialog_instance>();
            Dialogue_Manager._instance.startDialog(di.getDialogGraph());
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        GameObject go = other.gameObject;
        Debug.Log("OnCollisionEnter2D: " + go.name);
        //check if go contains Dialog_instance
        if (go.GetComponent<Dialog_instance>() == null)
        {
            //do nothing
        }
        else
        {
            Debug.Log("OnCollisionEnter: " + go.name);
            onCollidingTargetObject = other.gameObject;
            Dialog_instance di = go.GetComponent<Dialog_instance>();
            Dialogue_Manager._instance.startDialog(di.getDialogGraph());
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        onCollidingTargetObject = null;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        onCollidingTargetObject = null;
    }
}

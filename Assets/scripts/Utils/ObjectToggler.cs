using UnityEngine;

public class ObjectToggler : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private bool toggle = true; // if false, always turns ON

    public void Toggle()
    {
        if (target == null) return;

        if (toggle)
            target.SetActive(!target.activeSelf);
        else
            target.SetActive(true);
    }
}
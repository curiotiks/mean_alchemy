
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Button_Global : MonoBehaviour
{
    [SerializeField] private string destScene;
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClick);
    }
    private void OnButtonClick()
    {
        BountyBoardManager.instance.HardLoadScene(destScene);
    }
    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnButtonClick);
    }
}

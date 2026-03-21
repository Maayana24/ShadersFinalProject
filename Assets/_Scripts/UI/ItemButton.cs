using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    [SerializeField] private Item item;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Button button;

    public Item Item => item;

    void Awake()
    {
        button.onClick.AddListener(() => uiManager.OnItemButtonClicked(this));
    }
}
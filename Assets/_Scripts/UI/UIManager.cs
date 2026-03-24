using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RawImage[] images;
    [SerializeField] private ImagePool pool;

    [SerializeField] private Camera sheepCamera;
    [SerializeField] private LayerMask sheepLayer;
    [SerializeField] private Image cursor;

    private Item currentItem;
    private ItemButton currentButton;

    public bool IsHoldingItem => currentItem != null;

    private void Awake()
    {
        cursor.gameObject.SetActive(false);
    }

    private void Start()
    {
        ChangeImage();
    }

    private void Update()
    {
        if (!IsHoldingItem) return;

        cursor.rectTransform.position = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.isPressed && !EventSystem.current.IsPointerOverGameObject())
            TryApplyEffect();
    }

    [ContextMenu("ChangeImage")]
    public void ChangeImage()
    {
        int index = Random.Range(0, pool.Images.Count);
        images[0].texture = pool.Images[index].Right;
        images[1].texture = pool.Images[index].Left;
    }

    public void OnItemButtonClicked(ItemButton button)
    {
        if (currentButton == button)
        {
            DropItem();
            return;
        }

        DropItem();
        PickUpItem(button);
    }

    private void PickUpItem(ItemButton button)
    {
        currentItem = button.Item;
        currentButton = button;

        cursor.sprite = currentItem.Cursor;
        cursor.color = currentItem is ColorSpray spray ? spray.Color : Color.white;
        cursor.gameObject.SetActive(true);
    }

    private void DropItem()
    {
        cursor.gameObject.SetActive(false);
        currentItem = null;
        currentButton = null;
    }

    private void TryApplyEffect()
    {
        Ray ray = sheepCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, sheepLayer))
            currentItem.ApplyEffect(hit.point, hit.collider.gameObject);
    }
}
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RawImage[] images;
    [SerializeField] private ImagePool pool;
    [SerializeField] private Camera sheepCamera;
    [SerializeField] private LayerMask sheepLayer;

    private Item currentItem;
    private ItemButton currentButton;
    private GameObject draggableInstance;

    public bool IsHoldingItem => currentItem != null;

    [ContextMenu("ChangeImage")]
    public void ChangeImage()
    {
        int index = Random.Range(0, pool.Images.Count);
        images[0].texture = pool.Images[index].Right;
        images[1].texture = pool.Images[index].Left;
    }

    void Update()
    {
        if (!IsHoldingItem) return;

        Vector3 mouseScreen = new Vector3(Input.mousePosition.x, Input.mousePosition.y, currentItem.DistanceFromCamera);
        Vector3 worldPoint = sheepCamera.ScreenToWorldPoint(mouseScreen);
        draggableInstance.transform.position = worldPoint;

        if (Input.GetMouseButtonDown(0))
            TryApplyEffect();
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

        draggableInstance = Instantiate(currentItem.CursorPrefab);
        draggableInstance.transform.SetParent(sheepCamera.transform);
    }

    private void DropItem()
    {
        if (draggableInstance != null)
        {
            Destroy(draggableInstance);
            draggableInstance = null;
        }

        currentItem = null;
        currentButton = null;
    }

    private void TryApplyEffect()
    {
        Ray ray = sheepCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, sheepLayer))
            currentItem.ApplyEffect(hit.point, hit.collider.gameObject);
    }
}
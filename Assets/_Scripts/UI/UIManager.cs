using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RawImage[] images;
    [SerializeField] private ImagePool pool;

    [SerializeField] private Camera sheepCamera;
    [SerializeField] private LayerMask sheepLayer;
    [SerializeField] private Image cursor;
    [SerializeField] private TextMeshProUGUI difficultyText;

    private Item currentItem;
    private ItemButton currentButton;
    private bool isHighDifficulty = false;

    private bool IsHoldingItem => currentItem != null;

    public static event System.Action<bool> OnDifficultyChange;

    private void Awake()
    {
        cursor.gameObject.SetActive(false);
    }

    private void Start()
    {
        ChangeImage();
        UIScoreManager.OnRestart += OnRestart;
    }

    private void Update()
    {
        if (!IsHoldingItem) return;

        cursor.rectTransform.position = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.isPressed && !EventSystem.current.IsPointerOverGameObject())
            TryApplyEffect();
    }

    private void OnRestart()
    {
        DropItem();
        ChangeImage();
    }

    public void OnChangeDifficulty()
    {
        isHighDifficulty = !isHighDifficulty;
        difficultyText.text = isHighDifficulty ? "Difficulty: High" : "Difficulty: Low";
        OnDifficultyChange?.Invoke(isHighDifficulty);
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

    private void OnDestroy()
    {
        UIScoreManager.OnRestart -= OnRestart;
    }
}
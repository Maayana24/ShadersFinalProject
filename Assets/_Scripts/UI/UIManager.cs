using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RawImage[] images;
    [SerializeField] private ImagePool pool;
    [SerializeField] private ParticleController particleController;

    [SerializeField] private Camera sheepCamera;
    [SerializeField] private LayerMask sheepLayer;
    [SerializeField] private Image cursor;
    [SerializeField] private TextMeshProUGUI difficultyText;

    private Item currentItem;
    private ItemButton currentButton;
    private bool isHighDifficulty = false;
    private bool isPlayingParticles = false;

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

        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Mouse.current.leftButton.isPressed)
        {
            bool hitSheep = TryApplyEffect(out Vector3 hit);

            if (hitSheep)
                particleController.SetPosition(hit);

            if (Mouse.current.leftButton.wasPressedThisFrame && hitSheep)
            {
                particleController.Play();
                isPlayingParticles = true;
            }
            else if (!hitSheep && isPlayingParticles)
            {
                particleController.Stop();
                isPlayingParticles = false;
            }
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame && isPlayingParticles)
        {
            particleController.Stop();
            isPlayingParticles = false;
        }
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
        if(currentItem is ColorSpray spray)
        {
            cursor.color = spray.Color;
            particleController.SetCanColor(spray.Color);
        }
        else
        {
            cursor.color = Color.white;
        }
        cursor.gameObject.SetActive(true);
        particleController.SetTool(currentItem.Type);
    }

    private void DropItem()
    {
        cursor.gameObject.SetActive(false);
        currentItem = null;
        currentButton = null;
    }

    private bool TryApplyEffect(out Vector3 hitPoint)
    {
        Ray ray = sheepCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, sheepLayer))
        {
            hitPoint = hit.point;
            currentItem.ApplyEffect(hit.point, hit.collider.gameObject);
            return true;
        }
        hitPoint = Vector3.zero;
        return false;
    }

    private void OnDestroy()
    {
        UIScoreManager.OnRestart -= OnRestart;
    }
}
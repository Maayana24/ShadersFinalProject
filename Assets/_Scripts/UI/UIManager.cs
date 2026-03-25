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

    // Gizmo state
    private Vector3 _gizmoHitPoint;
    private Vector3 _gizmoHitNormal;
    private Ray _gizmoRay;
    private bool _gizmoHasHit;

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
        if (currentItem is ColorSpray spray)
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
        _gizmoRay = ray;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, sheepLayer))
        {
            hitPoint = hit.point;
            Vector2 uv = hit.textureCoord;
            Debug.Log($"[UIManager] Hit: worldPos={hitPoint}, uv={uv}");
            currentItem.ApplyEffect(hit.point, uv, hit.collider.gameObject);

            _gizmoHitPoint = hit.point;
            _gizmoHitNormal = hit.normal;
            _gizmoHasHit = true;
            return true;
        }
        hitPoint = Vector3.zero;
        _gizmoHasHit = false;
        return false;
    }

    private void OnDrawGizmos()
    {
        // Ray from camera
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(_gizmoRay.origin, _gizmoRay.direction * 10f);

        if (!_gizmoHasHit) return;

        // Hit point
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_gizmoHitPoint, 0.02f);

        // Normal at hit point (proxy collider surface)
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(_gizmoHitPoint, _gizmoHitNormal * 0.1f);

        // Ray from hit point back toward original mesh (along -normal)
        Gizmos.color = Color.green;
        Gizmos.DrawRay(_gizmoHitPoint, -_gizmoHitNormal * 0.1f);
    }

    private void OnDestroy()
    {
        UIScoreManager.OnRestart -= OnRestart;
    }
}
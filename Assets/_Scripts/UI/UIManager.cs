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
    [SerializeField][Range(0.01f, 1f)]  private float brushRadius   = 0.15f;
    [SerializeField][Range(0.01f, 10f)] private float brushStrength = 2f;

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
    public static event System.Action<ImageReference> OnImageChanged;

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
        if (!IsHoldingItem)
        {
            Shader.SetGlobalFloat("_BrushActive", 0f);
            return;
        }

        cursor.rectTransform.position = Mouse.current.position.ReadValue();

        if (EventSystem.current.IsPointerOverGameObject())
        {
            Shader.SetGlobalFloat("_BrushActive", 0f);
            return;
        }

        Ray ray = sheepCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        _gizmoRay = ray;

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, sheepLayer))
        {
            Vector2 uv = hit.textureCoord;
            SheepWoolManager woolManager = hit.collider.GetComponentInParent<SheepWoolManager>();

            _gizmoHitPoint = hit.point;
            _gizmoHitNormal = hit.normal;
            _gizmoHasHit = true;

            if (woolManager != null)
            {
                Shader.SetGlobalVector("_BrushUV", new Vector4(uv.x, uv.y, 0f, 0f));
                Shader.SetGlobalFloat("_BrushRadius", woolManager.GetUVRadius(brushRadius));
                Shader.SetGlobalFloat("_BrushActive", 1f);
            }

            particleController.SetPosition(hit.point);

            if (Mouse.current.leftButton.isPressed)
            {
                if (woolManager == null)
                    Debug.LogWarning($"[UIManager] No SheepWoolManager on {hit.collider.gameObject.name} or its parents");
                else
                {
                    Debug.Log($"[UIManager] Hit: worldPos={hit.point}, uv={uv}");
                    currentItem.ApplyEffect(hit.point, uv, woolManager, brushRadius, brushStrength);
                }

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    particleController.Play();
                    isPlayingParticles = true;
                }
            }
        }
        else
        {
            Shader.SetGlobalFloat("_BrushActive", 0f);
            _gizmoHasHit = false;

            if (isPlayingParticles)
            {
                particleController.Stop();
                isPlayingParticles = false;
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isPlayingParticles)
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
        if (pool.Images.Count == 0) return;
        int index = Random.Range(0, pool.Images.Count);
        ImageReference reference = pool.Images[index];
        images[0].texture = reference.Right;
        images[1].texture = reference.Left;
        OnImageChanged?.Invoke(reference);
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
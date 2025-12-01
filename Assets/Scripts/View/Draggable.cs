using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Model.Map;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Draggable;

// Attach this script to draggable UI elements (animal buttons).
public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Prefab to spawn when dragging ends successfully
    public GameObject prefabToSpawn;

    // Reference to the canvas for UI coordinate conversion
    public Canvas canvas;

    // UI ghost object for visual feedback while dragging
    private GameObject ghostObject;
    private RectTransform ghostRectTransform;

    // Cached reference to the original UI image (for ghost sprite)
    private Image originalImage;

    // Reference to external managers
    public gameUIButtonsManager buttonsManager;
    public SafariMap map;

    // The type of animal represented by this draggable
    private AnimalType animalType;

    // Animal prefabs (assigned via Inspector)
    public GameObject foxPrefab;
    [SerializeField] public GameObject lionPrefab;
    [SerializeField] public GameObject giraffePrefab;
    [SerializeField] public GameObject zebraPrefab;

    // Enum representing the animal types
    public enum AnimalType
    {
        Fox,
        Lion,
        Giraffe,
        Zebra
    }

    private void Start()
    {
        originalImage = GetComponent<Image>();

        // Determine the animal type based on the assigned prefab
        if (prefabToSpawn == foxPrefab)
            animalType = AnimalType.Fox;
        else if (prefabToSpawn == lionPrefab)
            animalType = AnimalType.Lion;
        else if (prefabToSpawn == giraffePrefab)
            animalType = AnimalType.Giraffe;
        else if (prefabToSpawn == zebraPrefab)
            animalType = AnimalType.Zebra;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Prevent dragging if not in ANIMALS shop tab
        if (buttonsManager.CurrentShopType != ShopTypes.ANIMALS) return;

        // Check if the player can afford this animal
        int animalPrice = GameManager.Instance.GetAnimalPrice(animalType);
        if (GameManager.Instance.Money < animalPrice)
        {
            return; // Don't start dragging
        }

        // Track number of animals by type
        switch (animalType)
        {
            case AnimalType.Fox:
            case AnimalType.Lion:
                GameManager.Instance.CurrentCarnivorousCount++;
                break;
            case AnimalType.Giraffe:
            case AnimalType.Zebra:
                GameManager.Instance.CurrentHerbivoresCount++;
                break;
        }

        // Create and configure the semi-transparent ghost image
        ghostObject = new GameObject("Ghost", typeof(Image));
        ghostObject.transform.SetParent(canvas.transform, false);

        Image ghostImage = ghostObject.GetComponent<Image>();
        ghostImage.sprite = originalImage.sprite;
        ghostImage.SetNativeSize();
        ghostImage.color = new Color(1, 1, 1, 0.4f); // semi-transparent white

        ghostRectTransform = ghostObject.GetComponent<RectTransform>();

        UpdateGhostPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log(buttonsManager.CurrentShopType);
        UpdateGhostPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Destroy(ghostObject);

        if (buttonsManager.CurrentShopType != ShopTypes.ANIMALS) return;

        Vector3 worldPos = GetWorldPosition(eventData.position);

        // Spawn the actual animal in the world
        GameObject animal = Instantiate(prefabToSpawn, worldPos, Quaternion.identity);

        // Adjust spawn position to align bottom of sprite with ground
        SpriteRenderer spriteRenderer = animal.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            float offsetY = spriteRenderer.bounds.extents.y;
            animal.transform.position -= new Vector3(0f, offsetY, 0f);
        }

        // Validate tile type before finalizing placement
        Tile tile = map.GetTileAt(new Vector2(animal.transform.position.x, animal.transform.position.y));
        if (tile.Type == Tile.ShopType.Lake || tile.Type == Tile.ShopType.River)
        {
            Destroy(animal); // Invalid placement
        }
        else
        {
            int animalPrice = GameManager.Instance.GetAnimalPrice(animalType);
            GameManager.Instance.Money -= animalPrice;
        }
    }

    // Update the position of the ghost image to follow the pointer
    private void UpdateGhostPosition(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            null,
            out localPoint))
        {
            if (ghostRectTransform != null)
                ghostRectTransform.anchoredPosition = localPoint;
        }
    }

    private void Update()
    {
        // Dynamically scale the ghost based on camera zoom (for better UX)
        if (ghostRectTransform != null)
        {
            float zoom = Camera.main.orthographicSize;
            float scale = Mathf.Clamp(1f / zoom, 0.3f, 2f);
            ghostRectTransform.localScale = Vector3.one * scale;
        }
    }

    // Convert screen coordinates to world position using raycasting
    private Vector3 GetWorldPosition(Vector2 screenPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null)
        {
            return hit.point;
        }

        return Vector3.zero;
    }
}



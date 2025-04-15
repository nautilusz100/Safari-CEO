using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject prefabToSpawn;
    public Canvas canvas;

    private GameObject ghostObject;
    private RectTransform ghostRectTransform;
    private Image originalImage;
    public gameUIButtonsManager buttonsManager;
    public SafariMap map;

    private void Start()
    {
        originalImage = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        
        if (buttonsManager.CurrentShopType != ShopTypes.ANIMALS) return;
        // Create ghost
        ghostObject = new GameObject("Ghost", typeof(Image));
        ghostObject.transform.SetParent(canvas.transform, false);

        // Copy sprite and size
        Image ghostImage = ghostObject.GetComponent<Image>();
        ghostImage.sprite = originalImage.sprite;
        ghostImage.SetNativeSize();
        ghostImage.color = new Color(1, 1, 1, 0.4f);

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
        GameObject animal = Instantiate(prefabToSpawn, worldPos, Quaternion.identity);

        SpriteRenderer spriteRenderer = animal.GetComponent<SpriteRenderer>();

        /*Adjusting for the bottom center pivot*/
        if (spriteRenderer != null)
        {
            float offsetY = spriteRenderer.bounds.extents.y;
            animal.transform.position -= new Vector3(0f, offsetY, 0f);
        }
        /*Check if placing it in a valid position*/
        Tile tile = map.GetTileAt(new Vector2(animal.transform.position.x, animal.transform.position.y));
        if (tile.Type == Tile.ShopType.Lake || tile.Type == Tile.ShopType.River)
        {
            Destroy(animal);
        }

    }

    private void UpdateGhostPosition(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            null,
            out localPoint))
        
        {
            ghostRectTransform.anchoredPosition = localPoint;
        }

    }

    void Update()
    {
        if (ghostRectTransform != null)
        {
            float zoom = Camera.main.orthographicSize;
            float scale = Mathf.Clamp(1f / zoom, 0.3f, 2f);

            ghostRectTransform.localScale = Vector3.one * scale;
        }
    }

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


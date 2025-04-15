using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public enum ShopType
    { 
        Tree,
        Bush,
        Flowerbed,
        Hills,
        River,
        Lake,
        Road,
        MainBuilding,
        Plains,
        Animal,
        Jeep,
        None
    }

    [SerializeField] private ShopType initialType = ShopType.Plains;
    public ShopType Type { get; set; }

    void Awake()
    {
        Type = initialType;
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10 + spriteRenderer.sortingOrder);
    }
}

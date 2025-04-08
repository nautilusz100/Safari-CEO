using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public enum TileType
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
        None
    }

    [SerializeField] private TileType initialType = TileType.Plains;
    public TileType Type { get; set; }

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

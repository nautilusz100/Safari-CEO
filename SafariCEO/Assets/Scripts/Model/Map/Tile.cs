using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public enum TileType
    {
        Plains,
        Tree,
        Hills,
        River,
        Lake,
        Bush,
        Flowerbed,
        Road
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

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


    [SerializeField] private int FoodAmount;

    void Awake()
    {
        Type = initialType;
        switch (Type)
        {
            case ShopType.Tree:
                FoodAmount = 10;
                break;
            case ShopType.Bush:
                FoodAmount = 6;
                break;
            case ShopType.Flowerbed:
                FoodAmount = 3;
                break;
            default:
                FoodAmount = 0;
                break;
        }
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10 + spriteRenderer.sortingOrder);
    }

    //anaimal eating
    public void ConsumeFood(int amount)
    {
        if (FoodAmount <= 0) return;

        FoodAmount -= amount;

        if (FoodAmount <= 0)
        {
            BecomePlains();
        }
        Debug.Log($"{gameObject.name} tile lost {amount} food. Remaining: {FoodAmount}");
    }
    private void Update()
    {
        if (FoodAmount <= 0 &&( ShopType.Tree == Type || ShopType.Bush == Type || ShopType.Flowerbed == Type))
        {
            BecomePlains();
        }
    }

    void BecomePlains()
    {
        Debug.Log("Tile food depleted, becoming plains at pos: x: " + this.transform.position.x + this.transform.position.y);
        if (GameManager.Instance != null)
        {
            Vector2Int pos = new Vector2Int((int)transform.position.x, (int)transform.position.y);
            GameManager.Instance.NotifyTileFoodDepleted(pos);
        }

    }
}

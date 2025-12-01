using UnityEngine;

/// <summary>
/// Represents a tile on the map with different terrain or object types, such as trees or hills.
/// Tiles can hold food, change state, and notify the game manager when food is depleted.
/// </summary>
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


        public int FoodAmount { get; set; }
        public bool IsLocked { get; set; }

        void Awake()
        {
            Type = initialType;
        // Initialize food amount based on tile type
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
            IsLocked = false;
        }

        void Start()
        {
            // Set sorting order based on Y position for visual layering
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10);
        }

        /// <summary>
        /// Reduces food amount when consumed by an animal or other entity.
        /// </summary>

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
            // Automatically convert food-producing tiles to plains when depleted
            if (FoodAmount <= 0 && (ShopType.Tree == Type || ShopType.Bush == Type || ShopType.Flowerbed == Type))
            {
                BecomePlains();
            }
        }
        /// <summary>
        /// Notifies the game manager that this tile's food has been depleted and should revert to plains.
        /// </summary>
        void BecomePlains()
        {
            Debug.Log("Tile food depleted, becoming plains at pos: x: " + transform.position.x + transform.position.y);
            if (GameManager.Instance != null)
            {
                Vector2Int pos = new Vector2Int((int)transform.position.x, (int)transform.position.y);
                GameManager.Instance.NotifyTileFoodDepleted(pos);
            }

        }
    }

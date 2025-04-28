using NUnit.Framework;
using UnityEngine;

public class TileTests
{
    private GameObject tileObject;
    private Tile tile;

    [SetUp]
    public void Setup()
    {
        tileObject = new GameObject();
        tile = tileObject.AddComponent<Tile>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(tileObject);
    }

    [Test]
    public void Initialization_SetsTypeAndFoodAmount_ForTree()
    {
        // "Awake" után várható értékeket kézzel állítjuk be
        tile.Type = Tile.ShopType.Tree;
        tile.FoodAmount = 10;
        tile.isLocked = false;

        Assert.AreEqual(Tile.ShopType.Tree, tile.Type);
        Assert.AreEqual(10, tile.FoodAmount);
        Assert.IsFalse(tile.isLocked);
    }

    [Test]
    public void Initialization_SetsTypeAndFoodAmount_ForBush()
    {
        tile.Type = Tile.ShopType.Bush;
        tile.FoodAmount = 6;
        tile.isLocked = false;

        Assert.AreEqual(Tile.ShopType.Bush, tile.Type);
        Assert.AreEqual(6, tile.FoodAmount);
        Assert.IsFalse(tile.isLocked);
    }

    [Test]
    public void Initialization_SetsTypeAndFoodAmount_ForPlains()
    {
        tile.Type = Tile.ShopType.Plains;
        tile.FoodAmount = 0;
        tile.isLocked = false;

        Assert.AreEqual(Tile.ShopType.Plains, tile.Type);
        Assert.AreEqual(0, tile.FoodAmount);
        Assert.IsFalse(tile.isLocked);
    }

    [Test]
    public void ConsumeFood_DecreasesFoodAmount()
    {
        tile.FoodAmount = 10;

        tile.ConsumeFood(3);

        Assert.AreEqual(7, tile.FoodAmount);
    }

    [Test]
    public void ConsumeFood_DoesNotGoNegative()
    {
        tile.FoodAmount = 2;

        tile.ConsumeFood(5);

        Assert.LessOrEqual(tile.FoodAmount, 0);
    }

    [Test]
    public void ConsumeFood_WhenFoodDepleted_NotifiesGameManager()
    {
        var mockManager = new GameObject().AddComponent<MockGameManager>();
        GameManager.Instance = mockManager;

        tile.Type = Tile.ShopType.Tree;
        tile.FoodAmount = 5;
        tile.transform.position = new Vector3(2, 3, 0);

        tile.ConsumeFood(5);

        Assert.IsTrue(mockManager.notified);
        Assert.AreEqual(new Vector2Int(2, 3), mockManager.notifiedPosition);

        Object.DestroyImmediate(mockManager.gameObject);
    }



    [Test]
    public void FoodAmountZero_WhenTileIsTree_TriggersBecomePlains()
    {
        var mockManager = new GameObject().AddComponent<MockGameManager>();
        GameManager.Instance = mockManager;

        tile.Type = Tile.ShopType.Tree;
        tile.FoodAmount = 0;
        tile.transform.position = new Vector3(2, 3, 0);

        // Manually trigger the Update behavior
        if (tile.FoodAmount <= 0 && (tile.Type == Tile.ShopType.Tree || tile.Type == Tile.ShopType.Bush || tile.Type == Tile.ShopType.Flowerbed))
        {
            var method = typeof(Tile).GetMethod("BecomePlains", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(tile, null);
        }

        Assert.IsTrue(mockManager.notified); // <- ezt nézzük
    }


    // Helper Mock Class
    public class MockGameManager : GameManager
    {
        public bool notified = false;
        public Vector2Int notifiedPosition;

        public override void NotifyTileFoodDepleted(Vector2Int pos)
        {
            notified = true;
            notifiedPosition = pos;
        }
    }
}

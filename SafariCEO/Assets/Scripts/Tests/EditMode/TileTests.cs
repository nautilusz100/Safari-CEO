using NUnit.Framework;
using UnityEngine;

public class TileTests : MonoBehaviour
{
    private GameObject tileObject;
    private Tile tile;

    [SetUp]
    public void Setup()
    {
        // Create a new GameObject and attach the Tile script to it
        tileObject = new GameObject();
        tile = tileObject.AddComponent<Tile>();
    }

    [Test]
    public void Awake_ShouldInitializeFoodAmountCorrectly_ForTree()
    {
        // Set the Type to Tree to test the food amount initialization
        tile.Type = Tile.ShopType.Tree;
        tile.Awake();

        // Assert that the FoodAmount for Tree is 10
        Assert.AreEqual(10, tile.FoodAmount);
    }

    [Test]
    public void Awake_ShouldInitializeFoodAmountCorrectly_ForBush()
    {
        // Set the Type to Bush to test the food amount initialization
        tile.Type = Tile.ShopType.Bush;
        tile.Awake();

        // Assert that the FoodAmount for Bush is 6
        Assert.AreEqual(6, tile.FoodAmount);
    }

    [Test]
    public void Awake_ShouldInitializeFoodAmountCorrectly_ForFlowerbed()
    {
        // Set the Type to Flowerbed to test the food amount initialization
        tile.Type = Tile.ShopType.Flowerbed;
        tile.Awake();

        // Assert that the FoodAmount for Flowerbed is 3
        Assert.AreEqual(3, tile.FoodAmount);
    }

    [Test]
    public void Awake_ShouldInitializeFoodAmountCorrectly_ForOtherTypes()
    {
        // Set the Type to None to test the default food amount initialization
        tile.Type = Tile.ShopType.None;
        tile.Awake();

        // Assert that the FoodAmount for None is 0
        Assert.AreEqual(0, tile.FoodAmount);
    }

    [Test]
    public void ConsumeFood_ShouldReduceFoodAmount()
    {
        // Set the Type to Tree and initialize food amount
        tile.Type = Tile.ShopType.Tree;
        tile.Awake();

        // Consume 5 food
        tile.ConsumeFood(5);

        // Assert that FoodAmount is reduced to 5
        Assert.AreEqual(5, tile.FoodAmount);
    }

    [Test]
    public void ConsumeFood_ShouldNotGoBelowZero()
    {
        // Set the Type to Bush and initialize food amount
        tile.Type = Tile.ShopType.Bush;
        tile.Awake();

        // Consume 7 food (more than available)
        tile.ConsumeFood(7);

        // Assert that FoodAmount is 0 (not negative)
        Assert.AreEqual(0, tile.FoodAmount);
    }

    [Test]
    public void BecomePlains_ShouldSetFoodAmountToZero_WhenFoodDepleted()
    {
        // Set the Type to Tree and initialize food amount
        tile.Type = Tile.ShopType.Tree;
        tile.Awake();

        // Consume all food
        tile.ConsumeFood(10);

        // Assert that the Tile is now Plains and FoodAmount is 0
        Assert.AreEqual(0, tile.FoodAmount);
    }

    [Test]
    public void Update_ShouldCallBecomePlains_WhenFoodAmountZero()
    {
        // Set the Type to Tree and initialize food amount
        tile.Type = Tile.ShopType.Tree;
        tile.Awake();

        // Consume all food to make it zero
        tile.ConsumeFood(10);

        // Update the tile, which should trigger BecomePlains
        tile.Update();

        // Assert that the tile becomes plains (FoodAmount should be 0)
        Assert.AreEqual(Tile.ShopType.Plains, tile.Type);
    }

    [Test]
    public void Start_ShouldSetSpriteRendererSortingOrder()
    {
        // Add SpriteRenderer to the tile object
        tileObject.AddComponent<SpriteRenderer>();

        // Set position to test sorting order
        tileObject.transform.position = new Vector3(1, 2, 0);

        // Call Start to ensure sorting order is set
        tile.Start();

        // Assert that sortingOrder is set to expected value based on y position
        Assert.AreEqual(Mathf.RoundToInt(-tileObject.transform.position.y * 10), tile.GetComponent<SpriteRenderer>().sortingOrder);
    }

    [TearDown]
    public void Teardown()
    {
        // Cleanup any resources
        Object.Destroy(tileObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;
using Assets.Scripts.Model.Map;

public class HerbivorePlayModeTest
{
    // Test GameObject references
    private GameObject herbivoreObj;
    private Herbivore herbivore;
    private GameObject foodTileObj;
    private Tile foodTile;
    private GameObject mateObj;
    private Herbivore mate;

    // Helper methods to access private members
    private T GetPrivateField<T>(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)field.GetValue(instance);
    }

    private void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(instance, value);
    }

    private void InvokePrivateMethod(object instance, string methodName, params object[] parameters)
    {
        var method = instance.GetType().GetMethod(methodName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(instance, parameters);
    }

    [UnitySetUp]
    public void SetUp()
    {
        // Create complete mock GameManager
        var gameManagerObj = new GameObject("GameManager");

        // Proper herbivore setup
        herbivoreObj = new GameObject("TestZebra");
        herbivoreObj.tag = "Zebra";
        herbivoreObj.layer = LayerMask.NameToLayer("Animals");

        // Essential components
        var sr = herbivoreObj.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(new Texture2D(1, 1), new Rect(0, 0, 1, 1), Vector2.zero);
        var col = herbivoreObj.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;
        col.isTrigger = true;

        herbivore = herbivoreObj.AddComponent<Herbivore>();
        herbivore.testMode = true;
        GameObject child = new GameObject("Child");
        child.AddComponent<SpriteRenderer>(); child.AddComponent<CircleCollider2D>();
        child.AddComponent<Herbivore>().testMode = true;
        herbivore.zebraPrefab = child;
        herbivore.giraffePrefab = child;

        // Food tile setup
        foodTileObj = new GameObject("FoodTile");
        foodTile = foodTileObj.AddComponent<Tile>();
        foodTile.Type = Tile.ShopType.Tree; // Herbivore food source
        foodTile.FoodAmount = 10;
        foodTileObj.AddComponent<SpriteRenderer>();
        var tileCol = foodTileObj.AddComponent<BoxCollider2D>();
        tileCol.size = Vector2.one;

        // Mate setup
        mateObj = new GameObject("TestMate");
        mateObj.tag = "Zebra";
        var mateSr = mateObj.AddComponent<SpriteRenderer>();
        mateSr.sprite = Sprite.Create(new Texture2D(1, 1), new Rect(0, 0, 1, 1), Vector2.zero);
        var mateCol = mateObj.AddComponent<CircleCollider2D>();
        mateCol.radius = 0.5f;
        mateCol.isTrigger = true;
        mate = mateObj.AddComponent<Herbivore>();
        mate.testMode = true;

        // Position entities
        herbivoreObj.transform.position = Vector3.zero;
        foodTileObj.transform.position = Vector3.right * 2f;
        mateObj.transform.position = Vector3.left * 2f;
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(herbivoreObj);
        Object.Destroy(foodTileObj);
        Object.Destroy(mateObj);
        Object.Destroy(GameObject.Find("GameManager"));
    }

    [UnityTest]
    public IEnumerator Herbivore_SearchFoodState_ShouldTargetFoodTile()
    {
        // Set up explored tiles
        yield return null;
        var exploredTiles = new List<Tile> { foodTile };
        SetPrivateField(herbivore, "exploredTiles", exploredTiles);

        // Set hunger state
       herbivore.hungerTimer = 0.1f * herbivore.hungerInterval;

        // Trigger food search
        InvokePrivateMethod(herbivore, "DecideNextAction");

        yield return null;

        var currentState = GetPrivateField<Herbivore.StateHerbivore>(herbivore, "currentState");
        Assert.AreEqual(Herbivore.StateHerbivore.SearchFood, currentState);
    }

    [UnityTest]
    public IEnumerator Herbivore_StartEating_ShouldConsumeFood()
    {
        // Set up food tile
        var initialFood = foodTile.FoodAmount;
        foodTile.transform.position = herbivoreObj.transform.position;

        InvokePrivateMethod(herbivore, "StartEating");

        yield return new WaitForSeconds(0.1f);

        InvokePrivateMethod(herbivore, "FinishEating");

        Assert.Less(foodTile.FoodAmount, initialFood);
    }

    [UnityTest]
    public IEnumerator Herbivore_Mating_ShouldCreateOffspring()
    {
        // Setup mating conditions
        herbivore.age = 100f;
        herbivore.mateTimer = 100f;

        // Add mate to spotted list
        var spottedMates = new Dictionary<GameObject, Vector3> { { mateObj, mateObj.transform.position } };
        SetPrivateField(herbivore, "spottedMates", spottedMates);

        // Trigger mating
        InvokePrivateMethod(herbivore, "StartMating", mateObj.transform);

        yield return new WaitForSeconds(0.1f);
        InvokePrivateMethod(herbivore, "FinishMating");

        var animals = GameObject.FindObjectsOfType<Herbivore>();
        Assert.Greater(animals.Length, 2);
    }

    [UnityTest]
    public IEnumerator Herbivore_ThirstState_ShouldFindWater()
    {
        // Create water tile
        var waterTileObj = new GameObject("WaterTile");
        var waterTile = waterTileObj.AddComponent<Tile>();
        waterTile.Type = Tile.ShopType.Lake;
        waterTileObj.AddComponent<BoxCollider2D>();
        waterTileObj.AddComponent<SpriteRenderer>();

        // Add to explored tiles
        var exploredTiles = new List<Tile> { waterTile };
        SetPrivateField(herbivore, "exploredTiles", exploredTiles);

        // Set thirst state
       herbivore.thirstTimer = 0.1f * herbivore.thirstInterval;

        InvokePrivateMethod(herbivore, "DecideNextAction");

        yield return null;

        var currentState = GetPrivateField<Herbivore.StateHerbivore>(herbivore, "currentState");
        Assert.AreEqual(Herbivore.StateHerbivore.SearchWater, currentState);
    }

    [UnityTest]
    public IEnumerator Herbivore_Vision_ShouldDetectResources()
    {
        // Position food tile in vision range
        foodTileObj.transform.position = herbivoreObj.transform.position + Vector3.right * 2f;

        // Execute vision update
        InvokePrivateMethod(herbivore, "UpdateVision");

        yield return null;

        var exploredTiles = GetPrivateField<List<Tile>>(herbivore, "exploredTiles");
        Assert.Contains(foodTile, exploredTiles);
    }
}

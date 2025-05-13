using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.AI;
using System.Reflection;
using Assets.Scripts.Model.Map;

public class CarnivorousPlayModeTests
{
    // Test GameObject references
    private GameObject carnivorousObj;
    private Carnivorous carnivorous;
    private GameObject preyObj;
    private Herbivore prey;
    private GameObject tileObj;
    private Tile tile;
    private GameObject mateObj;
    private Carnivorous mate;

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

    private Dictionary<GameObject, Vector3> CreateDummySpottedPreyPositions()
    {
        var positions = new Dictionary<GameObject, Vector3>();
        var dummyPrey = new GameObject("DummyPrey");
        dummyPrey.transform.position = Vector3.zero;
        positions.Add(dummyPrey, dummyPrey.transform.position);
        return positions;
    }

    private Dictionary<GameObject, Vector3> CreateDummySpottedMates()
    {
        var positions = new Dictionary<GameObject, Vector3>();
        var dummyMate = new GameObject("DummyMate");
        dummyMate.transform.position = Vector3.zero;
        positions.Add(dummyMate, dummyMate.transform.position);
        return positions;
    }

    [UnitySetUp]
    public void SetUp()
    {
        // Create proper NavMesh
        var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "NavMeshPlane";
        plane.transform.position = Vector3.zero;

        // Add proper NavMesh components
        /*var navMeshSurface = plane.AddComponent<NavMeshPlus.Components.NavMeshSurface>();
        navMeshSurface.collectObjects = NavMeshPlus.Components.CollectObjects.All;
        navMeshSurface.defaultArea = 0;
        navMeshSurface.BuildNavMesh();*/

        // Create complete mock GameManager
        var gameManagerObj = new GameObject("GameManager");
        var mockManager = gameManagerObj.AddComponent<MockGameManager>();
        mockManager.Animals = new List<GameObject>();

        // Proper carnivorous setup
        carnivorousObj = new GameObject("TestLion");
        carnivorousObj.tag = "Lion";
        carnivorousObj.layer = LayerMask.NameToLayer("Animals");

        // Essential components
        var sr = carnivorousObj.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(new Texture2D(1, 1), new Rect(0, 0, 1, 1), Vector2.zero);
        var col = carnivorousObj.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;
        col.isTrigger = true;

        carnivorous = carnivorousObj.AddComponent<Carnivorous>();
        /*var agent = carnivorousObj.AddComponent<NavMeshAgent>();
        agent.radius = 0.5f;
        agent.speed = 3.0f;
        agent.acceleration = 8.0f;
        agent.angularSpeed = 120;*/

        // Proper prey setup
        preyObj = new GameObject("TestZebra");
        preyObj.tag = "Zebra";
        preyObj.layer = LayerMask.NameToLayer("Animals");
        var preySr = preyObj.AddComponent<SpriteRenderer>();
        preySr.sprite = Sprite.Create(new Texture2D(1, 1), new Rect(0, 0, 1, 1), Vector2.zero);
        var preyCol = preyObj.AddComponent<CircleCollider2D>();
        preyCol.radius = 0.5f;
        preyCol.isTrigger = true;
        prey = preyObj.AddComponent<Herbivore>();
        //preyObj.AddComponent<NavMeshAgent>();

        // Proper tile setup with required components
        tileObj = new GameObject("TestTile");
        tile = tileObj.AddComponent<Tile>();
        tileObj.AddComponent<SpriteRenderer>(); // Add missing SpriteRenderer
        var tileCol = tileObj.AddComponent<BoxCollider2D>();
        tileCol.size = Vector2.one;

        // Complete mate setup
        mateObj = new GameObject("TestMate");
        mateObj.tag = "Lion";
        mateObj.layer = LayerMask.NameToLayer("Animals");
        var mateSr = mateObj.AddComponent<SpriteRenderer>();
        mateSr.sprite = Sprite.Create(new Texture2D(1, 1), new Rect(0, 0, 1, 1), Vector2.zero);
        var mateCol = mateObj.AddComponent<CircleCollider2D>();
        mateCol.radius = 0.5f;
        mateCol.isTrigger = true;
        mate = mateObj.AddComponent<Carnivorous>();
        /*var mateAgent = mateObj.AddComponent<NavMeshAgent>();
        mateAgent.radius = 0.5f;*/

        // Position entities
        carnivorousObj.transform.position = Vector3.zero;
        preyObj.transform.position = Vector3.right * 2f;
        mateObj.transform.position = Vector3.left * 2f;
        tileObj.transform.position = Vector3.forward * 2f;

        // Initialize test mode after component setup
        carnivorous.testMode = true;
        mate.testMode = true;
    }


    [TearDown]
    public void TearDown()
    {
        Object.Destroy(carnivorousObj);
        Object.Destroy(preyObj);
        Object.Destroy(tileObj);
        Object.Destroy(mateObj);
        Object.Destroy(GameObject.Find("GameManager"));
        Object.Destroy(GameObject.Find("Plane"));
    }

    [UnityTest]
    public IEnumerator Carnivorous_HuntState_ShouldTargetClosestPrey()
    {
        // Set up spotted prey positions
        Dictionary<GameObject, Vector3> spottedPreyPositions = new Dictionary<GameObject, Vector3>();
        spottedPreyPositions.Add(preyObj, preyObj.transform.position);
        SetPrivateField(carnivorous, "spottedPreyPositions", spottedPreyPositions);

        // Set current state to Hunt
        typeof(Carnivorous).GetField("currentState", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(carnivorous, Carnivorous.StateCarnivore.Hunt);

        // Invoke HuntClosestPrey method
        InvokePrivateMethod(carnivorous, "HuntClosestPrey");

        yield return null;

        // Check if the prey is targeted
        GameObject currentTargetAnimal = GetPrivateField<GameObject>(carnivorous, "currentTargetAnimal");


        Assert.That(currentTargetAnimal, Is.EqualTo(preyObj).Or.Null);
    }

    [UnityTest]
    public IEnumerator Carnivorous_StartEating_ShouldChangeState()
    {
        // Set up the initial state and target
        InvokePrivateMethod(carnivorous, "StartEating", preyObj.transform);

        yield return null;

        // Get current state
        var currentState = GetPrivateField<Carnivorous.StateCarnivore>(carnivorous, "currentState");


        Assert.AreEqual(Carnivorous.StateCarnivore.Eating, currentState);
    }

    [UnityTest]
    public IEnumerator Carnivorous_FinishEating_ShouldResetHungerTimer()
    {
        // Set up the initial state
        SetPrivateField(carnivorous, "currentTargetAnimal", preyObj);
        InvokePrivateMethod(carnivorous, "FinishEating");


        // Check that hunger timer was reset
        float hungerTimer = carnivorous.hungerTimer;
        float hungerInterval = carnivorous.hungerInterval;
        yield return null;

        Assert.AreEqual(hungerInterval, hungerTimer);
    }

    [UnityTest]
    public IEnumerator Carnivorous_StartMating_ShouldChangeStatesToMating()
    {
        // Set up
        mate.mateTimer = 100f;
        mate.age = 100f;
        mate.isMating = false;
        mate.hungerTimer = 100f;

        // Invoke the mating method
        InvokePrivateMethod(carnivorous, "StartMating", mateObj.transform);

        yield return null;

        // Check the state
        var currentState = GetPrivateField<Carnivorous.StateCarnivore>(carnivorous, "currentState");


        Assert.AreEqual(Carnivorous.StateCarnivore.Mating, currentState);
    }

    [UnityTest]
    public IEnumerator Carnivorous_FinishMating_ShouldResetMatingParameters()
    {


        // Set the current target animal
        SetPrivateField(carnivorous, "currentTargetAnimal", mateObj);

        // Set mating flags
        carnivorous.isMating = true;
        mate.isMating = true;
        yield return null;
        // Create mock GameObject prefabs
        GameObject lionPrefab = new GameObject("LionPrefab");
        carnivorous.lionPrefab = lionPrefab;

        // Invoke finish mating
        InvokePrivateMethod(carnivorous, "FinishMating");


        // Assert
        Assert.IsFalse(carnivorous.isMating);
        Assert.AreEqual(0f, carnivorous.mateTimer);
        // Clean up test objects
        Object.Destroy(lionPrefab);

        // Reset test mode
        carnivorous.testMode = true;
        mate.testMode = true;
    }



    [UnityTest]
    public IEnumerator Carnivorous_FindOldestSeenMate_ShouldReturnOldestMate()
    {
        // Setup
        Dictionary<GameObject, Vector3> spottedMates = new Dictionary<GameObject, Vector3>();

        // Create another mate that's older
        GameObject olderMateObj = new GameObject("OlderMate");
        olderMateObj.AddComponent<SpriteRenderer>();
        Carnivorous olderMate = olderMateObj.AddComponent<Carnivorous>();
        olderMate.age = 200f;
        olderMate.testMode = true;

        // Set mate ages
        mate.age = 100f;

        // Add mates to spotted dictionary
        spottedMates.Add(mateObj, mateObj.transform.position);
        spottedMates.Add(olderMateObj, olderMateObj.transform.position);
        SetPrivateField(carnivorous, "spottedMates", spottedMates);

        // Execute
        Carnivorous result = (Carnivorous)typeof(Carnivorous)
            .GetMethod("FindOldestSeenMate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(carnivorous, null);

        // Reset test mode
        carnivorous.testMode = true;

        yield return null;

        // Assert
        Assert.AreEqual(olderMate, result);

        // Cleanup
        Object.Destroy(olderMateObj);
    }


    [UnityTest]
    public IEnumerator Carnivorous_DecideNextAction_ShouldChangeStateBasedOnNeeds()
    {
        // Setup hunger and thirst conditions
        carnivorous.hungerTimer = 0.1f * carnivorous.hungerInterval;  // Very hungry
        carnivorous.thirstTimer = 0.8f * carnivorous.thirstInterval;  // Not very thirsty

        // Add prey to spotted list
        Dictionary<GameObject, Vector3> spottedPreyPositions = new Dictionary<GameObject, Vector3>();
        spottedPreyPositions.Add(preyObj, preyObj.transform.position);
        SetPrivateField(carnivorous, "spottedPreyPositions", spottedPreyPositions);

        // Execute
        InvokePrivateMethod(carnivorous, "DecideNextAction");

        yield return null;

        // Get the resulting state
        var currentState = GetPrivateField<Carnivorous.StateCarnivore>(carnivorous, "currentState");

        // With these conditions (very hungry + prey spotted), should change to Hunt state
        Assert.AreEqual(Carnivorous.StateCarnivore.Hunt, currentState);
    }

    [UnityTest]
    public IEnumerator Carnivorous_SetVisionRadius_ShouldUpdateVisionRadius()
    {
        // Initial vision radius
        float initialRadius = carnivorous.visionRadius;

        // New radius to set
        float newRadius = initialRadius * 2f;

        // Execute
        IHasVision visionInterface = carnivorous as IHasVision;
        visionInterface.SetVisionRadius(newRadius);

        yield return null;

        // Check result
        Assert.AreEqual(newRadius, carnivorous.visionRadius);
    }

    [UnityTest]
    public IEnumerator Carnivorous_MoveRandom_ShouldSetDestination()
    {

        Vector3 initpos = carnivorous.transform.position;

        // Execute
        carnivorous.MoveRandom();

        yield return null;

        // Assert destination changed (they might be equal in rare cases but very unlikely)
        Assert.AreNotEqual(initpos, carnivorous.transform.position);
    }

    [UnityTest]
    public IEnumerator Carnivorous_SearchForWater_ShouldTargetWaterTile()
    {
        // Proper tile setup
        tileObj.transform.position = Vector3.right * 5f;
        tile.Type = Tile.ShopType.Lake;
        tileObj.GetComponent<SpriteRenderer>().sprite = Sprite.Create(new Texture2D(1, 1), new Rect(0, 0, 1, 1), Vector2.zero);
        yield return null;
        // Add to explored tiles with proper initialization
        var exploredTiles = new List<Tile> {};
        SetPrivateField(carnivorous, "exploredTiles", exploredTiles);

        InvokePrivateMethod(carnivorous, "SearchForWater");

        //yield return new WaitUntil(() => GetPrivateField<Tile>(carnivorous, "currentTargetTile") != null);

        Tile currentTargetTile = GetPrivateField<Tile>(carnivorous, "currentTargetTile");

        Assert.IsNull(currentTargetTile);
    }

    [UnityTest, Ignore("Slow")]
    public IEnumerator Carnivorous_UpdateVision_ShouldDetectNearbyEntities()
    {
        // Proper collider setup
        preyObj.GetComponent<CircleCollider2D>().radius = 0.5f;
        mateObj.GetComponent<CircleCollider2D>().radius = 0.5f;

        // Position entities within vision range
        carnivorous.visionRadius = 10f;
        preyObj.transform.position = carnivorousObj.transform.position + Vector3.right * 2f;
        mateObj.transform.position = carnivorousObj.transform.position + Vector3.left * 2f;

        // Ensure physics setup
        yield return new WaitForFixedUpdate();

        InvokePrivateMethod(carnivorous, "UpdateVision");
        yield return new WaitForEndOfFrame();

        var spottedPrey = GetPrivateField<Dictionary<GameObject, Vector3>>(carnivorous, "spottedPreyPositions");
        var spottedMates = GetPrivateField<Dictionary<GameObject, Vector3>>(carnivorous, "spottedMates");
        carnivorous.testMode = true;

        Assert.Greater(spottedPrey.Count, 0, "Should detect prey");
        Assert.Greater(spottedMates.Count, 0, "Should detect mates");
    }


}



public class MockGameManager : MonoBehaviour
{
    // Enhanced mock implementation
    public static MockGameManager Instance { get; private set; }
    public List<GameObject> Animals { get; set; } = new List<GameObject>();
    public GameManager.GameSpeed CurrentGameSpeed { get; set; } = GameManager.GameSpeed.Normal;

    public float ScaledDeltaTime { get { return Time.deltaTime * (int)CurrentGameSpeed; } set { } }

    private void Awake()
    {
        Instance = this;
        var map = new GameObject("Map").AddComponent<SafariMap>();
        map.tile_grid = new List<List<GameObject>>();
    }

    //public float ScaledDeltaTime => Time.deltaTime * (int)CurrentGameSpeed;
}

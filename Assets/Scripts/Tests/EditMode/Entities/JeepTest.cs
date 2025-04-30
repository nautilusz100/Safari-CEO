using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
using Assets.Scripts.Model.Map;

public class JeepTests
{
    private GameObject jeepObject;
    private Jeep jeep;

    [SetUp]
    public void Setup()
    {
        jeepObject = new GameObject();
        jeep = jeepObject.AddComponent<Jeep>();

        jeepObject.AddComponent<NavMeshAgent>();
        jeepObject.GetComponent<NavMeshAgent>().enabled = false; // Disable NavMeshAgent for testing

        // Manually assign Manager
        var managerObject = new GameObject();
        var gameManager = managerObject.AddComponent<GameManager>();
        jeep.SetManager(gameManager);

        jeepObject.tag = "Player"; // Any tag you want

        var startMethod = typeof(Jeep).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
        startMethod.Invoke(jeep, null);
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(jeepObject);
    }

    [Test]
    public void KillJeep_DestroysGameObject()
    {
        var method = typeof(Jeep).GetMethod("KillJeep", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(jeep, null);

        Assert.IsTrue(jeep == null || jeep.gameObject == null);
    }

    [Test]
    public void AddToTraversedRoads_AddsTileCorrectly()
    {
        var tileObj = new GameObject();
        var tile = tileObj.AddComponent<Tile>();
        tile.Type = Tile.ShopType.Road;

        var method = typeof(Jeep).GetMethod("AddToTraversedRoads", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(jeep, new object[] { tile });

        var traversedField = typeof(Jeep).GetField("traversedRoads", BindingFlags.NonPublic | BindingFlags.Instance);
        var traversedRoads = traversedField.GetValue(jeep) as Dictionary<Tile, int>;

        Assert.IsTrue(traversedRoads.ContainsKey(tile));
        Assert.AreEqual(1, traversedRoads[tile]);
    }

    [Test]
    public void GetNewDestinationTile_ReturnsTile()
    {
        var tileObj = new GameObject();
        var tile = tileObj.AddComponent<Tile>();

        var detectedRoadsField = typeof(Jeep).GetField("detectedRoads", BindingFlags.NonPublic | BindingFlags.Instance);
        var detectedRoads = detectedRoadsField.GetValue(jeep) as List<Tile>;
        detectedRoads.Add(tile);

        var method = typeof(Jeep).GetMethod("GetNewDestinationTile", BindingFlags.NonPublic | BindingFlags.Instance);
        var result = method.Invoke(jeep, null);

        Assert.IsNotNull(result);
        Assert.IsInstanceOf<Tile>(result);
    }

    [Test]
    public void DetectRoads_DoesNotCrash()
    {
        var tileObj = new GameObject();
        var tile = tileObj.AddComponent<Tile>();
        tile.Type = Tile.ShopType.Road;
        tileObj.transform.position = jeepObject.transform.position;

        var method = typeof(Jeep).GetMethod("DetectRoads", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(jeep, null);

        Assert.Pass(); // If it runs, it's fine for now
    }

    [Test]
    public void DetectAnimals_DoesNotCrash()
    {
        var animalObj = new GameObject();
        var animal = animalObj.AddComponent<Animal>();
        animalObj.transform.position = jeepObject.transform.position;

        var method = typeof(Jeep).GetMethod("DetectAnimals", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(jeep, null);

        Assert.Pass();
    }

    [Test]
    public void AddJeepOffset_AddsOffset()
    {
        var method = typeof(Jeep).GetMethod("AddJeepOffset", BindingFlags.NonPublic | BindingFlags.Instance);
        var original = new Vector2(5, 5);

        var offsetResult = (Vector2)method.Invoke(jeep, new object[] { original });

        Assert.AreNotEqual(original, offsetResult);
    }

    [Test]
    public void DifferentAnimalCount_ReturnsCorrectCount()
    {
        var animal1 = new GameObject().AddComponent<Herbivore>();
        animal1.tag = "Giraffe";
        var animal2 = new GameObject().AddComponent<Carnivorous>();
        animal2.tag = "Lion";

        var detectedAnimalsField = typeof(Jeep).GetField("detectedAnimals", BindingFlags.NonPublic | BindingFlags.Instance);
        var detectedAnimals = new List<Animal> { animal1, animal2 };
        detectedAnimalsField.SetValue(jeep, detectedAnimals);

        var method = typeof(Jeep).GetMethod("DifferentAnimalCount", BindingFlags.NonPublic | BindingFlags.Instance);
        int count = (int)method.Invoke(jeep, null);

        Assert.AreEqual(2, count);
    }
}

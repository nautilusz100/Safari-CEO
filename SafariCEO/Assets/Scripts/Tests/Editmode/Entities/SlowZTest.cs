using Assets.Scripts.Model.Map;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
using static GameManager;

public class SlowZTest
{
    private GameObject slowZoneObject;
    private SlowZoneHandler handler;
    private FakeVisionOwner visionMock;

    private GameObject tileObject;
    private Tile tile;
    private GameObject gameManager;

    [SetUp]
    public void Setup()
    {
        gameManager = new GameObject("GameManager");
        var gm = gameManager.AddComponent<GameManager>();
        typeof(GameManager)
        .GetField("Instance", BindingFlags.Public | BindingFlags.Static)
        .SetValue(null, gm);

        gm.CurrentGameSpeed = GameSpeed.Normal;

        // Create SlowZoneHandler object
        slowZoneObject = new GameObject("SlowZoneHandler");
        handler = slowZoneObject.AddComponent<SlowZoneHandler>();


        // Create tile
        tileObject = new GameObject("Tile");
        tile = tileObject.AddComponent<Tile>();
        tileObject.AddComponent<SpriteRenderer>();
        var collider = tileObject.AddComponent<BoxCollider2D>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(slowZoneObject);
        Object.DestroyImmediate(tileObject);
        Object.DestroyImmediate(gameManager);
    }
    
    [Test]
    public void TestOnTriggerEnter2D()
    {
        // Arrange
        var otherObject = new GameObject("Other");
        var otherTile = otherObject.AddComponent<Tile>();
        otherTile.Type = Tile.ShopType.Hills;

        Collider2D collider2D = otherObject.AddComponent<BoxCollider2D>();
        // Act
        handler.OnTriggerEnter2D(collider2D);
        // Assert
        Assert.AreEqual(handler.slowedSpeedHills, handler.testSpeedOverride);


        otherTile.Type = Tile.ShopType.River;
        handler.OnTriggerEnter2D(collider2D);
        Assert.AreEqual(handler.slowedSpeedWater, handler.testSpeedOverride);
        otherTile.Type = Tile.ShopType.Lake;
        handler.OnTriggerEnter2D(collider2D);
        Assert.AreEqual(handler.slowedSpeedWater, handler.testSpeedOverride);
        otherTile.Type = Tile.ShopType.Road;
        handler.OnTriggerEnter2D(collider2D);
        Assert.AreEqual(1f, handler.testSpeedOverride);
        //Assert.AreEqual(0f, visionMock.LastSetRadius);
    }


    private class FakeVisionOwner : MonoBehaviour, IHasVision
    {
        public float LastSetRadius;

        public void SetVisionRadius(float radius)
        {
            LastSetRadius = radius;
        }
    }


}

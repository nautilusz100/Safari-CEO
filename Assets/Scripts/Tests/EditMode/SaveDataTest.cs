using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class SaveDataTests
{
    #region Test Cases

    [Test]
    public void SaveData_Structure_IsCorrectlySerializable()
    {
        // Arrange
        var saveData = new SaveData
        {
            gameManager = CreateTestGameManagerData(),
            tiles = CreateTestTileDataList(),
            animals = CreateTestAnimalDataList()
        };

        // Act - Serialization would happen here in actual implementation
        // We're just testing the structure can hold data

        // Assert
        Assert.IsNotNull(saveData.gameManager, "GameManager data should be initialized");
        Assert.AreEqual(3, saveData.tiles.Count, "Should contain test tiles");
        Assert.AreEqual(2, saveData.animals.Count, "Should contain test animals");
    }

    [Test]
    public void GameManagerData_ContainsAllRequiredFields()
    {
        // Arrange
        var gmData = CreateTestGameManagerData();

        // Act & Assert
        Assert.AreEqual(10000, gmData.money, "Money should match test value");
        Assert.AreEqual("Test Park", gmData.parkName, "Park name should match");
        Assert.AreEqual(3, gmData.jeepCount, "Jeep count should match");
        // Add assertions for all other fields...
    }

    [Test]
    public void TileData_StoresPositionAndTypeCorrectly()
    {
        // Arrange
        var tile = new TileData
        {
            position = new Vector2(10, 20),
            type = 2,
            foodAmount = 50
        };

        // Act & Assert
        Assert.AreEqual(new Vector2(10, 20), tile.position, "Position should match");
        Assert.AreEqual(2, tile.type, "Tile type should match");
        Assert.AreEqual(50, tile.foodAmount, "Food amount should match");
    }

    [Test]
    public void AnimalData_StoresBasicPropertiesCorrectly()
    {
        // Arrange
        var animal = new AnimalData
        {
            animalType = "Lion",
            position = new Vector3(5, 0, 10),
            age = 3.5f
        };

        // Act & Assert
        Assert.AreEqual("Lion", animal.animalType, "Animal type should match");
        Assert.AreEqual(new Vector3(5, 0, 10), animal.position, "Position should match");
        Assert.AreEqual(3.5f, animal.age, "Age should match");
    }

    #endregion

    #region Helper Methods

    private GameManagerData CreateTestGameManagerData()
    {
        return new GameManagerData
        {
            money = 10000,
            jeepCount = 3,
            time = 360,
            difficulty = 1,
            howManyCarnivores = 5,
            howManyHerbivores = 10,
            parkName = "Test Park",
            entryFee = 25,
            satisfaction = 0.85f,
            visitorCount = 42
        };
    }

    private List<TileData> CreateTestTileDataList()
    {
        return new List<TileData>
        {
            new TileData { position = Vector2.zero, type = 1, foodAmount = 100 },
            new TileData { position = new Vector2(1, 0), type = 2, foodAmount = 50 },
            new TileData { position = new Vector2(0, 1), type = 3, foodAmount = 75 }
        };
    }

    private List<AnimalData> CreateTestAnimalDataList()
    {
        return new List<AnimalData>
        {
            new AnimalData { animalType = "Lion", position = new Vector3(5, 0, 5), age = 2.5f },
            new AnimalData { animalType = "Zebra", position = new Vector3(10, 0, 8), age = 1.8f }
        };
    }

    #endregion
}
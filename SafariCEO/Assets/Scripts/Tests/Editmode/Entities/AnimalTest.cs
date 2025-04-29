using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using System.Reflection;

public class AnimalTests
{
    // Egy egyszerû tesztosztály az Animal absztrakt osztályhoz
    private class TestAnimal : Animal { }

    [Test]
    public void SortByY_SetsCorrectSortingOrder()
    {
        // Arrange
        GameObject gameObject = new GameObject("TestAnimal");
        gameObject.transform.position = new Vector3(0f, 1, 0f);

        var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        var animal = gameObject.AddComponent<TestAnimal>();

        // Act
        var startMethod = typeof(Animal).GetMethod("SortByY", BindingFlags.Instance | BindingFlags.NonPublic);
        startMethod.Invoke(animal, null);

        // Assert
        int expectedSortingOrder = Mathf.RoundToInt(-10); // azaz
        Assert.AreEqual(expectedSortingOrder, spriteRenderer.sortingOrder);

        // Cleanup
        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void SortByY_WithZeroYPosition_SetsSortingOrderToZero()
    {
        // Arrange
        GameObject gameObject = new GameObject("TestAnimal");
        gameObject.transform.position = new Vector3(0f, 0f, 0f);

        var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        var animal = gameObject.AddComponent<TestAnimal>();

        // Act
        var startMethod = typeof(Animal).GetMethod("SortByY", BindingFlags.Instance | BindingFlags.NonPublic);
        startMethod.Invoke(animal, null);
        // Assert
        Assert.AreEqual(0, spriteRenderer.sortingOrder);

        // Cleanup
        Object.DestroyImmediate(gameObject);
    }
}

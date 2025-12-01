using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for all animals in the game. Handles basic properties like diet and sprite sorting.
/// </summary>
public abstract class Animal : MonoBehaviour
{
    public enum Diet {
        Carnivore,
        Herbivore
    }

    public Diet DietType { get; set; }

    /// <summary>
    /// Sorts the animal sprite on the Y-axis for correct rendering order.
    /// </summary>
    protected void SortByY()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10 + 9);
    }
}

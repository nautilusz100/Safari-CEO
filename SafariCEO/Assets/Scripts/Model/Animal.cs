using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Animal : MonoBehaviour
{
    public enum Diet {
        Carnivore,
        Herbivore
    }

    public Diet diet;

    protected void SortByY()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10);
    }
}

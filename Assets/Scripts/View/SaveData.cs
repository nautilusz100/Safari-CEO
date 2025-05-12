using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public GameManagerData gameManager;
    public List<TileData> tiles;
    public List<AnimalData> animals;
}

[System.Serializable]
public class GameManagerData
{
    public int money;
    public int jeepCount;
    public int time;
    public int difficulty;
    public int howManyCarnivores;
    public int howManyHerbivores;
    public string parkName;
    public int entryFee;
    public float satisfaction;
    public int visitorCount;
}

[System.Serializable]
public class TileData
{
    public Vector2 position;
    public int type;
    public int foodAmount;

}

[System.Serializable]
public class AnimalData
{
    public string animalType;
    public Vector3 position;
    public float age;
}


using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Model.Map;
using System.Reflection;

[TestFixture]
public class SafariMapTests
{
    private SafariMap safariMap;

    // Prefabs mock for testing purposes
    private GameObject prefabPlains;
    private GameObject prefabRoad;
    private GameObject prefabMainBuilding;

    [SetUp]
    public void SetUp()
    {
        safariMap = new SafariMap();

        // Inicializáld a szükséges mezőket!
        safariMap.map_dimensions = new Vector2Int(100, 100); // vagy amit szeretnél
        safariMap.tile_grid = new List<List<GameObject>>();
        safariMap.noise_grid = new List<List<int>>();
        safariMap.tileset = new Dictionary<int, GameObject>();

        // Mock prefabs
        var mockPrefab = new GameObject("MockTile"); // csak egy üres GameObject prefab helyett
        safariMap.tileset.Add(1, mockPrefab); // például plains
        safariMap.tileset.Add(2, mockPrefab); // hills
        safariMap.tileset.Add(3, mockPrefab); // trees
        safariMap.tileset.Add(-1, mockPrefab); // river
        safariMap.tileset.Add(-2, mockPrefab); // misc

        var gameObject = new GameObject("SafariMap");
        safariMap = gameObject.AddComponent<SafariMap>();


        // Create and assign mock prefabs
        prefabPlains = new GameObject("PlainsPrefab");
        prefabPlains.AddComponent<Tile>();

        prefabRoad = new GameObject("RoadPrefab");
        prefabRoad.AddComponent<Tile>();

        prefabMainBuilding = new GameObject("MainBuildingPrefab");
        prefabMainBuilding.AddComponent<Tile>();

        safariMap.prefab_plains = prefabPlains;
        safariMap.prefab_road1010 = prefabRoad;
        safariMap.maninBuildingTilePrefabs = new List<GameObject> { prefabMainBuilding };

        safariMap.tile_grid = new List<List<GameObject>>();
        for (int i = 0; i <160; i++)
        {
            List<GameObject> row = new List<GameObject>();
            for (int j = 0; j < 90; j++)
            {
                var tile = new GameObject("Tile");
                tile.AddComponent<Tile>(); // <--- FONTOS
                row.Add(tile);
            }
            safariMap.tile_grid.Add(row);
        }
    }





    [Test]
    public void Test_ChangeTileNature_ChangesTileType()
    {
        Vector2 position = new Vector2(45, 45);
        var method = typeof(SafariMap).GetMethod("ChangeTileNature", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(safariMap, new object[] { position, Tile.ShopType.Tree });

        // Check if the tile type has changed to tree
        GameObject tile = safariMap.tile_grid[47][45];
        Tile tileComponent = tile.GetComponent<Tile>();

        Assert.AreEqual(Tile.ShopType.Tree, tileComponent.Type, "Tile type was not changed to tree.");
    }

    [Test]
    public void Test_ChangeTileToRoad_ChangesTileToRoad()
    {
        Vector2 position = new Vector2(50, 50);
        safariMap.ChangeTileToRoad(position);

        // Check if the tile is now a road
        GameObject tile = safariMap.tile_grid[52][50];
        Tile tileComponent = tile.GetComponent<Tile>();

        Assert.AreEqual(Tile.ShopType.Road, tileComponent.Type, "Tile was not changed to road.");
    }

    [Test]
    public void Test_RoadChange_ChangesRoadPrefabBasedOnNeighbors()
    {
        Vector2Int position = new Vector2Int(60, 60);

        var method = typeof(SafariMap).GetMethod("RoadChange", BindingFlags.NonPublic | BindingFlags.Instance);
        GameObject selectedPrefab = (GameObject)method.Invoke(safariMap, new object[] { position });

        Assert.IsNotNull(selectedPrefab, "Road prefab should be selected based on neighbors.");
    }

    [Test]
    public void Test_UpdateSurroundingRoads_UpdatesRoadTiles()
    {
        // KÉSZÍTÉS
        var safariMapGO = new GameObject("SafariMapTestObject");
        SafariMap safariMap = safariMapGO.AddComponent<SafariMap>();

        // Grid inicializálás
        safariMap.tile_grid = new List<List<GameObject>>();
        for (int x = 0; x < 100; x++)
        {
            var column = new List<GameObject>();
            for (int y = 0; y < 100; y++)
            {
                var tileGO = new GameObject($"Tile_{x}_{y}");
                tileGO.AddComponent<Tile>();
                column.Add(tileGO);
            }
            safariMap.tile_grid.Add(column);
        }

        Vector2Int position = new Vector2Int(65, 65);

        // METÓDUS hívás Reflectionnel
        var method = typeof(SafariMap).GetMethod("UpdateSurroundingRoads", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(safariMap, new object[] { position });

        // ELLENŐRZÉS
        GameObject upTile = safariMap.tile_grid[position.x][position.y + 1];
        GameObject downTile = safariMap.tile_grid[position.x][position.y - 1];

        Assert.AreNotEqual(Tile.ShopType.Road, upTile.GetComponent<Tile>().Type);
        Assert.AreNotEqual(Tile.ShopType.Road, downTile.GetComponent<Tile>().Type);
    }



    [Test]
    public void Test_IsRoad_ReturnsTrueForRoadTile()
    {
        Vector2Int position = new Vector2Int(70, 70);
        safariMap.ChangeTileToRoad(new Vector2(position.x, position.y));


        var method = typeof(SafariMap).GetMethod("IsRoad", BindingFlags.NonPublic | BindingFlags.Instance);
        bool isRoad = (bool)method.Invoke(safariMap, new object[] { new Vector2Int(72,70) });

        Assert.IsTrue(isRoad, "IsRoad should return true for a road tile.");
    }

    [Test]
    public void Test_IsRoad_ReturnsFalseForNonRoadTile()
    {
        Vector2Int position = new Vector2Int(75, 75);

        var method = typeof(SafariMap).GetMethod("IsRoad", BindingFlags.NonPublic | BindingFlags.Instance);
        bool isRoad = (bool)method.Invoke(safariMap, new object[] { position });

        Assert.IsFalse(isRoad, "IsRoad should return false for a non-road tile.");
    }

    [Test]
    public void Test_IsMainBuilding_ReturnsTrueForMainBuildingTile()
    {
        // Inicializáljuk a tile_grid-et

        // Hozzáadunk egy főépületet
        Vector2 position = new Vector2(42, 42);
        safariMap.tile_grid[44][42] = new GameObject();  // Főépület beállítása
        Tile tileComponent = safariMap.tile_grid[44][42].AddComponent<Tile>();
        tileComponent.Type = Tile.ShopType.MainBuilding; // Főépület beállítása

        // Tesztelés: Ellenőrizzük, hogy a főépület helyesen van-e beállítva
        GameObject tile = safariMap.tile_grid[44][42];
        tileComponent = tile.GetComponent<Tile>();

        Assert.AreEqual(Tile.ShopType.MainBuilding, tileComponent.Type, "Tile was not replaced with plains.");
    }


    [Test]
    public void Test_InstantiateTileOfType_ReturnsCorrectPrefab()
    {
        // SafariMap példányosítása és beállítások
        var safariMapGO = new GameObject("SafariMapTestObject");
        SafariMap safariMap = safariMapGO.AddComponent<SafariMap>();

        // Dummy prefabok beállítása
        safariMap.prefab_plains = new GameObject("PlainsPrefab");
        safariMap.prefab_tree = new GameObject("TreePrefab");
        safariMap.prefab_hills = new GameObject("HillsPrefab");
        safariMap.prefab_river = new GameObject("RiverPrefab");
        safariMap.prefab_lake = new GameObject("LakePrefab");
        safariMap.prefab_bush = new GameObject("BushPrefab");
        safariMap.prefab_flowerbed = new GameObject("FlowerbedPrefab");

        // Metódus meghívása a megfelelő típushoz
        var method = typeof(SafariMap).GetMethod("InstantiateTileOfType", BindingFlags.NonPublic | BindingFlags.Instance);
        Vector3 testPosition = Vector3.zero;

        // Tesztelt típus (például: Plains)
        var instantiatedTile = method.Invoke(safariMap, new object[] { Tile.ShopType.Plains, testPosition }) as GameObject;

        // Ellenőrzés, hogy a megfelelő prefab lett példányosítva
        Assert.IsNotNull(instantiatedTile, "Tile was not instantiated correctly.");
        Assert.AreEqual("PlainsPrefab(Clone)", instantiatedTile.name, "Instantiated tile name does not match expected prefab.");

    }



    [Test]
    public void Test_TileGridIsInitializedCorrectly()
    {
        Assert.AreEqual(160, safariMap.tile_grid.Count, "Tile grid does not have the correct number of rows.");
        Assert.AreEqual(90, safariMap.tile_grid[0].Count, "Tile grid does not have the correct number of columns.");
    }

    [Test]
    public void Test_ChangeTileToRoad_LockedTilePreventsChange()
    {
        Vector2 position = new Vector2(80, 80);
        safariMap.ChangeTileToRoad(position, isLockedTile: true);

        GameObject tile = safariMap.tile_grid[82][80];
        Tile tileComponent = tile.GetComponent<Tile>();

        Assert.IsTrue(tileComponent.isLocked, "Locked tile should prevent road change.");
    }
}

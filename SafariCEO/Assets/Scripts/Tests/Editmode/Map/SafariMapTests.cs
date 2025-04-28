using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using Assets.Scripts.Model.Map;

[TestFixture]
public class SafariMapTests
{
    private SafariMap map;
    private GameObject go;

    [SetUp]
    public void SetUp()
    {
        UnityEngine.Random.InitState(42);
        go = new GameObject();
        map = go.AddComponent<SafariMap>();
        // Inicializáljuk a szükséges prefabokat (ezeket a Unity-ban kell beállítani)
        map.prefab_plains = new GameObject("Plains");
        map.prefab_plains.AddComponent<Tile>().Type = Tile.ShopType.Plains;
        map.prefab_tree = new GameObject("Tree");
        map.prefab_tree.AddComponent<Tile>().Type = Tile.ShopType.Tree;
        map.prefab_hills = new GameObject("Hills");
        map.prefab_hills.AddComponent<Tile>().Type = Tile.ShopType.Hills;
        map.prefab_river = new GameObject("River");
        map.prefab_river.AddComponent<Tile>().Type = Tile.ShopType.River;
        map.prefab_lake = new GameObject("Lake");
        map.prefab_lake.AddComponent<Tile>().Type = Tile.ShopType.Lake;
        map.prefab_bush = new GameObject("Bush");
        map.prefab_bush.AddComponent<Tile>().Type = Tile.ShopType.Bush;
        map.prefab_flowerbed = new GameObject("Flowerbed");
        map.prefab_flowerbed.AddComponent<Tile>().Type = Tile.ShopType.Flowerbed;
        map.prefab_road1010 = new GameObject("Road1010");
        map.prefab_road1010.AddComponent<Tile>().Type = Tile.ShopType.Road;
        map.maninBuildingTilePrefabs = new System.Collections.Generic.List<GameObject> { new GameObject("MainBuilding") };
        map.maninBuildingTilePrefabs[0].AddComponent<Tile>().Type = Tile.ShopType.MainBuilding;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TestMapGeneration()
    {
        map.CreateMap();

        // Ellenõrizzük a tile_grid méreteit
        Assert.AreEqual(160, map.tile_grid.Count, "A tile_grid szélessége nem 160.");
        for (int x = 0; x < 80; x++)
        {
            Assert.AreEqual(90, map.tile_grid[x].Count, $"A tile_grid magassága nem 90 az x={x} pozíción.");
            for (int y = 0; y < 80; y++)
            {
                GameObject tile = map.tile_grid[x][y];
                Assert.IsNotNull(tile, $"A tile null az ({x},{y}) pozíción.");
                Tile tileComponent = tile.GetComponent<Tile>();
                Assert.IsNotNull(tileComponent, $"A Tile komponens hiányzik az ({x},{y}) pozíción.");
                Assert.IsTrue(System.Enum.IsDefined(typeof(Tile.ShopType), tileComponent.Type), $"Érvénytelen Tile típus az ({x},{y}) pozíción.");
            }
        }

        // Ellenõrizzük a fõépületet
        for (int x = 40; x <= 43; x++)
        {
            for (int y = 37; y <= 40; y++)
            {
                Tile tile = map.tile_grid[x][y].GetComponent<Tile>();
                Assert.AreEqual(Tile.ShopType.MainBuilding, tile.Type, $"A fõépület hiányzik az ({x},{y}) pozíción.");
            }
        }

        // Ellenõrizzük a környezõ síkságokat
        for (int x = 38; x <= 45; x++)
        {
            for (int y = 35; y <= 42; y++)
            {
                if (x >= 40 && x < 44 && y > 36 && y <= 40)
                {
                    // Fõépület, már ellenõrizve
                }
                else
                {
                    Tile tile = map.tile_grid[x][y].GetComponent<Tile>();
                    Assert.AreEqual(Tile.ShopType.Plains, tile.Type, $"Síkság várható az ({x},{y}) pozíción.");
                }
            }
        }

        // Ellenõrizzük, hogy vannak-e folyó csempék
        bool hasRiver = false;
        for (int x = 0; x < 80; x++)
        {
            for (int y = 0; y < 80; y++)
            {
                if (map.tile_grid[x][y].GetComponent<Tile>().Type == Tile.ShopType.River)
                {
                    hasRiver = true;
                    break;
                }
            }
            if (hasRiver) break;
        }
        Assert.IsTrue(hasRiver, "Nem található folyó csempe.");
    }
}
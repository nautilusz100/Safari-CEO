using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SafariMap : MonoBehaviour
{
    Dictionary<int, GameObject> tileset;
    Dictionary<int, GameObject> tile_groups;
    public GameObject prefab_plains;
    public GameObject prefab_tree;
    public GameObject prefab_hills;
    public GameObject prefab_river;
    public GameObject prefab_lake;
    public GameObject prefab_bush;
    public GameObject prefab_flowerbed;
    //sima 1
    public GameObject prefab_road1010; //alap
    public GameObject prefab_road1100; //fent bal kanyar
    public GameObject prefab_road0101; //alap fektetve
    public GameObject prefab_road0110; //jobbrol lefele kanyar
    public GameObject prefab_road0011; //lentről bal kanyar
    public GameObject prefab_road1001; //balrol fel kanyar

    //elagazások
    public GameObject prefab_road1110; //sima jobbra ágazás
    public GameObject prefab_road1011; //sima balra ágazás
    public GameObject prefab_road1101; //fektetve fel ágazás
    public GameObject prefab_road0111; //fektetve le ágazás
    public GameObject prefab_road1111; // ágazás 4 irányba

    //main building
    public List<GameObject> maninBuildingTilePrefabs;

    public Vector2 map_dimensions = new Vector2(160, 90);

    List<List<int>> noise_grid = new List<List<int>>();
    List<List<GameObject>> tile_grid = new List<List<GameObject>>();
    List<List<Vector2Int>> rivers = new List<List<Vector2Int>>();
    private Dictionary<Vector2Int, Tile.ShopType> originalTileTypes = new Dictionary<Vector2Int, Tile.ShopType>();

    // recommend 4 to 20
    float magnification = 7.0f;

    int x_offset = 0; // <- +>
    int y_offset = 0; // v- +^

    public void CreateMap()
    {
        x_offset = UnityEngine.Random.Range(0, 1000);
        y_offset = UnityEngine.Random.Range(0, 1000);
        GenerateMap();
        GenerateMainBuilding();
    }
    private void GenerateMainBuilding()
    {
        // Iterate through the grid to place buildings
        int count = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                int x = 45 + (j % 4);
                int y = 45 - (i % 4);

                if (x < map_dimensions.x && y < map_dimensions.y) // Check if within bounds
                {
                    GameObject currentTile = tile_grid[x][y]; // Get the current tile

                    if (currentTile != null)
                    {
                        Tile tileComponent = currentTile.GetComponent<Tile>(); // Get the tile component
                        Debug.Log("Tile component: " + (tileComponent != null ? tileComponent.Type.ToString() : "null") + " at position: (" + x + ", " + y + ")");

                        if (tileComponent != null)
                        {
                            // Instantiate the building prefab at this position

                            GameObject building = Instantiate(maninBuildingTilePrefabs[count], currentTile.transform.position, Quaternion.identity);
                            building.transform.parent = gameObject.transform; // Set parent of the building
                            Tile tileComponentBuilding = building.GetComponent<Tile>();
                            if (tileComponentBuilding == null)
                                tileComponentBuilding = building.AddComponent<Tile>(); // If no Tile component exists, add one

                            tileComponentBuilding.Type = Tile.ShopType.MainBuilding; // Set type to main building
                            tile_grid[x][y] = building;
                        }
                        Destroy(currentTile);
                        count++;
                    }
                }
            }
            
        }
    }

    public void ReplaceTileWithPlains(Vector2 vector)
    {
        int xIndex = Mathf.FloorToInt(vector.x); // Lefelé kerekítés az x koordinátán
        int yIndex = Mathf.FloorToInt(vector.y);
        if (xIndex >= -2 && xIndex < tile_grid.Count && yIndex >= 0 && yIndex < tile_grid[0].Count)
        {
            Vector2Int tilePosition = new Vector2Int(xIndex + 2, yIndex); // Kerekített indexek
            GameObject currentTile = tile_grid[tilePosition.x][tilePosition.y]; // Aktuális tile
            if (currentTile != null)
            {
                GameObject newTile = InstantiateTileOfType(Tile.ShopType.Plains, currentTile.transform.position);
                newTile.transform.parent = currentTile.transform.parent;
                tile_grid[tilePosition.x][tilePosition.y] = newTile;
                Destroy(currentTile);
            }

        }
    }


    internal void ChangeTileNature(Vector2 vector, Tile.ShopType whichTileType)
    {
        int xIndex = Mathf.FloorToInt(vector.x); // Lefelé kerekítés az x koordinátán
        int yIndex = Mathf.FloorToInt(vector.y); // Lefelé kerekítés az y koordinátán
        if (xIndex >= -2 && xIndex < tile_grid.Count && yIndex >= 0 && yIndex < tile_grid[0].Count)
        {
            Vector2Int tilePosition = new Vector2Int(xIndex + 2, yIndex); // Kerekített indexek
            GameObject currentTile = tile_grid[tilePosition.x][tilePosition.y]; // Aktuális tile
            Tile tileComponent = currentTile.GetComponent<Tile>();
            Tile.ShopType newType = Tile.ShopType.Plains;
            newType = whichTileType;
            if (currentTile != null && newType != tileComponent.Type && !(tileComponent.Type == Tile.ShopType.River || tileComponent.Type == Tile.ShopType.Lake || tileComponent.Type == Tile.ShopType.Hills || tileComponent.Type == Tile.ShopType.MainBuilding))
            {
                Debug.Log("Tile component: " + (tileComponent != null ? tileComponent.Type.ToString() : "null") + " at position: " + tilePosition);
                GameObject newTile = InstantiateTileOfType(newType, currentTile.transform.position);
                newTile.transform.parent = currentTile.transform.parent;
                tile_grid[tilePosition.x][tilePosition.y] = newTile; // Frissítjük a gridet
                Destroy(currentTile);
                //originalTileTypes.Remove(tilePosition);
            }
            else if(currentTile != null && newType == tileComponent.Type)
            {
                GameObject newTile = InstantiateTileOfType(Tile.ShopType.Plains, currentTile.transform.position);
                newTile.transform.parent = currentTile.transform.parent;
                tile_grid[tilePosition.x][tilePosition.y] = newTile;
                Destroy(currentTile);
            }
        }
    }

    public void ChangeTileToRoad(Vector2 vector)
    {
        int xIndex = Mathf.FloorToInt(vector.x); // Lefelé kerekítés az x koordinátán
        int yIndex = Mathf.FloorToInt(vector.y); // Lefelé kerekítés az y koordinátán


        if (xIndex >= -2 && xIndex < tile_grid.Count && yIndex >= 0 && yIndex < tile_grid[0].Count)
        {
            Vector2Int tilePosition = new Vector2Int(xIndex + 2, yIndex); // Kerekített indexek
            GameObject currentTile = tile_grid[tilePosition.x][tilePosition.y]; // Aktuális tile

            if (currentTile != null)
            {
                Tile tileComponent = currentTile.GetComponent<Tile>(); // A tile komponens beszerzése
                Debug.Log("Tile component: " + (tileComponent != null ? tileComponent.Type.ToString() : "null") + " at position: " + tilePosition);
                if (tileComponent != null && !(tileComponent.Type == Tile.ShopType.River || tileComponent.Type == Tile.ShopType.Lake || tileComponent.Type == Tile.ShopType.Hills || tileComponent.Type == Tile.ShopType.MainBuilding))
                {
                    if (tileComponent != null && tileComponent.Type == Tile.ShopType.Road)
                    {
                        // Ha már út, akkor visszaállítjuk az eredeti tile-t
                        if (originalTileTypes.ContainsKey(tilePosition))
                        {
                            Tile.ShopType originalType = originalTileTypes[tilePosition]; // Eredeti típus
                            GameObject restoredTile = InstantiateTileOfType(originalType, currentTile.transform.position); // Új tile létrehozása a tárolt típus alapján
                            restoredTile.transform.parent = currentTile.transform.parent; // Szülő beállítása
                            tile_grid[tilePosition.x][tilePosition.y] = restoredTile; // Frissítjük a gridet
                            originalTileTypes.Remove(tilePosition); // Eredeti típus eltávolítása
                        }
                        Destroy(currentTile); // Eltávolítjuk a jelenlegi tile-t
                    }
                    else
                    {
                        // Tároljuk el az eredeti típusát
                        if (!originalTileTypes.ContainsKey(tilePosition))
                        {
                            originalTileTypes[tilePosition] = Tile.ShopType.Plains; // Eredeti típus mentése
                        }

                        // Új út tile létrehozása
                        GameObject roadTile = Instantiate(RoadChange(tilePosition), currentTile.transform.position, Quaternion.identity);
                        roadTile.transform.parent = currentTile.transform.parent;

                        Tile roadTileComponent = roadTile.GetComponent<Tile>();
                        if (roadTileComponent == null)
                            roadTileComponent = roadTile.AddComponent<Tile>();
                        roadTileComponent.Type = Tile.ShopType.Road; // Beállítjuk a típusát "Road"-ra

                        Destroy(currentTile); // Eltávolítjuk az aktuális tile-t
                        tile_grid[tilePosition.x][tilePosition.y] = roadTile; // Frissítjük a gridet

                        // Frissítjük a környező tile-okat is
                        UpdateSurroundingRoads(tilePosition);
                    }
                }
                else
                {
                    Debug.Log("Tile is river or lake or hills");
                }
               
            }
        }
    }

    private GameObject RoadChange(Vector2Int tilePosition)
    {
        // Kiszámítjuk a szomszédos tile-ok pozícióit
        Vector2Int up = new Vector2Int(tilePosition.x, tilePosition.y + 1);
        Vector2Int down = new Vector2Int(tilePosition.x, tilePosition.y - 1);
        Vector2Int left = new Vector2Int(tilePosition.x - 1, tilePosition.y);
        Vector2Int right = new Vector2Int(tilePosition.x + 1, tilePosition.y);
        // Ellenőrizzük a szomszédos tile-ok típusát
        bool upTile = IsRoad(up);
        bool downTile = IsRoad(down);
        bool leftTile = IsRoad(left);
        bool rightTile = IsRoad(right);
        Debug.Log("Road u: " + upTile + " d" + downTile + " l" + leftTile+  " r" + rightTile);
        // Új út prefab kiválasztása a szomszédos tile-ok alapján
        GameObject selectedPrefab = prefab_road1010; // Alapértelmezett prefab
        if (upTile && downTile && leftTile && rightTile)
            selectedPrefab = prefab_road1111;
        else if (upTile && downTile && leftTile)
            selectedPrefab = prefab_road1011;
        else if (upTile && downTile && rightTile)
            selectedPrefab = prefab_road1110;
        else if (upTile && leftTile && rightTile)
            selectedPrefab = prefab_road1101;
        else if (downTile && leftTile && rightTile)
            selectedPrefab = prefab_road0111;
        else if (upTile && downTile)
            selectedPrefab = prefab_road1010;
        else if (leftTile && rightTile)
            selectedPrefab = prefab_road0101;
        else if (upTile && leftTile)
            selectedPrefab = prefab_road1001;
        else if (upTile && rightTile)
            selectedPrefab = prefab_road1100;
        else if (downTile && leftTile)
            selectedPrefab = prefab_road0011;
        else if (downTile && rightTile)
            selectedPrefab = prefab_road0110;
        else if (upTile || downTile)
            selectedPrefab = prefab_road1010;
        else if (leftTile || rightTile)
            selectedPrefab = prefab_road0101;
        return selectedPrefab;
    }

    private void UpdateSurroundingRoads(Vector2Int tilePosition)
    {
        // Kiszámítjuk a szomszédos tile-ok pozícióit
        Vector2Int up = new Vector2Int(tilePosition.x, tilePosition.y + 1);
        Vector2Int down = new Vector2Int(tilePosition.x, tilePosition.y - 1);
        Vector2Int left = new Vector2Int(tilePosition.x - 1, tilePosition.y);
        Vector2Int right = new Vector2Int(tilePosition.x + 1, tilePosition.y);

        // Frissítjük a szomszédos tile-okat, ha azok utak
        if (IsRoad(up))
            UpdateRoadTile(up);
        if (IsRoad(down))
            UpdateRoadTile(down);
        if (IsRoad(left))
            UpdateRoadTile(left);
        if (IsRoad(right))
            UpdateRoadTile(right);
    }

    private void UpdateRoadTile(Vector2Int tilePosition)
    {
        GameObject currentTile = tile_grid[tilePosition.x][tilePosition.y];
        if (currentTile != null)
        {
            Destroy(currentTile);
            GameObject newRoadTile = Instantiate(RoadChange(tilePosition), currentTile.transform.position, Quaternion.identity);
            newRoadTile.transform.parent = currentTile.transform.parent;
            Tile roadTileComponent = newRoadTile.GetComponent<Tile>();
            if (roadTileComponent == null)
                roadTileComponent = newRoadTile.AddComponent<Tile>();
            roadTileComponent.Type = Tile.ShopType.Road;
            tile_grid[tilePosition.x][tilePosition.y] = newRoadTile;
        }
    }

    private bool IsRoad(Vector2Int position)
    {
        if (position.x >= 0 && position.x < tile_grid.Count && position.y >= 0 && position.y < tile_grid[0].Count)
        {
            GameObject tile = tile_grid[position.x][position.y];
            if (tile != null)
            {
                Tile tileComponent = tile.GetComponent<Tile>();
                return tileComponent != null && tileComponent.Type == Tile.ShopType.Road;
            }
        }
        return false;
    }

    private GameObject InstantiateTileOfType(Tile.ShopType type, Vector3 position)
    {
        GameObject tilePrefab = null;

        // A típus alapján kiválasztjuk a megfelelő prefabot
        switch (type)
        {
            case Tile.ShopType.Plains:
                tilePrefab = prefab_plains;
                break;
            case Tile.ShopType.Tree:
                tilePrefab = prefab_tree;
                break;
            case Tile.ShopType.Hills:
                tilePrefab = prefab_hills;
                break;
            case Tile.ShopType.River:
                tilePrefab = prefab_river;
                break;
            case Tile.ShopType.Lake:
                tilePrefab = prefab_lake;
                break;
            case Tile.ShopType.Bush:
                tilePrefab = prefab_bush;
                break;
            case Tile.ShopType.Flowerbed:
                tilePrefab = prefab_flowerbed;
                break;
            default:
                return null;
        }

        // Létrehozzuk az új tile-t a kiválasztott prefab alapján
        return Instantiate(tilePrefab, position, Quaternion.identity);
    }



/*Helper function for river generating, if river moves diagonally adds another tile so it looks better*/
public (bool,Vector2Int) SmoothRiverEdges(Vector2Int prevTile, Vector2Int currentTile)
    {
        int dx = currentTile.x - prevTile.x;
        int dy = currentTile.y - prevTile.y;

        // If movement is diagonal (both x and y changed)
        if (dx != 0 && dy != 0)
        {
            // Pick one adjacent tile to fill (randomly choose horizontal or vertical)
            Vector2Int extraTile = UnityEngine.Random.Range(1,3) == 1
                ? new Vector2Int(prevTile.x, currentTile.y)  // Keep Y, adjust X
                : new Vector2Int(currentTile.x, prevTile.y); // Keep X, adjust Y

            // Ensure the tile is valid and not already a river tile or a lake
            if (!IsRiver(extraTile.x,extraTile.y) && GetTileFromPerlin(extraTile.x,extraTile.y).Item1 != 0)
            {
                return (true, extraTile);
            }
                
        }
        return (false,new Vector2Int(0, 0));
    }

    List<Vector2Int> GenerateRiver(Vector2Int start, int maxLength, int width)
    {
        List<Vector2Int> riverTiles = new List<Vector2Int>();
        Vector2Int currentPos = start;
        
        if (GetTileFromPerlin(start.x, start.y).Item1 == 0)
        {
            return riverTiles;
        }
        for (int i = 0; i < maxLength; i++)
        {
            riverTiles.Add(currentPos);

            // Get surrounding lower tiles
            List<Vector2Int> possibleNextTiles = GetLowerNeighbors(currentPos);
            Vector2Int prev = currentPos;
            
            // Pick a random lower tile to move to (adds some randomness)
            currentPos = possibleNextTiles[UnityEngine.Random.Range(0, possibleNextTiles.Count)];

            //Check for lakes override randomness and stop the river if found
            foreach (Vector2Int nextTile in possibleNextTiles)
            {
                if (GetTileFromPerlin(nextTile.x,nextTile.y).Item1 == 0)
                {
                    currentPos = nextTile;
                    riverTiles.Add(currentPos);
                    return riverTiles;
                }
            }

            (bool, Vector2Int) smoothEdge = SmoothRiverEdges(prev, currentPos);
            if (smoothEdge.Item1)
            {
                riverTiles.Add(smoothEdge.Item2);
            }
        }

        return riverTiles;
    }

    /*
     Helper function used for generating rivers. Gets the lower elevation tiles around it. If no such exists returns the minimally elevated tile.
     */
    List<Vector2Int> GetLowerNeighbors(Vector2Int start)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        float start_elevation = GetTileFromPerlin(start.x, start.y).Item2;

        Vector2Int min_elevated_tile = new Vector2Int(start.x -1, start.y - 1);
        float min_elevation = GetTileFromPerlin(min_elevated_tile.x, min_elevated_tile.y).Item2;

        //Check neighbors under and above
        for (int i = -1; i < 1;i+=2)
        {
            for (int j = -1; j < 2; j++)
            {
                (int, float) tile_elevation = GetTileFromPerlin(start.x + j, start.y + i);
                if (min_elevation > tile_elevation.Item2)
                {
                    min_elevation = tile_elevation.Item2;
                    min_elevated_tile = new Vector2Int(start.x + j, start.y + i);
                }
                
                if (tile_elevation.Item2 < start_elevation)
                {
                    neighbors.Add(new Vector2Int(start.x + j, start.y + i));
                }
            }
        }

        //Check neighbors to the sides
        for (int j = -1; j < 1; j += 2)
        {
            (int, float) tile_elevation = GetTileFromPerlin(start.x + j, start.y);
            if (min_elevation > tile_elevation.Item2)
            {
                min_elevation = tile_elevation.Item2;
                min_elevated_tile = new Vector2Int(start.x + j, start.y);
            }

            if (tile_elevation.Item2 < start_elevation)
            {
                neighbors.Add(new Vector2Int(start.x + j, start.y));
            }
        }
        
        if (neighbors.Count == 0)
        {
            neighbors.Add(min_elevated_tile);
        }

        return neighbors;

    }

    void GenerateMap()
    {
        /** Elevation based tile order. **/

        tileset = new Dictionary<int, GameObject>
        {
            { 0, prefab_lake },
            { 1, prefab_plains },
            { 2, prefab_tree },
            { 3, prefab_hills }
        };

        /** Create empty gameobjects for grouping tiles of the same type, ie
            trees **/

        tile_groups = new Dictionary<int, GameObject>();
        GameObject river_group = new GameObject("Rivers");
        river_group.transform.parent = gameObject.transform;
        river_group.transform.localPosition = new Vector3(0, 0, 0);
        tile_groups.Add(-1, river_group);

        GameObject misc_group = new GameObject("Misc");
        misc_group.transform.parent = gameObject.transform;
        misc_group.transform.localPosition = new Vector3(0, 0, 0);
        tile_groups.Add(-2, misc_group);

        foreach (KeyValuePair<int, GameObject> prefab_pair in tileset)
        {
            GameObject tile_group = new GameObject(prefab_pair.Value.name);
            tile_group.transform.parent = gameObject.transform;
            tile_group.transform.localPosition = new Vector3(0, 0, 0);
            tile_groups.Add(prefab_pair.Key, tile_group);
        }

        /*Generate rivers*/
        double area = (double)map_dimensions.y * map_dimensions.x;
        int maxRivers = (int) Math.Round(area * 0.001);
        int riverNumber = UnityEngine.Random.Range(maxRivers/2, maxRivers);
        Console.WriteLine(riverNumber);


        Vector2Int regionSize = new Vector2Int((int)Math.Round(map_dimensions.x / maxRivers), (int)Math.Round(map_dimensions.y / maxRivers));
        for (int i = 0; i < riverNumber; i++)
        {
            //Create regions so they dont spawn on eachother and are further apart

            Vector2Int currentRegion = new Vector2Int(i * regionSize.x, i * regionSize.y);
            int x = UnityEngine.Random.Range(currentRegion.x, currentRegion.x+regionSize.x);
            int y = UnityEngine.Random.Range(currentRegion.y, currentRegion.x+regionSize.y);

            int length = UnityEngine.Random.Range(10, 35);
            rivers.Add(GenerateRiver(new Vector2Int(x, y), length, 1));
        }
        


        /** Generate a 2D grid using the Perlin noise fuction, storing it as
            both raw ID values and tile gameobjects **/

        for (int x = 0; x < map_dimensions.x; x++)
        {
            noise_grid.Add(new List<int>());
            tile_grid.Add(new List<GameObject>());
 
            for(int y = 0; y < map_dimensions.y; y++)
            {
                
                if (!IsRiver(x, y))
                {
                    int tile_id = GetTileFromPerlin(x, y).Item1;
                    if (tile_id == 1) //If its plains have a chance to generate Misc underbushes
                    {
                        if (UnityEngine.Random.Range(1,10) == 1)
                        {
                            noise_grid[x].Add(-2);
                            CreateTile(-2, x, y, false, true);
                            continue;
                        }
                    }

                    noise_grid[x].Add(tile_id);
                    CreateTile(tile_id, x, y);
                }
                else
                {
                    noise_grid[x].Add(-1);
                    CreateTile(-1, x, y,true);
                }
                
            }
        }
    }

    bool IsRiver(int x, int y)
    {
        foreach (var river in rivers)
        {
            foreach(Vector2Int tile in river)
            {
                if (x ==  tile.x && y == tile.y) { return true; }
            }
        }

        return false;
    }
    
    public Tile GetTileAt(Vector2 pos)
    {
        int x = Mathf.FloorToInt(pos.x)+2;
        int y = Mathf.FloorToInt(pos.y)-1;
        GameObject gameObject = tile_grid[x][y];
        return gameObject.GetComponent<Tile>();
    }

    (int, float) GetTileFromPerlin(int x, int y)
    {
        /** Using a grid coordinate input, generate a Perlin noise value to be
            converted into a tile ID code. And return the raw elevation for further calculations. **/
 
        float raw_perlin = Mathf.PerlinNoise(
            (x - x_offset) / magnification,
            (y - y_offset) / magnification
        );
        float clamp_perlin = Mathf.Clamp01(raw_perlin);
        float scaled_perlin = clamp_perlin * tileset.Count;
 
        if(scaled_perlin == tileset.Count)
        {
            scaled_perlin = (tileset.Count - 1);
        }
        return (Mathf.FloorToInt(scaled_perlin), raw_perlin);
    }
 
    void CreateTile(int tile_id, int x, int y, bool isRiver = false, bool isMisc = false)
    {
        /** Creates a new tile using the type id code, group it with common
            tiles, set it's position and store the gameobject. River and flowers/bushes are generated seperately **/

        if (isMisc)
        {
            GameObject tile_prefab = UnityEngine.Random.Range(1,3) == 1 ? prefab_flowerbed : prefab_bush;
            GameObject tile_group = tile_groups[-2];
            GameObject tile = Instantiate(tile_prefab, tile_group.transform);

            tile.name = string.Format("tile_x{0}_y{1}", x, y);
            tile.transform.localPosition = new Vector3(x, y, 0);

            tile_grid[x].Add(tile);
            return;
        }

        if (!isRiver)
        {
            GameObject tile_prefab = tileset[tile_id];
            GameObject tile_group = tile_groups[tile_id];
            GameObject tile = Instantiate(tile_prefab, tile_group.transform);

            tile.name = string.Format("tile_x{0}_y{1}", x, y);
            tile.transform.localPosition = new Vector3(x, y, 0);

            tile_grid[x].Add(tile);
        }
        else
        {
            GameObject tile_prefab = prefab_river;
            GameObject tile_group = tile_groups[-1];
            GameObject tile = Instantiate(tile_prefab, tile_group.transform);

            tile.name = string.Format("tile_x{0}_y{1}", x, y);
            tile.transform.localPosition = new Vector3(x, y, 0);

            tile_grid[x].Add(tile);
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private SafariMap currentMap;
    public SafariMap safariMapPrefab;
    public GameObject animalPrefab;

    public Tile.TileType IsBuilding { get; set; } = Tile.TileType.Animal;

    // Singleton GameManager
    void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Ha van már térkép, akkor töröljük
        if (currentMap != null)
        {
            Destroy(currentMap.gameObject);
        }

        currentMap = safariMapPrefab;

        // Térkép generálása
        currentMap.CreateMap();
    }

    void Update()
    {
        // Csak akkor dolgozunk, ha az útépítési mód engedélyezett
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (IsBuilding == Tile.TileType.Road)
            {
                // Left click event
                if (Input.GetMouseButtonDown(0)) // Left click
                {
                    // Raycast from camera position
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

                    if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Tiles"))
                    {
                        // Debug message for the clicked object
                        Debug.Log("Clicked object: " + hit.collider.gameObject.name);

                        // Option: Change tile to road
                        Vector2 tilePosition = hit.collider.gameObject.transform.position;
                        currentMap.ChangeTileToRoad(tilePosition);
                    }
                }
            }
            else if ((int)IsBuilding < 3)
            {
                if (Input.GetMouseButtonDown(0)) // Left click
                {
                    // Raycast from camera position
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

                    if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Tiles"))
                    {
                        // Debug message for the clicked object
                        Debug.Log("Clicked object: " + hit.collider.gameObject.name);

                        // Option: Change tile nature
                        Vector2 tilePosition = hit.collider.gameObject.transform.position;
                        currentMap.ChangeTileNature(tilePosition, IsBuilding);
                    }
                }
            }
            else if (IsBuilding == Tile.TileType.Animal)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

                    if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Tiles"))
                    {
                        Vector2 tilePosition = hit.collider.gameObject.transform.position;

                        // Lehelyezés z = 0-ra
                        Vector3 spawnPos = new Vector3(tilePosition.x, tilePosition.y, 0f);

                        GameObject newAnimal = Instantiate(animalPrefab, spawnPos, Quaternion.identity);
                    }
                }
            }


        }

    }

    // Útépítési mód engedélyezése
    public void EnableRoadBuilding()
    {
        IsBuilding = Tile.TileType.Road;
    }

    // Útépítési mód letiltása
    public void DisableRoadBuilding()
    {
        IsBuilding = Tile.TileType.None;
    }
}

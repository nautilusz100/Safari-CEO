using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NavMeshPlus.Components;
using NavMeshPlus.Extensions;
using UnityEditor.AI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private SafariMap currentMap;
    public SafariMap safariMapPrefab;
    public GameObject animalPrefab;

    public GameObject navMesh;
    private NavMeshSurface navMeshSurface;

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
        navMeshSurface = navMesh.GetComponent<NavMeshSurface>();
        // Térkép generálása
        currentMap.CreateMap();
        // NavMesh generálása
        navMeshSurface.BuildNavMesh();
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
                        navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
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
                        navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);


                    }
                }
            }
            /* Not needed anymore because of draggable animals, left the code here for debugging reasons
             * else if (IsBuilding == Tile.TileType.Animal)
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
            }*/


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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NavMeshPlus.Components;
using NavMeshPlus.Extensions;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using System;
using System.Net;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private SafariMap currentMap;
    public SafariMap safariMapPrefab;
    [SerializeField]private int jeepCount = 0;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI visitorCount;
    [SerializeField] private Difficulty gameDifficulty;

    //public GameObject animalPrefab;
    public int EntryFee { get; set; }
    public int Visitors { get; set; } 

    public GameObject navMesh;
    private NavMeshSurface navMeshSurface;

    public Tile.ShopType IsBuilding { get; set; } = Tile.ShopType.Animal;

    // Singleton GameManager
    void Start()
    {

        EntryFee = 100;
        Visitors = 0;
        scoreText.text = "$" + EntryFee.ToString();
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

        //Game difficulty
        gameDifficulty = GameSettings.SelectedDifficulty;
        Debug.Log("Difficulty from static: " + gameDifficulty);


    }
    internal void NotifyTileFoodDepleted(Vector2Int pos)
    {
        currentMap.ReplaceTileWithPlains(pos);
    }
    void Update()
    {

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (IsBuilding == Tile.ShopType.Road)
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
        }
        else if (IsBuilding == Tile.ShopType.Jeep)
        {
            jeepCount++;
            IsBuilding = Tile.ShopType.None;
        }

        AttemptJeepSpawn();

    }

    private void AttemptJeepSpawn()
    {
        if(jeepCount > 0)
        {
            // Spawn a new Jeep
            GameObject jeep = Instantiate(currentMap.prefab_jeep, new Vector3(39,38,0), Quaternion.identity );
            jeepCount--;
        }
    }

    // Útépítési mód engedélyezése
    public void EnableRoadBuilding()
    {
        IsBuilding = Tile.ShopType.Road;
    }

    // Útépítési mód letiltása
    public void DisableRoadBuilding()
    {
        IsBuilding = Tile.ShopType.None;
    }
    public void PriceIncrease()
    { 
        Debug.Log("PriceIncrease");
        EntryFee++;
        scoreText.text = "$" + EntryFee.ToString();
    }
    public void PriceDecrease()
    {
        EntryFee--;
        scoreText.text = "$" + EntryFee.ToString();
    }

    public void UpdateVisitorCount()
    {
        visitorCount.text = Visitors.ToString();
    }

}

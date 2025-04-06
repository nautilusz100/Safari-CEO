using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private SafariMap currentMap;
    public SafariMap safariMapPrefab;
    public bool IsRoadBuildingMode { get; private set; } = true;

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

    // Update method for raycasting
    void Update()
    {
        // Csak akkor dolgozunk, ha az útépítési mód engedélyezett
        if (IsRoadBuildingMode)
        {
            // Bal kattintás esemény
            if (Input.GetMouseButtonDown(0)) // Bal kattintás
            {
                // Raycast a kamera pozíciójából
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

                if (hit.collider != null)
                {
                    // Debug üzenet a kattintott objektumról
                    Debug.Log("Kattintott objektum: " + hit.collider.gameObject.name);

                    // Opció: Ha rákattintunk, váltsunk tile-t
                    Vector2 tilePosition = hit.collider.gameObject.transform.position;
                    currentMap.ChangeTileToRoad(tilePosition);
                }
            }
        }
    }

    // Útépítési mód engedélyezése
    public void EnableRoadBuilding()
    {
        IsRoadBuildingMode = true;
    }

    // Útépítési mód letiltása
    public void DisableRoadBuilding()
    {
        IsRoadBuildingMode = false;
    }
}

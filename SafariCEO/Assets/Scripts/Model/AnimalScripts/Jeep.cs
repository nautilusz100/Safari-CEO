using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/*
 TODO:
    - Add logic so jeep doesnt move back and forth
 
 */
public class Jeep : MonoBehaviour
{
    private NavMeshAgent agent;
    private Vector2 safariExit;
    private float visionRadius = 1f;

    public Vector2 destinationTilePos;

    [SerializeField] private List<Tile> detectedRoads = new List<Tile>();


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.autoBraking = false;
        agent.avoidancePriority = Random.Range(1, 99);

        safariExit = new Vector2(42.5f, 39.5f);
        //agent.SetDestination(safariExit);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (agent.remainingDistance < 0.1f)
        {
            DetectRoads();
            Tile newDestinationTile = GetNewDestinationTile();
            destinationTilePos = newDestinationTile.transform.position;
            if (newDestinationTile != null)
            {
                Vector2 newDestination = AddJeepOffset(destinationTilePos);
                agent.SetDestination(newDestination);
            }
        }
    }

    private Vector2 AddJeepOffset(Vector2 position)
    {
        Vector2 offset = new Vector2(0, 0.5f);
        return position + offset;
    }

    private Tile GetNewDestinationTile()
    {
        if (detectedRoads.Count > 0)
        {
            int randomIndex = Random.Range(0, detectedRoads.Count);
            Tile selectedTile = detectedRoads[randomIndex];
            return selectedTile;
        }
        return null;
    }

    private void DetectRoads()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRadius);
        // Clear the list of detected roads
        detectedRoads.Clear();
        foreach (var hit in hits)
        {
            Tile tile = hit.GetComponent<Tile>();
            if (tile != null && !detectedRoads.Contains(tile) && tile.Type == Tile.ShopType.Road)
            {
                detectedRoads.Add(tile);
            }
        }
    }
}

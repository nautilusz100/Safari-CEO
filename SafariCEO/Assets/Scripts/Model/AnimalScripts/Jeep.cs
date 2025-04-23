using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/*
 TODO:
    - Add animal detection logic
    - Add post-ride satisfaction logic
    - Add fail-safe incase jeep gets stuck
 */
public class Jeep : MonoBehaviour
{
    private NavMeshAgent agent;
    private Vector2 safariEntry;
    private Vector2 safariExit;
    private float visionRadius = 0.65f;
    private bool isReturningHome = false;

    public Vector2 destinationTilePos;


    private Dictionary<Tile, int> traversedRoads = new Dictionary<Tile, int>();
    private List<Tile> detectedRoads = new List<Tile>();


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.autoBraking = false;
        agent.avoidancePriority = Random.Range(1, 99);

        safariEntry = new Vector2(37.5f, 39.5f);
        safariExit = new Vector2(42.5f, 39.5f);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isReturningHome) HasReachedHome();
        else
        {
            AtExit();
            Movement();
        }
    }

    private void Movement()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.1f)
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

    private void HasReachedHome()
    {
        bool isAtHome = Vector2.Distance(transform.position, safariEntry) < 0.25f;
        if (isAtHome) KillJeep();
    }
    private void AtExit()
    {
        bool isAtExit = Vector2.Distance(transform.position, safariExit) < 0.25f;
        if (isAtExit)
        {
            Debug.Log("Jeep has reached the exit");
            isReturningHome = true;
            agent.SetDestination(safariEntry);
        }
    }

    private Vector2 AddJeepOffset(Vector2 position)
    {
        Vector2 offset = new Vector2(Random.Range(-0.05f,0.05f), 0.5f);
        return position + offset;
    }

    private Tile GetNewDestinationTile()
    {
        if (detectedRoads.Count == 0)
        {
            KillJeep();
            return null;
        }

        List<Tile> curiousTileCandidates = new List<Tile>();
        int minTraversedCount = int.MaxValue;

        foreach (var tile in detectedRoads)
        {
            int count = traversedRoads.ContainsKey(tile) ? traversedRoads[tile] : 0;
            if (count < minTraversedCount)
            {
                minTraversedCount = count;
                curiousTileCandidates.Clear();
                curiousTileCandidates.Add(tile);
            }
            else if (count == minTraversedCount)
            {
                curiousTileCandidates.Add(tile);
            }
        }

        if (curiousTileCandidates.Count > 0)
        {
            return curiousTileCandidates[Random.Range(0, curiousTileCandidates.Count)];
        }

        KillJeep();
        return null;
    }

    private void KillJeep()
    {
        Debug.Log("Jeep has been killed");
        Destroy(gameObject);
    }


    private void AddToTraversedRoads(Tile tile)
    {

        if (!traversedRoads.ContainsKey(tile))
            traversedRoads[tile] = 0;

        traversedRoads[tile]++;
    }


    private void DetectRoads()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRadius);
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
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, visionRadius);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Tile tile = collision.GetComponent<Tile>();
        if (tile != null && tile.Type == Tile.ShopType.Road)
        {
            AddToTraversedRoads(tile);
        }
    }
}

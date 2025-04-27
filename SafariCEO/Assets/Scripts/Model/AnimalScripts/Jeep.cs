using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public class Jeep : MonoBehaviour
{
    // general
    private GameManager gameManager;
    private NavMeshAgent agent;
    private Vector2 safariEntry;
    private Vector2 safariExit;
    private bool isReturningHome = false;

    // Vision, Pathfinding
    public float roadVisionRadius = 0.65f;
    public float animalVisionRadius = 2f;
    public Vector2 destinationTilePos;

    private Dictionary<Tile, int> traversedRoads = new Dictionary<Tile, int>();
    [SerializeField]private List<Tile> detectedRoads = new List<Tile>();
    [SerializeField]private List<Animal> detectedAnimals = new List<Animal>();

    // stuck check
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float stuckCheckInterval = 2f; // Check every 2 seconds
    private float minDistanceDelta = 0.5f; // Must move at least this much


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.autoBraking = false;
        agent.avoidancePriority = Random.Range(1, 99);

        safariEntry = new Vector2(37.5f, 39.5f);
        safariExit = new Vector2(42.5f, 39.5f);
        

        InvokeRepeating("DetectAnimals", 0, 0.25f);
        InvokeRepeating("CheckIfStuck", stuckCheckInterval, stuckCheckInterval);
    }

    public void SetManager(GameManager manager)
    {
        gameManager = manager;
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

    private void CheckIfStuck()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        if (distanceMoved < minDistanceDelta)
        {
            stuckTimer += stuckCheckInterval;
            Debug.Log("Jeep might be stuck... (" + stuckTimer + "s)");

            if (stuckTimer >= 6f) // Stuck for 6 seconds total
            {
                Debug.LogWarning("Jeep confirmed stuck! Killing...");
                KillJeep();
            }
        }
        else
        {
            stuckTimer = 0f; // Reset timer if moved enough
        }

        lastPosition = transform.position;
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
        if (isAtHome)
        {
            int differentAnimals = DifferentAnimalCount();
            gameManager.JeepIsHome(detectedAnimals.Count, differentAnimals);
            KillJeep();
        }
    }

    private int DifferentAnimalCount()
    {
        int count = 0;
        List<string> animalTags = new List<string>();

        foreach (var animal in detectedAnimals)
        {
            if (animal != null)
            {
                var tag = animal.tag;
                if (animalTags.Contains(tag))
                {
                    count++;
                }
                else
                {
                    animalTags.Add(tag);
                }
            }
        }
        return count;
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
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, roadVisionRadius);
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

    private void DetectAnimals()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, animalVisionRadius);
        foreach (var hit in hits)
        {
            Animal animal = hit.GetComponent<Animal>();
            if (animal != null && !detectedAnimals.Contains(animal))
            {
                detectedAnimals.Add(animal);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, roadVisionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, animalVisionRadius);
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

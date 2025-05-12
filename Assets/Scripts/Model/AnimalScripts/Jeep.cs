using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model.Map;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Represents a Jeep in the game. The Jeep can move around the map, detect animals, and traverse roads.
/// </summary>
public class Jeep : MonoBehaviour
{
    // general
    private GameManager gameManager;
    private NavMeshAgent agent;
    private GameObject inspection;

    public int id = 0;

    // safari entry and exit points
    private Vector2 safariEntry;
    private Vector2 safariExit;
    private bool isReturningHome = false;
    public int tourists = 0;
    float baseSpeed = 2f;

    // Vision, Pathfinding
    public float roadVisionRadius = 0.65f;
    public float animalVisionRadius = 2f;
    public Vector2 destinationTilePos;

    // Road Traversing
    private Dictionary<Tile, int> traversedRoads = new Dictionary<Tile, int>();
    [SerializeField]private List<Tile> detectedRoads = new List<Tile>();
    [SerializeField]private List<Animal> detectedAnimals = new List<Animal>();

    // stuck check
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float stuckCheckInterval = 2f; // Check every 2 seconds
    private float minDistanceDelta = 0.5f; // Must move at least this much
    private float baseAcceleration = 2f;
    private float baseAngularSpeed = 60f;

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    void Start()
    {
        inspection = GameObject.FindWithTag("InspectionWindow");

        // Set the initial position of the Jeep
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.autoBraking = false;
        agent.avoidancePriority = Random.Range(1, 99);

        // Set the initial position of the Jeep
        safariEntry = new Vector2(37.5f, 39.5f);
        safariExit = new Vector2(42.5f, 39.5f);

        // Set the initial position of the Jeep
        InvokeRepeating("DetectAnimals", 0, 0.25f);
        InvokeRepeating("CheckIfStuck", stuckCheckInterval, stuckCheckInterval);
    }

    public void SetManager(GameManager manager)
    {
        gameManager = manager;
    }

    /// <summary>
    /// Updates the speed of the NavMeshAgent based on the current game speed.
    /// </summary>
    void UpdateAgentSpeed()
    {
        float sp = 0.5f;
        agent.speed = Mathf.Clamp((int)GameManager.Instance.CurrentGameSpeed * sp,0.5f,3.5f);
        agent.acceleration = baseAcceleration * (int)GameManager.Instance.CurrentGameSpeed;
        agent.angularSpeed = baseAngularSpeed * Mathf.Clamp((int)GameManager.Instance.CurrentGameSpeed, 1f, 3f);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAgentSpeed();
        if (isReturningHome) HasReachedHome();
        else
        {
            AtExit();
            Movement();
        }
    }

    private void OnMouseDown()
    {
        inspection.GetComponent<InspectionManager>().Display(tourists,id);
    }
    /// <summary>
    /// Checks if the Jeep is stuck by comparing its current position with the last recorded position.
    /// </summary>
    private void CheckIfStuck()
    {
        if (gameObject == null) return;
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        if (distanceMoved < minDistanceDelta)
        {
            stuckTimer += stuckCheckInterval;

            if (stuckTimer >= 6f) // Stuck for 6 seconds total
            {
                gameManager.totalJeepCount--;
                KillJeep();
            }
        }
        else
        {
            stuckTimer = 0f; // Reset timer if moved enough
        }
        if (this != null)
            lastPosition = transform.position;
    }

    /// <summary>
    /// Handles the movement of the Jeep. If the Jeep is not moving towards a destination, it will find a new tile to move to.
    /// </summary>
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
    /// <summary>
    /// Checks if the Jeep has reached home. If it has, it notifies the GameManager and destroys the Jeep.
    /// </summary>
    private void HasReachedHome()
    {
        bool isAtHome = Vector2.Distance(transform.position, safariEntry) < 0.5f;
        if (isAtHome)
        {
            int differentAnimals = DifferentAnimalCount();
            gameManager.JeepIsHome(detectedAnimals.Count, differentAnimals);
            KillJeep();
        }
    }

    private int DifferentAnimalCount()
    {
        return detectedAnimals
        .Where(animal => animal != null)
        .Select(animal => animal.tag)
        .Distinct()
        .Count();
    }


    private void AtExit()
    {
        bool isAtExit = Vector2.Distance(transform.position, safariExit) < 0.5f;
        if (isAtExit)
        {
            isReturningHome = true;
            tourists = 0;
            agent.SetDestination(safariEntry);
        }
    }

    private Vector2 AddJeepOffset(Vector2 position)
    {
        Vector2 offset = new Vector2(Random.Range(-0.05f,0.05f), 0.5f);
        return position + offset;
    }
    /// <summary>
    /// Gets a new destination tile for the Jeep to move to. It selects the tile with the least number of traversals.
    /// </summary>
    /// <returns></returns>
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

        #if UNITY_EDITOR
            // In EditMode, use DestroyImmediate to prevent the warning
            DestroyImmediate(gameObject);
        #else
            // In runtime, use Destroy normally
            Destroy(gameObject);
        #endif
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
    /// <summary>
    /// Detects animals within the animal vision radius and adds them to the detectedAnimals list.
    /// </summary>
    private void DetectAnimals()
    {
        if (gameObject == null) return;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Tile tile = collision.GetComponent<Tile>();
        if (tile != null && tile.Type == Tile.ShopType.Road)
        {
            AddToTraversedRoads(tile);
        }
    }
}

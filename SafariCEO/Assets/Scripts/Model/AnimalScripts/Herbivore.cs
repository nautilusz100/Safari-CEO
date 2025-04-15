using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using static Tile;

public class Herbivore : MonoBehaviour
{
    //for debugging
    private SpriteRenderer spriteRenderer;
    public Color normalColor = Color.white;
    public Color restingColor = Color.blue;
    public Color eatingColor = Color.green;
    public Color drinkingColor = Color.cyan;
    public Color searchingColor = Color.yellow;


    // Movement parameters
    public float moveRange = 100f;
    public float normalSpeed = 1f;
    public float slowedSpeedWater = 0.5f;
    public float slowedSpeedHills = 0.7f;

    // Perception
    public float visionRadius = 10f;

    // Survival needs
    public float hungerInterval = 120f;
    public float thirstInterval = 90f;
    public float starvationTime = 300f;
    public float dehydrationTime = 240f;
    public float eatingDuration = 10f;
    public float drinkingDuration = 5f;

    public float hungerTimer;
    public float thirstTimer;
    private float starvationTimer;
    private float dehydrationTimer;

    public float age = 0f;
    public float maxAge = 1000f;

    private int slowZoneCountWater = 0;
    private int slowZoneCountHills = 0;

    private NavMeshAgent agent;
    private List<Tile> exploredTiles = new List<Tile>();
    private Tile currentTarget = null;

    private enum State { Wander, SearchFood, SearchWater, Eating, Drinking, Rest }
    private State currentState = State.Wander;
    public bool beingAttacked = false;

    private void Start()
    {
        //debugging
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component missing!");
            enabled = false;
            return;
        }


        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component missing!");
            enabled = false;
            return;
        }

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = normalSpeed;

        // Initialize timers
        starvationTimer = starvationTime;
        dehydrationTimer = dehydrationTime;
        hungerTimer = hungerInterval;
        thirstTimer = thirstInterval;
        maxAge = Random.Range(maxAge * 0.8f, maxAge * 1.2f);

        InvokeRepeating("DecideNextAction", 0f, 2f);
        InvokeRepeating("UpdateVision", 0f, 0.5f);
    }

    private void Update()
    {
        if (!beingAttacked)
        {

            // Aging
            age += Time.deltaTime;
            if (age >= maxAge)
            {
                Die();
                return;
            }

            // Hunger system
            if (currentState != State.Eating && currentState != State.SearchFood)
            {
                hungerTimer -= Time.deltaTime;
            }

            // Thirst system
            if (currentState != State.Drinking && currentState != State.SearchWater)
            {
                thirstTimer -= Time.deltaTime;
            }

            // Starvation handling
            if (hungerTimer <= 0)
            {
                starvationTimer -= Time.deltaTime;
                if (starvationTimer <= 0)
                {
                    Die();
                }
            }
            else
            {
                starvationTimer = starvationTime;
            }

            // Dehydration handling
            if (thirstTimer <= 0)
            {
                dehydrationTimer -= Time.deltaTime;
                if (dehydrationTimer <= 0)
                {
                    Die();
                }
            }
            else
            {
                dehydrationTimer = dehydrationTime;
            }

            // Prioritize needs - if either hunger or thirst is critical, switch to searching
            if (hungerTimer <= hungerInterval * 0.3f || thirstTimer <= thirstInterval * 0.3f)
            {
                if (hungerTimer <= thirstTimer)
                {
                    currentState = State.SearchFood;
                }
                else
                {
                    currentState = State.SearchWater;
                }
            }
        }
        else
        {
            agent.isStopped = true;
        }
        
    }

    // Vision of the animal, adding tile to the explored list
    private void UpdateVision()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRadius);
        foreach (var hit in hits)
        {
            Tile tile = hit.GetComponent<Tile>();
            if (tile != null && !exploredTiles.Contains(tile))
            {
                exploredTiles.Add(tile);
            }
        }
    }

    private void DecideNextAction()
    {
        switch (currentState)
        {
            case State.Wander:
                spriteRenderer.color = normalColor;

                // Random chance to search for resources even if not desperate
                if (Random.value < 0.1f)
                {
                    if (hungerTimer < thirstTimer)
                    {
                        currentState = State.SearchFood;
                    }
                    else
                    {
                        currentState = State.SearchWater;
                    }
                }
                else
                {
                    MoveToRandomPosition();
                }
                break;

            case State.SearchFood:
                spriteRenderer.color = searchingColor;

                currentTarget = FindClosestFood();
                if (currentTarget != null)
                {
                    agent.SetDestination(currentTarget.transform.position);
                    StartCoroutine(CheckIfReachedDestination(State.Eating));
                }
                else
                {
                    MoveToRandomPosition();
                    Debug.Log(transform.name + " exploring new area for food.");
                }
                break;

            case State.SearchWater:
                spriteRenderer.color = searchingColor;

                currentTarget = FindClosestWater();
                if (currentTarget != null)
                {
                    agent.SetDestination(currentTarget.transform.position);
                    StartCoroutine(CheckIfReachedDestination(State.Drinking));
                }
                else
                {
                    MoveToRandomPosition();
                    Debug.Log(transform.name + " exploring new area for water.");
                }
                break;

            case State.Eating:
                // Handled by coroutine
                break;

            case State.Drinking:
                // Handled by coroutine
                break;

            case State.Rest:
                agent.isStopped = true;
                Invoke("ResumeWander", Random.Range(5f, 10f));
                break;
        }
    }

    private Tile FindClosestFood()
    {
        return exploredTiles
            .FindAll(t => t.Type == TileType.Tree || t.Type == TileType.Flowerbed || t.Type == TileType.Bush)
            .OrderBy(t => Vector3.Distance(transform.position, t.transform.position))
            .FirstOrDefault();
    }

    private Tile FindClosestWater()
    {
        return exploredTiles
            .FindAll(t => t.Type == TileType.Lake || t.Type == TileType.River)
            .OrderBy(t => Vector3.Distance(transform.position, t.transform.position))
            .FirstOrDefault();
    }

    private IEnumerator CheckIfReachedDestination(State nextState)
    {
        while (currentTarget != null &&
               (agent.pathPending ||
                agent.remainingDistance > agent.stoppingDistance))
        {
            yield return null;
        }

        if (currentTarget != null)
        {
            switch (nextState)
            {
                case State.Eating:
                    StartEating();
                    break;
                case State.Drinking:
                    StartDrinking();
                    break;
            }
        }
    }

    private void StartEating()
    {
        spriteRenderer.color = eatingColor;

        currentState = State.Eating;
        agent.isStopped = true;
        Debug.Log(transform.name + " is eating at " + currentTarget.name);
        Invoke("FinishEating", eatingDuration);
    }

    private void FinishEating()
    {
        spriteRenderer.color = restingColor;

        hungerTimer = hungerInterval;
        currentTarget = null;
        currentState = State.Rest;
        Debug.Log(transform.name + " finished eating and is now resting.");
    }

    private void StartDrinking()
    {
        spriteRenderer.color = drinkingColor;

        currentState = State.Drinking;
        agent.isStopped = true;
        Debug.Log(transform.name + " is drinking at " + currentTarget.name);
        Invoke("FinishDrinking", drinkingDuration);
    }

    private void FinishDrinking()
    {
        spriteRenderer.color = restingColor;

        thirstTimer = thirstInterval;
        currentTarget = null;
        currentState = State.Rest;
        Debug.Log(transform.name + " finished drinking and is now resting.");
    }

    private void ResumeWander()
    {
        agent.isStopped = false;
        currentState = State.Wander;

        spriteRenderer.color = normalColor;
    }

    private void MoveToRandomPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * moveRange;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, moveRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Tile tile = other.GetComponent<Tile>();
        if (tile == null) return;

        if (tile.Type == TileType.Lake || tile.Type == TileType.River)
        {
            slowZoneCountWater++;
            agent.speed = slowedSpeedWater;
        }
        else if (tile.Type == TileType.Hills)
        {
            slowZoneCountHills++;
            agent.speed = slowedSpeedHills;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Tile tile = other.GetComponent<Tile>();
        if (tile == null) return;

        if (tile.Type == TileType.Lake || tile.Type == TileType.River)
        {
            slowZoneCountWater--;
            if (slowZoneCountWater <= 0) agent.speed = normalSpeed;
        }
        else if (tile.Type == TileType.Hills)
        {
            slowZoneCountHills--;
            if (slowZoneCountHills <= 0) agent.speed = normalSpeed;
        }
    }

    private void Die()
    {
        Debug.Log($"{name} died at age {age}.");
        Destroy(gameObject);
    }
}
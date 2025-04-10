using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static Tile;

public class Carnivorous : MonoBehaviour
{
    // Debugging
    private SpriteRenderer spriteRenderer;
    public Color normalColor = Color.white;
    public Color restingColor = Color.blue;
    public Color eatingColor = Color.green;
    public Color drinkingColor = Color.cyan;
    public Color searchingColor = Color.yellow;
    public Color huntingColor = Color.red;

    // Movement parameters
    public float moveRange = 100f;
    public float normalSpeed = 3f;
    public float slowedSpeedWater = 0.5f;
    public float slowedSpeedHills = 0.7f;
    public float huntingSpeed = 5f;

    // Perception
    public float visionRadius = 15f;
    public float visionAngle = 120f;

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
    private Dictionary<Vector3, float> spottedPreyPositions = new Dictionary<Vector3, float>();

    private enum State { Wander, SearchFood, SearchWater, Eating, Drinking, Hunt, Rest }
    private State currentState = State.Wander;

    private Transform currentTargetAnimal;
    private Tile currentTargetTile;

    private void Start()
    {
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

        InvokeRepeating("DecideNextAction", 0f, 1f);
        InvokeRepeating("UpdateVision", 0f, 0.3f);
    }

    private void Update()
    {
        // Aging
        age += Time.deltaTime;
        if (age >= maxAge)
        {
            Die();
            return;
        }

        // Hunger system
        if (currentState != State.Eating && currentState != State.Hunt)
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
            if (starvationTimer <= 0) Die();
        }
        else
        {
            starvationTimer = starvationTime;
        }

        // Dehydration handling
        if (thirstTimer <= 0)
        {
            dehydrationTimer -= Time.deltaTime;
            if (dehydrationTimer <= 0) Die();
        }
        else
        {
            dehydrationTimer = dehydrationTime;
        }
    }

    private void UpdateVision()
    {
        // Új észlelések
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Herbivore"))
            {
                Vector3 preyPosition = hit.transform.position;
                Vector3 directionToPrey = preyPosition - transform.position;

                if (Vector3.Angle(transform.forward, directionToPrey) < visionAngle / 2)
                {
                    // Frissítjük vagy hozzáadjuk a pozíciót
                    if (spottedPreyPositions.ContainsKey(preyPosition))
                    {
                        spottedPreyPositions[preyPosition] = Time.time;
                    }
                    else
                    {
                        spottedPreyPositions.Add(preyPosition, Time.time);
                    }

                    Debug.Log($"Prey spotted at {preyPosition}");
                }
            }

            // Tile-ok kezelése marad
            Tile tile = hit.GetComponent<Tile>();
            if (tile != null && !exploredTiles.Contains(tile))
            {
                exploredTiles.Add(tile);
            }
        }
    }

    private void DecideNextAction()
    {
        Debug.Log($"Current state: {currentState}, Hunger: {hungerTimer}, Thirst: {thirstTimer}");
        // Critical needs take priority
        if (hungerTimer <= hungerInterval * 0.2f && spottedPreyPositions.Count > 0)
        {
            currentState = State.Hunt;
            HuntClosestPrey();
            return;
        }
        else if (thirstTimer <= thirstInterval * 0.2f)
        {
            currentState = State.SearchWater;
            SearchForWater();
            return;
        }
        else if (hungerTimer <= hungerInterval * 0.5f && spottedPreyPositions.Count > 0)
        {
            currentState = State.Hunt;
            HuntClosestPrey(); ;
            return;
        }

        // Normal behavior
        switch (currentState)
        {
            case State.Wander:
                spriteRenderer.color = normalColor;
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
                if (spottedPreyPositions.Count > 0)
                {
                    currentState = State.Hunt;
                    HuntClosestPrey();
                }
                else
                {
                    MoveToRandomPosition();
                }
                break;

            case State.SearchWater:
                spriteRenderer.color = searchingColor;
                SearchForWater();
                break;

            case State.Rest:
                spriteRenderer.color = restingColor;
                agent.isStopped = true;
                Invoke("ResumeWander", Random.Range(3f, 7f));
                break;
        }
    }

    private void HuntClosestPrey()
    {
        currentState = State.Hunt;
        spriteRenderer.color = huntingColor;
        agent.speed = huntingSpeed;

        // Kiválasztjuk a legközelebbi pozíciót
        Vector3 closestPosition = Vector3.zero;
        float closestDistance = Mathf.Infinity;

        foreach (var pos in spottedPreyPositions.Keys)
        {
            float dist = Vector3.Distance(transform.position, pos);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPosition = pos;
            }
        }

        if (closestDistance < Mathf.Infinity)
        {
            agent.SetDestination(closestPosition);
            StartCoroutine(CheckIfReachedHuntingPosition(closestPosition));
        }
        else
        {
            currentState = State.Wander;
        }
    }


    private IEnumerator CheckIfReachedHuntingPosition(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > agent.stoppingDistance)
        {
            // Közben ellenõrizzük, hogy látunk-e közelebbi prédát
            CheckForBetterPrey(ref targetPosition);
            yield return null;
        }

        // Ha ideértünk, de nem találtunk prédát (mert elmenekült)
        if (spottedPreyPositions.ContainsKey(targetPosition))
        {
            // Megnézzük, van-e élõ préda a közelben
            Collider2D[] hits = Physics2D.OverlapCircleAll(targetPosition, 2f);
            bool foundLivePrey = false;

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Herbivore"))
                {
                    foundLivePrey = true;
                    StartEating(hit.transform);
                    break;
                }
            }

            if (!foundLivePrey)
            {
                spottedPreyPositions.Remove(targetPosition);
                currentState = State.Wander;
            }
        }
        else
        {
            currentState = State.Wander;
        }
    }

    private void CheckForBetterPrey(ref Vector3 currentTarget)
    {
        float currentDistance = Vector3.Distance(transform.position, currentTarget);

        foreach (var pos in spottedPreyPositions.Keys)
        {
            float newDistance = Vector3.Distance(transform.position, pos);
            if (newDistance < currentDistance * 0.7f) // 30%-al közelebbi
            {
                currentTarget = pos;
                agent.SetDestination(pos);
                break;
            }
        }
    }

    private void StartEating(Transform prey)
    {
        if (prey != null)
        {
            Destroy(prey.gameObject);
            hungerTimer = hungerInterval;
        }
        currentState = State.Rest;

        Invoke("FinishEating", drinkingDuration);
    }
    private void SearchForWater()
    {
        currentTargetTile = exploredTiles
            .FindAll(t => t.Type == TileType.Lake || t.Type == TileType.River)
            .OrderBy(t => Vector3.Distance(transform.position, t.transform.position))
            .FirstOrDefault();

        if (currentTargetTile != null)
        {
            agent.SetDestination(currentTargetTile.transform.position);
            StartCoroutine(CheckIfReachedWater());
        }
        else
        {
            MoveToRandomPosition();
        }
    }

    private IEnumerator CheckIfReachedWater()
    {
        while (currentTargetTile != null &&
               (agent.pathPending ||
                agent.remainingDistance > agent.stoppingDistance))
        {
            yield return null;
        }

        if (currentTargetTile != null)
        {
            StartDrinking();
        }
        else
        {
            currentState = State.Wander;
        }
    }


    private void FinishEating()
    {
        currentState = State.Rest;
        spriteRenderer.color = restingColor;
        currentTargetAnimal = null;
        agent.speed = normalSpeed;
    }

    private void StartDrinking()
    {
        spriteRenderer.color = drinkingColor;
        currentState = State.Drinking;
        agent.isStopped = true;

        thirstTimer = thirstInterval;
        dehydrationTimer = dehydrationTime;

        Invoke("FinishDrinking", drinkingDuration);
    }

    private void FinishDrinking()
    {
        currentState = State.Rest;
        spriteRenderer.color = restingColor;
        currentTargetTile = null;
    }

    private void ResumeWander()
    {
        agent.isStopped = false;
        currentState = State.Wander;
        spriteRenderer.color = normalColor;
    }

    private void MoveToRandomPosition()
    {
        if (agent.remainingDistance < 2f)
        {
            Vector3 randomDirection = Random.insideUnitSphere * moveRange;
            randomDirection += transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, moveRange, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        Vector3 leftBound = Quaternion.Euler(0, -visionAngle / 2, 0) * transform.forward * visionRadius;
        Vector3 rightBound = Quaternion.Euler(0, visionAngle / 2, 0) * transform.forward * visionRadius;

        Gizmos.DrawLine(transform.position, transform.position + leftBound);
        Gizmos.DrawLine(transform.position, transform.position + rightBound);
    }
}
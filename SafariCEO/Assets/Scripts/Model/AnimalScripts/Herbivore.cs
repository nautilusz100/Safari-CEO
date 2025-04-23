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
    public float visionRadius = 5f;

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
    private enum State { Wander, SearchFood, SearchWater, Eating, Drinking, Rest, Mature, FindMate, Mating }

    private State currentState = State.Wander;
    public bool beingAttacked = false;

    //csak 1 rutin legyen mindig
    private Coroutine moveCoroutine;

    private GameObject currentTargetAnimal;
    private Dictionary<GameObject, Vector3> spottedMates = new Dictionary<GameObject, Vector3>();

    private float mateDuration = 5f;
    private float minMateAge = 80f;
    public float mateTimer = 0f;
    public float mateInterval = 100f;


    public GameObject zebraPrefab;
    public GameObject giraffePrefab;

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


        //matehez be kell állítani itt,  ami változik
        mateTimer = 0;
        age = 0;
        slowZoneCountHills = 0;
        slowZoneCountWater = 0;
        currentState = State.Wander;
        beingAttacked = false;
        currentTarget = null;
        exploredTiles = new List<Tile>();

        maxAge = Random.Range(maxAge * 0.8f, maxAge * 1.2f);

        moveCoroutine = null;

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
            mateTimer += Time.deltaTime;
            if (age >= minMateAge && mateTimer >= mateInterval)
            {
                currentState = State.Mature;
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
            if (hit.gameObject == this.gameObject)
                continue; //saját magát kihagyja
            Tile tile = hit.GetComponent<Tile>();
            if (tile != null && !exploredTiles.Contains(tile))
            {
                exploredTiles.Add(tile);
            }
            if (hit.CompareTag(this.tag))
            {
                GameObject mate = hit.gameObject;
                Vector3 matePosition = mate.transform.position;

                spottedMates[mate] = matePosition;
                Debug.Log($"Spotted mate: {mate.name} at {matePosition}");
            }
        }
    }

    private void DecideNextAction()
    {
        Debug.Log($"Current state: {currentState}");
        if (moveCoroutine != null)
            return;
        switch (currentState)
        {
            case State.Wander:
                this.agent.isStopped = false;
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
                this.agent.isStopped = false;
                spriteRenderer.color = searchingColor;

                currentTarget = FindClosestFood();
                if (currentTarget != null)
                {
                    agent.SetDestination(currentTarget.transform.position);
                    moveCoroutine = StartCoroutine(CheckIfReachedDestination(State.Eating));
                    Debug.Log("Corutin called food" + moveCoroutine);

                }
                else
                {
                    MoveToRandomPosition();
                    Debug.Log(transform.name + " exploring new area for food.");
                }
                break;

            case State.SearchWater:
                this.agent.isStopped = false;
                spriteRenderer.color = searchingColor;

                currentTarget = FindClosestWater();
                if (currentTarget != null)
                {
                    agent.SetDestination(currentTarget.transform.position);
                    moveCoroutine = StartCoroutine(CheckIfReachedDestination(State.Drinking));
                }
                else
                {
                    MoveToRandomPosition();
                    Debug.Log(transform.name + " exploring new area for water.");
                }
                break;
            case State.Mature:
                this.agent.isStopped = false;
                spriteRenderer.color = searchingColor;
                if (spottedMates.Count > 0) //ha látott már állatot
                {
                    currentState = State.FindMate;
                    FindClosestMate();
                    Debug.Log($"Searching for mate: {currentTargetAnimal?.name} at {currentTargetAnimal?.transform.position}");
                }
                else MoveToRandomPosition();
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
    private void FindClosestMate()
    {
        spriteRenderer.color = Color.magenta;
        spottedMates = spottedMates
        .Where(p => p.Key != null)
        .ToDictionary(p => p.Key, p => p.Key.transform.position);

        if (spottedMates.Count == 0)
        {
            currentState = State.Wander;
            return;
        }

        // Kiválasztjuk a legközelebbi prédát
        var closestMate = spottedMates
            .OrderBy(p => Vector3.Distance(transform.position, p.Value))
            .FirstOrDefault();

        // Ha a préda túl közel van (1 egységnél közelebb), mate
        if (closestMate.Key != null && Vector3.Distance(transform.position, closestMate.Value) < 1f)
        {
            Debug.Log($"Try mate  (by mate closest): {closestMate.Key.name} pos: {closestMate.Value}");
            StartMating(closestMate.Key.transform);
        }
        // Különben követjük
        else if (closestMate.Key != null)
        {
            agent.SetDestination(closestMate.Value);
        }
    
    }
    private void StartMating(Transform mate)
    {
        if (mate == null) return;

        Debug.Log($"Started mating with: {mate.name}");

        // Állapotbeállítások
        currentState = State.Mating;
        spriteRenderer.color = Color.magenta;

        // Ragadozó mozgás leállítása
        agent.isStopped = true;

        // Préda mozgás letiltása (ha van NavMeshAgent-je)
        NavMeshAgent mateAgent = mate.GetComponent<NavMeshAgent>();
        if (mateAgent != null) mateAgent.isStopped = true; //megállítja
        Debug.Log($"Stopped mate agent: {mate.name}");
        currentTargetAnimal = mate.gameObject; //valamiért újra be kell állítani, mert ha nincs akkor a préda nem tûnik el
        // 10 másodperc után vége az evésnek
        Invoke("FinishMating", mateDuration);
    }
    private void FinishMating()
    {
        if (currentTargetAnimal != null)
        {
            Debug.Log($"Finished mating: {currentTargetAnimal.name}");
            currentTargetAnimal.GetComponent<NavMeshAgent>().isStopped = false;

            // Véletlenszerû pozíció generálása a szülõ közelében (pl. 1-3 egység távolságra)
            Vector3 randomOffset = Random.insideUnitCircle * 2f; // 2 egység sugarú körben
            Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

            GameObject prefabToSpawn = null;

            switch (this.tag)
            {
                case "Zebra":
                    prefabToSpawn = zebraPrefab;
                    break;
                case "Giraffe":
                    prefabToSpawn = giraffePrefab;
                    break;
            }

            if (prefabToSpawn != null)
            {
                // Létrehozzuk az új állatot a megadott pozícióban
                Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Missing prefab for spawn!");
            }
        }

        // Visszaállítások
        agent.isStopped = false;
        currentState = State.Rest;
        mateTimer = 0f;
        currentTargetAnimal = null;
        spriteRenderer.color = restingColor;
    }


    private Tile FindClosestFood()
    {
        return exploredTiles
            .FindAll(t => t.Type == ShopType.Tree || t.Type == ShopType.Flowerbed || t.Type == ShopType.Bush)
            .OrderBy(t => Vector3.Distance(transform.position, t.transform.position))
            .FirstOrDefault();
    }

    private Tile FindClosestWater()
    {
        return exploredTiles
            .FindAll(t => t.Type == ShopType.Lake || t.Type == ShopType.River)
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

        float ageRatio = Mathf.Clamp01(age / maxAge); // 0 (young) to 1 (old)
        float hungerMultiplier = Mathf.Lerp(1f, 0.5f, ageRatio); // Old animals get less benefit
        hungerTimer = hungerInterval * hungerMultiplier;

        // --- TILE FOOD DEPLETION HERE ---
        Tile tile = currentTarget.GetComponent<Tile>();
        if (tile != null)
        {
            int baseFoodConsumption = 1;
            int foodToEat = Mathf.RoundToInt(Mathf.Lerp(baseFoodConsumption, baseFoodConsumption * 2f, ageRatio));
            tile.ConsumeFood(foodToEat);
            
        }

        currentTarget = null;
        currentState = State.Rest;
        Debug.Log(transform.name + " finished eating and is now resting.");
        moveCoroutine = null; // ha végzett a coroutine, akkor nullázzuk
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
        moveCoroutine = null; // ha végzett a coroutine, akkor nullázzuk
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

        if (tile.Type == ShopType.Lake || tile.Type == ShopType.River)
        {
            slowZoneCountWater++;
            agent.speed = slowedSpeedWater;
        }
        else if (tile.Type == ShopType.Hills)
        {
            slowZoneCountHills++;
            agent.speed = slowedSpeedHills;
            visionRadius = 10f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Tile tile = other.GetComponent<Tile>();
        if (tile == null) return;

        if (tile.Type == ShopType.Lake || tile.Type == ShopType.River)
        {
            slowZoneCountWater--;
            if (slowZoneCountWater <= 0) agent.speed = normalSpeed;
        }
        else if (tile.Type == ShopType.Hills)
        {
            slowZoneCountHills--;
            if (slowZoneCountHills <= 0)
            {
                agent.speed = normalSpeed;
                visionRadius = 5f; //visszaállítja a látótávolságot
            }
        }
    }

    private void Die()
    {
        Debug.Log($"{name} died at age {age}.");
        Destroy(gameObject);
    }
}
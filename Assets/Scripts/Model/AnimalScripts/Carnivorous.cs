using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model.Map;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using static Herbivore;
using static Tile;
using Random = UnityEngine.Random;

/// <summary>
/// Represents a carnivorous animal in the game.
/// </summary>

public class Carnivorous : Animal, IHasVision
{
    // Debugging
    public string uuid;

    // Movement parameters
    public float moveRange = 10f;
    public float herdOffset = 2f;
    public float huntingSpeed = 3f;
    public float normalSpeed = 1f;

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
    public float StarvationTimer {  get;}

    private float dehydrationTimer;
    public float DehydrationTimer { get; }
    public float age = 0f;
    public float maxAge = 1000f;

    public bool testMode = false;

    private NavMeshAgent agent;
    private SpriteRenderer spriteRenderer;
    private List<Tile> exploredTiles = new List<Tile>();
    private Dictionary<GameObject, Vector3> spottedPreyPositions = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Vector3> spottedMates = new Dictionary<GameObject, Vector3>();
    // State machine
    public enum StateCarnivore { Wander, SearchFood, SearchWater, Eating, Drinking, Hunt, Rest, Mature, FindMate, Mating }
    private StateCarnivore currentState = StateCarnivore.Wander;
    
    public StateCarnivore CurrentState { get { return currentState; }}

    // Actual target
    private GameObject currentTargetAnimal;
    private Tile currentTargetTile;

    // For mating
    private float mateDuration = 5f;
    private float minMateAge = 80f;
    public float mateTimer = 0f;
    public float mateInterval = 100f;
    public bool isMating;

    // prefabs
    public GameObject lionPrefab;
    public GameObject foxPrefab;

    // stuck check
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float stuckCheckInterval = 5f; // Check every 2 seconds
    private float minDistanceDelta = 0.5f; // Must move at least this much


    private void Start()
    {
        
        uuid = Guid.NewGuid().ToString();
        spriteRenderer = GetComponent<SpriteRenderer>();
        agent = testMode ? null : GetComponent<NavMeshAgent>();
        DietType = Diet.Carnivore;

        if (!testMode)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = (int)GameManager.Instance.CurrentGameSpeed * normalSpeed;
        }
        

        starvationTimer = starvationTime;
        dehydrationTimer = dehydrationTime;
        hungerTimer = hungerInterval;
        thirstTimer = thirstInterval;
        mateTimer = 0;
        maxAge = Random.Range(maxAge * 0.8f, maxAge * 1.2f);

        // Set the initial position of the animal
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Animals.Add(gameObject);
        }
        // Invoke methods for periodic updates
        InvokeRepeating(nameof(DecideNextAction), 0f, 1f);
        InvokeRepeating(nameof(UpdateVision), 0f, 0.3f);
        InvokeRepeating(nameof(CheckIfStuck), stuckCheckInterval, stuckCheckInterval);

    }


    private void OnClick()
    {
        GameObject inspection = GameObject.FindWithTag("InspectionWindow");
        if (inspection != null)
        {
            inspection.GetComponent<InspectionManager>().Display(gameObject);
        }
    }

    float baseAcceleration = 4f;
    float baseAngularSpeed = 120f;

    /// <summary>
    /// Updates the speed of the NavMeshAgent based on the current game speed.
    /// </summary>
    void UpdateAgentSpeed()
    {
        if (testMode) return;
        float sp = CurrentState == StateCarnivore.Hunt ? huntingSpeed : normalSpeed;
        agent.speed = (int)GameManager.Instance.CurrentGameSpeed * sp;
        agent.acceleration = baseAcceleration * (int)GameManager.Instance.CurrentGameSpeed;
        agent.angularSpeed = baseAngularSpeed * Mathf.Clamp((int)GameManager.Instance.CurrentGameSpeed, 1f, 3f);
    }

    /// <summary>
    /// Updates the animal's state and checks for user input.
    /// </summary>
    private void Update()
    {
        UpdateAgentSpeed();
        if (!testMode && Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, 1 << LayerMask.NameToLayer("Animals"));
            if (hit.collider != null && hit.transform == transform)
            {
                OnClick();
            }
        }

        SortByY();
        // Ha a prédát elkapta, akkor az állapotot át kell állítani, ez a agent coliderrel mûkdöik, TODO nem így kene sztem
        if (currentState == StateCarnivore.Hunt && currentTargetAnimal != null)
        {
            // Ha a préda elég közel van (3 egység)
            if (Vector3.Distance(transform.position, currentTargetAnimal.transform.position) < 1f)
            {
                StartEating(currentTargetAnimal.transform);
                
            }
        }

        float dtime = 0.1f;
        if (!testMode)
        { 
            dtime = GameManager.Instance.ScaledDeltaTime;
        }
        age += dtime;
        if (age >= maxAge) Die();

        if (currentState != StateCarnivore.Eating && currentState != StateCarnivore.Hunt)
            hungerTimer -= dtime;

        if (currentState != StateCarnivore.Drinking && currentState != StateCarnivore.SearchWater)
            thirstTimer -= dtime;

        if (hungerTimer <= 0)
        {
            starvationTimer -= dtime;
            if (starvationTimer <= 0) Die();
        }

        if (thirstTimer <= 0)
        {
            dehydrationTimer -= dtime;
            if (dehydrationTimer <= 0) Die();
        }
        
        if (currentState != StateCarnivore.Mating && currentState != StateCarnivore.FindMate)//Mate system
        {
            mateTimer += dtime;
        }
    }
    public void SetVisionRadius(float radius)//IHasVision interface implementáció
    {
        visionRadius = radius;
    }
    /// <summary>
    /// Updates the vision of the animal by checking for nearby objects within the vision radius.
    /// </summary>
    private void UpdateVision()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRadius);

        foreach (var hit in hits)
        {
            if (hit.gameObject == this.gameObject)
                continue; //saját magát kihagyja
            if (hit.CompareTag("Zebra") || hit.CompareTag("Giraffe") || hit.CompareTag("Giraffes") || hit.CompareTag("Zebras"))
            {
                GameObject prey = hit.gameObject;
                Vector3 preyPosition = prey.transform.position;

                spottedPreyPositions[prey] = preyPosition;
                
            }
            if (hit.CompareTag(this.tag))
            {
                GameObject mate = hit.gameObject;
                Vector3 matePosition = mate.transform.position;

                spottedMates[mate] = matePosition;
            }

            Tile tile = hit.GetComponent<Tile>();
            if (tile != null && !exploredTiles.Contains(tile))
                exploredTiles.Add(tile);
        }
    }
    /// <summary>
    /// Decides the next action for the animal based on its current state and needs.
    /// </summary>
    private void DecideNextAction()
    {
        if (hungerTimer <= hungerInterval * 0.2f && spottedPreyPositions.Count > 0)
        {
            currentState = StateCarnivore.Hunt;
            HuntClosestPrey();
            return;
        }
        else if (thirstTimer <= thirstInterval * 0.2f)
        {
            currentState = StateCarnivore.SearchWater;
            SearchForWater();
            return;
        }
        else if (mateTimer >= mateInterval && age >= minMateAge)
        {

            currentState = StateCarnivore.Mature;
            
        }

            switch (currentState)
            {
                case StateCarnivore.Wander:
                    MoveTowardsHerdOrRandom();
                    break;

                case StateCarnivore.SearchFood:
                    if (spottedPreyPositions.Count > 0) //ha látott már állatot
                    {
                        currentState = StateCarnivore.Hunt;
                        HuntClosestPrey();
                    }
                    else MoveTowardsHerdOrRandom();
                    break;

                case StateCarnivore.SearchWater:
                    SearchForWater();
                    break;

                case StateCarnivore.Rest:
                    if (!testMode) agent.isStopped = true;
                    Invoke(nameof(ResumeWander), Random.Range(3f, 7f));
                    break;
                case StateCarnivore.Mature:
                    if (spottedMates.Count > 0)
                    {
                        currentState = StateCarnivore.FindMate;
                        FindClosestMate();
                    }
                    else MoveTowardsHerdOrRandom();
                    break;
            }
    }
    /// <summary>
    /// Finds the closest mate within the vision radius and initiates mating if conditions are met.
    /// </summary>
    private void FindClosestMate()
    {
        spottedMates = spottedMates
        .Where(p => p.Key != null)
        .ToDictionary(p => p.Key, p => p.Key.transform.position);

        if (spottedMates.Count == 0)
        {
            currentState = StateCarnivore.Wander;
            return;
        }

        // Kiválasztjuk a legközelebbi prédát
        var closestMate = spottedMates
            .OrderBy(p => Vector3.Distance(transform.position, p.Value))
            .FirstOrDefault();

        // Ha a pr�da t�l k�zel van (1 egys�gn�l k�zelebb), mate
        if (closestMate.Key != null && Vector3.Distance(transform.position, closestMate.Value) < 1f &&
            age >= minMateAge && mateTimer >= mateInterval && !closestMate.Key.GetComponent<Carnivorous>().isMating) //ha mature
        {
            StartMating(closestMate.Key.transform);
        }
        else if (closestMate.Key != null)        // K�l�nben k�vetj�k
        {
            if (!testMode) agent.SetDestination(closestMate.Value);
        }
        //Debug.Log($"Closest mate: {closestMate.Key.name} at {closestMate.Value}");

    }
    /// <summary>
    /// Starts the mating process with the specified mate.
    /// </summary>
    /// <param name="mate"></param>
    private void StartMating(Transform mate)
    {
        if (mate == null) return;
        currentTargetAnimal = mate.gameObject;
        Carnivorous mateScript = mate.GetComponent<Carnivorous>();

        if (mateScript.mateTimer < mateInterval || mateScript.age < minMateAge || mateScript.isMating || mateScript == null)
        {
            MoveRandom(); // menjen kicsit arr�b hogy �j matet keressen majd
            return;
        }


        // Állapotbeállítások
        currentState = StateCarnivore.Mating;

        // kezdem�nyez� meg�ll�t�sa
        if (!testMode)
        {
            agent.isStopped = true;
            agent.ResetPath(); // <- Important! Cancel the current path completely.
            agent.velocity = Vector3.zero; // <- Immediately stop any leftover movement.
        }
        

        isMating = true;

        // T�rs mozg�s letilt�sa (ha van NavMeshAgent-je)
        if (mateScript != null && !testMode)
        {
            mateScript.isMating = true; //meg�ll�tja havert

            mateScript.agent.isStopped = true;
            mateScript.agent.ResetPath();
            mateScript.agent.velocity = Vector3.zero;

        }
        else return;


        // 5 m�sodperc ut�n v�ge az mate-nek
        currentTargetAnimal = mate.gameObject;
        Invoke(nameof(FinishMating), mateDuration);
    }
    /// <summary>
    /// Ends the mating process and spawns a new animal.
    /// </summary>
    private void FinishMating()
    {

        if (currentTargetAnimal != null)
        {
            // T�rs statok resetel�se
            Carnivorous targetScript = currentTargetAnimal.GetComponent<Carnivorous>();

            targetScript.isMating = false; //mate agent is stopped
            targetScript.mateTimer = 0f; //mate timer reset
            if (!testMode)targetScript.agent.isStopped = false; //mate agent is stopped
            targetScript.currentState = StateCarnivore.Rest; //mate agent is stopped

            Vector3 randomOffset = Random.insideUnitCircle * 1f;
            Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

            GameObject prefabToSpawn = null;

            switch (this.tag)
            {
                case "Lion":
                    prefabToSpawn = lionPrefab;
                    break;
                case "Fox":
                    prefabToSpawn = foxPrefab;
                    break;
            }

            if (prefabToSpawn != null)
            {
                // Létrehozzuk az új állatot a megadott pozícióban
                Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            }
        }

        // Kezdemm�nyez� statok resetel�se
        if (!testMode) agent.isStopped = false;
        isMating = false;
        currentState = StateCarnivore.Rest;
        mateTimer = 0f;
        currentTargetAnimal = null;
    }


    private void HuntClosestPrey()
    {

        // Frissítjük a prédák pozícióját (eltávolítjuk a null elemeket)
        spottedPreyPositions = spottedPreyPositions
            .Where(p => p.Key != null)
            .ToDictionary(p => p.Key, p => p.Key.transform.position);

        if (spottedPreyPositions.Count == 0)
        {
            currentState = StateCarnivore.Wander;
            return;
        }

        // Kiválasztjuk a legközelebbi prédát
        var closestPrey = spottedPreyPositions
            .OrderBy(p => Vector3.Distance(transform.position, p.Value))
            .FirstOrDefault();

        // Ha a préda túl közel van (3 egységnél közelebb), megtámadjuk
        if (closestPrey.Key != null && Vector3.Distance(transform.position, closestPrey.Value) < 1f)
        {
            StartEating(closestPrey.Key.transform);
        }
        // Különben követjük
        else if (closestPrey.Key != null && !testMode)
        {
            agent.SetDestination(closestPrey.Value);
        }
    }

    private void StartEating(Transform prey)
    {
        if (prey == null) return;

        // Állapotbeállítások
        currentState = StateCarnivore.Eating;

        Herbivore herbivore = prey.GetComponent<Herbivore>();
        if (!testMode)
        {
            agent.isStopped = true;

            // Préda mozgás letiltása (ha van NavMeshAgent-je)
            NavMeshAgent preyAgent = prey.GetComponent<NavMeshAgent>();
            if (preyAgent != null) herbivore.beingAttacked = true;
        }
        
        
        currentTargetAnimal = prey.gameObject; //valamiért újra be kell állítani, mert ha nincs akkor a préda nem tûnik el
        // 10 másodperc után vége az evésnek
        if (!testMode) Invoke(nameof(FinishEating), eatingDuration / (int)GameManager.Instance.CurrentGameSpeed);
    }
    private void FinishEating()
    {
        if (currentTargetAnimal != null)
        {
            Destroy(currentTargetAnimal); // Préda megsemmisítése
        }

        // Back to normal state
        hungerTimer = hungerInterval;
        currentTargetAnimal = null;
        currentState = StateCarnivore.Rest;
        if (!testMode)
            agent.isStopped = false;
    }

    private void SearchForWater()
    {
        currentTargetTile = exploredTiles
            .FindAll(t => t.Type == ShopType.Lake || t.Type == ShopType.River)
            .OrderBy(t => Vector3.Distance(transform.position, t.transform.position))
            .FirstOrDefault();

        if (currentTargetTile != null)
        {
            if (!testMode)
                agent.SetDestination(currentTargetTile.transform.position);
            StartCoroutine(CheckIfReachedWater());
        }
        else MoveTowardsHerdOrRandom();
    }
    /// <summary>
    /// Checks if the animal has reached the water tile.
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckIfReachedWater()
    {

        while (!testMode && currentTargetTile != null &&
               (agent.pathPending || agent.remainingDistance > agent.stoppingDistance))
        {
            yield return null;
        }

        if (currentTargetTile != null)
            StartDrinking();
        else
            currentState = StateCarnivore.Wander;
    }

    private void StartDrinking()
    {
        currentState = StateCarnivore.Drinking;
        if (!testMode)
            agent.isStopped = true;

        thirstTimer = thirstInterval;
        dehydrationTimer = dehydrationTime;

        currentState = StateCarnivore.Rest;
        currentTargetTile = null;
    }
    private void ResumeWander()
    {
        if (testMode)
        {
            currentState = StateCarnivore.Wander;
            return;
        }
        agent.isStopped = false;
        currentState = StateCarnivore.Wander;
    }
    /// <summary>
    /// Moves the animal towards the closest herd member or a random position if no herd member is found.
    /// </summary>
    private void MoveTowardsHerdOrRandom()
    {

        if (!testMode && agent.remainingDistance < 2f) //ez uj
        {
            Carnivorous oldestMate = FindOldestSeenMate();
            Vector3 destination = Vector3.zero;

            // if oldest mate found, move towards them
            if (oldestMate != null && age < oldestMate.age)
            {
                Vector3 matePosition = oldestMate.transform.position;
                Vector3 randomOffset = Random.insideUnitCircle * herdOffset;

                destination = matePosition + randomOffset;

                //Debug.Log($"Moving towards mate: {oldestMate.name} at {matePosition}");
                if (NavMesh.SamplePosition(destination, out NavMeshHit hit, herdOffset, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
            else
            {
                MoveRandom();
            }
        }
    }
    /// <summary>
    /// Finds the oldest seen mate from the spotted mates list.
    /// </summary>
    /// <returns></returns>
    private Carnivorous FindOldestSeenMate()
    {
        GameObject oldestMate = null;
        float oldestAge = 0f;
        foreach (var mate in spottedMates.Keys)
        {
            Carnivorous mateScript = mate.GetComponent<Carnivorous>();
            if (mateScript != null && mateScript.age > oldestAge)
            {
                oldestAge = mateScript.age;
                oldestMate = mate;
            }
        }
        if (oldestMate == null)
        {
            return null;
        }
        return oldestMate.GetComponent<Carnivorous>();
    }
    public void MoveRandom()
    {

        Vector3 destination = Vector3.zero;
        destination = Random.insideUnitSphere * moveRange;
        destination += transform.position;

        if (testMode) transform.position = destination;

        if (!testMode && NavMesh.SamplePosition(destination, out NavMeshHit hit, moveRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void CheckIfStuck()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        if (distanceMoved < minDistanceDelta)
        {
            stuckTimer += stuckCheckInterval;

            if (stuckTimer >= 20f) // Stuck for 6 seconds total
            {
                MoveRandom();
            }
        }
        else
        {
            stuckTimer = 0f; // Reset timer if moved enough
        }

        lastPosition = transform.position;
    }

    private void Die()
    {
        //for saving
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Animals.Remove(gameObject);
        }
        Destroy(gameObject);

    }
}

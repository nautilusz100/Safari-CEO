using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using static Tile;
using System;
using Random = UnityEngine.Random;
using UnityEngine.EventSystems;
using Assets.Scripts.Model.Map;
using static GameManager;

/// <summary>
///  Herbivore class representing herbivorous animals in the game.
/// </summary>
public class Herbivore : Animal, IHasVision
{
    public string uuid;

    // Movement parameters
    public float moveRange = 10f;
    public float herdOffset = 2f;
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

    // State timers
    public float hungerTimer;
    public float thirstTimer;
    private float starvationTimer;
    private float dehydrationTimer;

    // State timers for hunger and thirst
    public float StarvationTimer { get; }
    public float DehydrationTimer { get; }

    // Age parameters
    public float age = 0f;
    public float maxAge = 1000f;

    // State parameters
    private NavMeshAgent agent;
    private List<Tile> exploredTiles = new List<Tile>();
    private Tile currentTarget = null;
    public enum StateHerbivore { Wander, SearchFood, SearchWater, Eating, Drinking, Rest, Mature, FindMate, Mating }

    [SerializeField]private StateHerbivore currentState = StateHerbivore.Wander;

    public StateHerbivore CurrentState { get { return currentState; } }
    public bool beingAttacked = false;

    // Debugging
    private Coroutine moveCoroutine;

    // Animal detection
    private GameObject currentTargetAnimal;
    private Dictionary<GameObject, Vector3> spottedMates = new Dictionary<GameObject, Vector3>();

    // Mating parameters
    private float mateDuration = 5f;
    private float minMateAge = 80f;
    public float mateTimer = 0f;
    public float mateInterval = 100f;
    public bool isMating;

    // Prefabs
    public GameObject zebraPrefab;
    public GameObject giraffePrefab;
    public Sprite herdSprite;

    // stuck check
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float stuckCheckInterval = 5f; // Check every 2 seconds
    private float minDistanceDelta = 0.5f; // Must move at least this much

    float baseAcceleration = 4f;
    float baseAngularSpeed = 120f;

    /// <summary>
    /// Initializes the herbivore's properties and sets up the NavMeshAgent.
    /// </summary>
    private void Start()
    {
        //debugging
        uuid = Guid.NewGuid().ToString();
        DietType = Diet.Herbivore;

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            enabled = false;
            return;
        }

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = (int)GameManager.Instance.CurrentGameSpeed * normalSpeed;
        agent.avoidancePriority = Random.Range(1, 99); 

        // Initialize timers
        starvationTimer = starvationTime;
        dehydrationTimer = dehydrationTime;
        hungerTimer = hungerInterval;
        thirstTimer = thirstInterval;

        //matehez be kell állítani itt,  ami változik

        mateTimer = 0;
        age = 0;
        currentState = StateHerbivore.Wander;
        beingAttacked = false;
        isMating = false;
        currentTarget = null;
        exploredTiles = new List<Tile>();

        maxAge = Random.Range(maxAge * 0.8f, maxAge * 1.2f);

        moveCoroutine = null;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Animals.Add(gameObject);
        }

        // Update vision radius
        InvokeRepeating(nameof(UpdateVision), 0f, 0.5f);
        // Check if stuck
        InvokeRepeating(nameof(CheckIfStuck), stuckCheckInterval, stuckCheckInterval);
    }

    /// <summary>
    /// Handles the click event on the herbivore. Opens the inspection window.
    /// </summary>
    private void OnClick()
    {
        GameObject inspection = GameObject.FindWithTag("InspectionWindow");
        if (inspection != null)
        {
            inspection.GetComponent<InspectionManager>().Display(gameObject);
        }
    }
    /// <summary>
    /// Updates the speed and acceleration of the NavMeshAgent based on the current game speed.
    /// </summary>
    void UpdateAgentSpeed()
    {
        float sp = normalSpeed;
        agent.speed = (int)GameManager.Instance.CurrentGameSpeed * sp;
        agent.acceleration = baseAcceleration * (int)GameManager.Instance.CurrentGameSpeed;
        agent.angularSpeed = baseAngularSpeed*Mathf.Clamp((int)GameManager.Instance.CurrentGameSpeed,1f,3f);
    }
    /// <summary>
    ///  Updates the herbivore's state and checks for user input.
    /// </summary>
    private void Update()
    {
        SortByY();

        UpdateAgentSpeed();
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, 1 << LayerMask.NameToLayer("Animals"));
            if (hit.collider != null && hit.transform == transform)
            {
                OnClick();
            }
        }

        if (!beingAttacked && !isMating)
        {
            // Aging
            age += GameManager.Instance.ScaledDeltaTime;  

            if (age >= maxAge)
            {
                Die();
                return;
            }


            // Prioritize needs - if either hunger or thirst is critical, switch to searching

            if (currentState != StateHerbivore.Mating && currentState != StateHerbivore.FindMate)//Mate system
            {
                mateTimer += GameManager.Instance.ScaledDeltaTime;
            }

            if (currentState != StateHerbivore.Eating && currentState != StateHerbivore.SearchFood)            // Hunger system
            {
                hungerTimer -= GameManager.Instance.ScaledDeltaTime;
            }

            if (currentState != StateHerbivore.Drinking && currentState != StateHerbivore.SearchWater)// Thirst system
            {
                thirstTimer -= GameManager.Instance.ScaledDeltaTime;
            }
            

            // Starvation handling

            if (hungerTimer <= 0)
            {
                starvationTimer -= GameManager.Instance.ScaledDeltaTime;
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
                dehydrationTimer -= GameManager.Instance.ScaledDeltaTime;
                if (dehydrationTimer <= 0)
                {
                    Die();
                }
            }
            else
            {
                dehydrationTimer = dehydrationTime;
            }
            DecideNextAction();

        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();
        }


    }

    /// <summary>
    /// Sets the vision radius of the herbivore. This is used to determine how far the herbivore can see and interact with other objects in the game world.
    /// </summary>
    /// <param name="radius"></param>
    public void SetVisionRadius(float radius)//IHasVision interface implementáció
    {
        visionRadius = radius;
    }
    /// <summary>
    /// Checks if the herbivore is stuck in its current position. If it hasn't moved significantly for a certain period, it will move randomly.
    /// </summary>
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

    // Vision of the animal, adding tile to the explored list
    private void UpdateVision()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRadius);
        spottedMates = spottedMates.Where(p => p.Key != null).ToDictionary(p => p.Key, p => p.Key.transform.position); //ha m�r nem l�tezik a j�t�kban, akkor t�rli a sz�t�rb�l
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
            }
        }
    }
    /// <summary>
    /// Decides the next action for the herbivore based on its current state and needs.
    /// </summary>
    private void DecideNextAction()
    {
        if (!agent.isActiveAndEnabled) return;
        // Set State
        if (moveCoroutine != null)
            return;
        if (hungerTimer <= hungerInterval * 0.3f || thirstTimer <= thirstInterval * 0.3f)
        {
                if (hungerTimer <= thirstTimer)
                {
                    currentState = StateHerbivore.SearchFood;
                }
                else
                {
                    currentState = StateHerbivore.SearchWater;
                }
        }
        else if (age >= minMateAge && mateTimer >= mateInterval) //mating 
        {
            currentState = StateHerbivore.Mature;
        }

        // Action based on state
        switch (currentState)
        {
            case StateHerbivore.Wander:
                this.agent.isStopped = false;

                MoveTowardsHerdOrRandom();
                break;
            
            case StateHerbivore.SearchFood:
                this.agent.isStopped = false;
                currentTarget = FindClosestFood();
                if (currentTarget != null)
                {
                    Vector3 randomOffset = Random.insideUnitCircle;
                    agent.SetDestination(currentTarget.transform.position + randomOffset);
                    if (moveCoroutine != null) // ha m�r fut egy coroutine, akkor le�ll�tja
                    {
                        StopCoroutine(moveCoroutine);
                        moveCoroutine = null;
                    }
                    moveCoroutine = StartCoroutine(CheckIfReachedDestination(StateHerbivore.Eating));
                }
                else
                {
                    MoveTowardsHerdOrRandom();
                }
                break;

            case StateHerbivore.SearchWater:
                this.agent.isStopped = false;
                currentTarget = FindClosestWater();
                if (currentTarget != null)
                {
                    Vector3 randomOffset = Random.insideUnitCircle;
                    agent.SetDestination(currentTarget.transform.position + randomOffset);
                    if (moveCoroutine != null)
                    {
                        StopCoroutine(moveCoroutine);
                        moveCoroutine = null;
                    }
                    moveCoroutine = StartCoroutine(CheckIfReachedDestination(StateHerbivore.Drinking));
                }
                else
                {
                    MoveTowardsHerdOrRandom();
                }
                break;
            case StateHerbivore.Mature:
                this.agent.isStopped = false;
                if (spottedMates.Count > 0) //ha látott már állatot
                {
                    currentState = StateHerbivore.FindMate;
                    FindClosestMate();
                }
                else MoveTowardsHerdOrRandom();
                break;
            case StateHerbivore.Eating:
                // Handled by coroutine
                break;

            case StateHerbivore.Drinking:
                // Handled by coroutine
                break;

            case StateHerbivore.Rest:
                agent.isStopped = true;
                Invoke(nameof(ResumeWander), Random.Range(5f, 10f)/(int)GameManager.Instance.CurrentGameSpeed);
                break;
        }
    }
    /// <summary>
    ///  Finds the closest mate within the herbivore's vision radius. If a mate is found, it will move towards them.
    /// </summary>
    private void FindClosestMate()
    {
        spottedMates = spottedMates
        .Where(p => p.Key != null)
        .ToDictionary(p => p.Key, p => p.Key.transform.position);

        if (spottedMates.Count == 0)
        {
            currentState = StateHerbivore.Wander;
            return;
        }

        // Kiválasztjuk a legközelebbi prédát
        var closestMate = spottedMates
            .OrderBy(p => Vector3.Distance(transform.position, p.Value))
            .FirstOrDefault();

        // Ha a pr�da t�l k�zel van (1 egys�gn�l k�zelebb), mate
        if (closestMate.Key != null && Vector3.Distance(transform.position, closestMate.Value) < 1f &&
            age >= minMateAge && mateTimer >= mateInterval && !closestMate.Key.GetComponent<Herbivore>().isMating) //ha mature
        {
            StartMating(closestMate.Key.transform);
        }
        else if (closestMate.Key != null)        // K�l�nben k�vetj�k
        {
            agent.SetDestination(closestMate.Value);
        }

    }
    /// <summary>
    /// Starts the mating process with another herbivore. If the mate is not ready, it will move randomly.
    /// </summary>
    /// <param name="mate"></param>
    private void StartMating(Transform mate)
    {
        if (mate == null) return;
        currentTargetAnimal = mate.gameObject; 
        Herbivore mateScript = mate.GetComponent<Herbivore>();

        if (mateScript.mateTimer < mateInterval || mateScript.age < minMateAge || mateScript.isMating || mateScript == null)
        {
            MoveRandom(); // menjen kicsit arr�b hogy �j matet keressen majd
            return;
        }


        // Állapotbeállítások
        currentState = StateHerbivore.Mating;

        // kezdem�nyez� meg�ll�t�sa
        agent.isStopped = true;
        agent.ResetPath(); // <- Important! Cancel the current path completely.
        agent.velocity = Vector3.zero; // <- Immediately stop any leftover movement.

        isMating = true;

        // T�rs mozg�s letilt�sa (ha van NavMeshAgent-je)
        if (mateScript != null)
        {
            mateScript.isMating = true; //meg�ll�tja havert

            mateScript.agent.isStopped = true;
            mateScript.agent.ResetPath();
            mateScript.agent.velocity = Vector3.zero;

        }
        else return;

        // 5 m�sodperc ut�n v�ge az mate-nek
        currentTargetAnimal = mate.gameObject;
        Invoke(nameof(FinishMating), mateDuration / (int)GameManager.Instance.CurrentGameSpeed);
    }
    /// <summary>
    /// Finishes the mating process. Resets the state of both animals and spawns a new animal.
    /// </summary>
    private void FinishMating()
    {
        if (currentTargetAnimal != null)
        {
            // T�rs statok resetel�se
            Herbivore targetScript = currentTargetAnimal.GetComponent<Herbivore>();

            targetScript.isMating = false; //mate agent is stopped
            targetScript.mateTimer = 0f; //mate timer reset
            targetScript.agent.isStopped = false; //mate agent is stopped
            targetScript.currentState = StateHerbivore.Rest; //mate agent is stopped

            Vector3 randomOffset = Random.insideUnitCircle * 1f;
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
        }

        // Kezdemm�nyez� statok resetel�se
        agent.isStopped = false;
        isMating = false;
        currentState = StateHerbivore.Rest;
        mateTimer = 0f;
        currentTargetAnimal = null;



    }
    private Tile FindClosestFood()
    {
        return exploredTiles
            .FindAll(t => t != null)
            .FindAll(t => t.Type == ShopType.Tree || t.Type == ShopType.Flowerbed || t.Type == ShopType.Bush)
            .OrderBy(t => Vector3.Distance(transform.position, t.transform.position))
            .FirstOrDefault();
    }
    private Tile FindClosestWater()
    {
        return exploredTiles
            .FindAll(t => t != null)
            .FindAll(t => t.Type == ShopType.Lake || t.Type == ShopType.River)
            .OrderBy(t => Vector3.Distance(transform.position, t.transform.position))
            .FirstOrDefault();
    }
    /// <summary>
    ///   Checks if the herbivore has reached its destination. If it has, it will start eating or drinking.
    /// </summary>
    /// <param name="nextState"></param>
    /// <returns></returns>
    private IEnumerator CheckIfReachedDestination(StateHerbivore nextState)
    {
        if (!agent.isActiveAndEnabled) yield break;
        while (currentTarget != null &&(agent.pathPending ||agent.remainingDistance > agent.stoppingDistance))
        {
            if (currentTarget == null) //
            {
                currentTarget = null;
                currentState = nextState == StateHerbivore.Eating ? StateHerbivore.SearchFood : StateHerbivore.SearchWater;
                agent.isStopped = false;
                moveCoroutine = null;
                yield break;
            }
            yield return null; // wait for the next frame
        }

        if (currentTarget != null)
        {
            switch (nextState)
            {
                case StateHerbivore.Eating:
                    StartEating();
                    break;
                case StateHerbivore.Drinking:
                    StartDrinking();
                    break;
            }
        }
        
    }
    /// <summary>
    /// Starts the eating process. Sets the current state to Eating and stops the agent.
    /// </summary>
    private void StartEating()
    {
        if (currentTarget == null) // safe check, hogy m�g l�tezik-e a tile
        {
            currentTarget = null;
            currentState = StateHerbivore.SearchFood;
            agent.isStopped = false;
            moveCoroutine = null;
            return;
        }
        currentState = StateHerbivore.Eating;
        agent.isStopped = true;
        Invoke(nameof(FinishEating), eatingDuration/(int)GameManager.Instance.CurrentGameSpeed);
    }
    /// <summary>
    /// Finishes the eating process. Resets the hunger timer and depletes the food from the tile.
    /// </summary>
    private void FinishEating()
    {

        float ageRatio = Mathf.Clamp01(age / maxAge); // 0 (young) to 1 (old)
        float hungerMultiplier = Mathf.Lerp(1f, 0.5f, ageRatio); // Old animals get less benefit
        hungerTimer = hungerInterval * hungerMultiplier;

        Tile tile = currentTarget.GetComponent<Tile>();
        if (tile != null)
        {
            int baseFoodConsumption = 1;
            int foodToEat = Mathf.RoundToInt(Mathf.Lerp(baseFoodConsumption, baseFoodConsumption * 2f, ageRatio));
            tile.ConsumeFood(foodToEat);
            
        }

        currentTarget = null;
        currentState = StateHerbivore.Rest;
        moveCoroutine = null; // if the coroutine is finished, set it to null
    }


    private void StartDrinking()
    {

        currentState = StateHerbivore.Drinking;
        agent.isStopped = true;
        Invoke(nameof(FinishDrinking), drinkingDuration/(int)GameManager.Instance.CurrentGameSpeed);
    }

    private void FinishDrinking()
    {

        thirstTimer = thirstInterval;
        currentTarget = null;
        currentState = StateHerbivore.Rest;
        moveCoroutine = null; // if the coroutine is finished, set it to null
    }

    private void ResumeWander()
    {
        agent.isStopped = false;
        currentState = StateHerbivore.Wander;
    }

    /// <summary>
    /// Moves the herbivore towards the closest herd member or randomly if no herd member is found.
    /// </summary>
    private void MoveTowardsHerdOrRandom()
    {
        if (!agent.isActiveAndEnabled) return;
        if (agent.remainingDistance < 2f) //ez uj
        {
            Herbivore oldestMate = FindOldestSeenMate();
            Vector3 destination = Vector3.zero;

            // if oldest mate found, move towards them
            if (oldestMate != null && age < oldestMate.age)
            {
                Vector3 matePosition = oldestMate.transform.position;
                Vector3 randomOffset = Random.insideUnitCircle * herdOffset;

                destination = matePosition + randomOffset;

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

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void MoveRandom()
    {
        Vector3 destination = Vector3.zero;
        destination = Random.insideUnitSphere * moveRange;
        destination += transform.position;

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, moveRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }


    private Herbivore FindOldestSeenMate()
    {
        GameObject oldestMate = null;
        float oldestAge = 0f;
        foreach (var mate in spottedMates.Keys)
        {
            if (mate == null) continue;
            Herbivore mateScript = mate.GetComponent<Herbivore>();
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
        return oldestMate.GetComponent<Herbivore>();
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
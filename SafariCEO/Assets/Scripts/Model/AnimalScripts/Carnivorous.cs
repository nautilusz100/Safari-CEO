using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model.Map;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using static Tile;
using Random = UnityEngine.Random;

public class Carnivorous : Animal, IHasVision
{
    // Debugging
    private SpriteRenderer spriteRenderer;
    public Color normalColor = Color.white;
    public Color restingColor = Color.blue;
    public Color eatingColor = Color.green;
    public Color drinkingColor = Color.cyan;
    public Color searchingColor = Color.yellow;
    public Color huntingColor = Color.red;
    public string uuid;

    // Movement parameters
    public float moveRange = 100f;
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

    private NavMeshAgent agent;
    private List<Tile> exploredTiles = new List<Tile>();
    private Dictionary<GameObject, Vector3> spottedPreyPositions = new Dictionary<GameObject, Vector3>();

    public enum StateCarnivore { Wander, SearchFood, SearchWater, Eating, Drinking, Hunt, Rest }
    private StateCarnivore currentState = StateCarnivore.Wander;

    public StateCarnivore CurrentState { get { return currentState; }}

    //épen aktuális célpontok
    private GameObject currentTargetAnimal;
    private Tile currentTargetTile;
    private void Start()
    {
        uuid = Guid.NewGuid().ToString();
        spriteRenderer = GetComponent<SpriteRenderer>();
        agent = GetComponent<NavMeshAgent>();
        diet = Diet.Carnivore;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = normalSpeed;

        starvationTimer = starvationTime;
        dehydrationTimer = dehydrationTime;
        hungerTimer = hungerInterval;
        thirstTimer = thirstInterval;
        maxAge = Random.Range(maxAge * 0.8f, maxAge * 1.2f);

        InvokeRepeating(nameof(DecideNextAction), 0f, 1f);
        InvokeRepeating(nameof(UpdateVision), 0f, 0.3f);

    }

    private void OnClick()
    {
        Debug.Log("Clicked");
        GameObject inspection = GameObject.FindWithTag("InspectionWindow");
        if (inspection != null)
        {
            inspection.GetComponent<InspectionManager>().Display(gameObject);
        }
    }

    private void Update()
    {

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
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
            Debug.Log($"Distance to prey: {Vector3.Distance(transform.position, currentTargetAnimal.transform.position)}");
            if (Vector3.Distance(transform.position, currentTargetAnimal.transform.position) < 1f)
            {
                Debug.Log("Prey caught via distance check!");
                StartEating(currentTargetAnimal.transform);
            }
        }
        age += Time.deltaTime;
        if (age >= maxAge) Die();

        if (currentState != StateCarnivore.Eating && currentState != StateCarnivore.Hunt)
            hungerTimer -= Time.deltaTime;

        if (currentState != StateCarnivore.Drinking && currentState != StateCarnivore.SearchWater)
            thirstTimer -= Time.deltaTime;

        if (hungerTimer <= 0)
        {
            starvationTimer -= Time.deltaTime;
            if (starvationTimer <= 0) Die();
        }
        else
        {
            starvationTimer = starvationTime;
        }

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
    public void SetVisionRadius(float radius)//IHasVision interface implementáció
    {
        visionRadius = radius;
    }

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
                Debug.Log($"Spotted prey: {prey.name} at {preyPosition}"); 
                
            }

            Tile tile = hit.GetComponent<Tile>();
            if (tile != null && !exploredTiles.Contains(tile))
                exploredTiles.Add(tile);
        }
    }

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

        switch (currentState)
        {
            case StateCarnivore.Wander:
                spriteRenderer.color = normalColor;
                MoveToRandomPosition();
                break;

            case StateCarnivore.SearchFood:
                spriteRenderer.color = searchingColor;
                if (spottedPreyPositions.Count > 0) //ha látott már állatot
                {
                    currentState = StateCarnivore.Hunt;
                    HuntClosestPrey();
                }
                else MoveToRandomPosition();
                break;

            case StateCarnivore.SearchWater:
                spriteRenderer.color = searchingColor;
                SearchForWater();
                break;

            case StateCarnivore.Rest:
                spriteRenderer.color = restingColor;
                agent.isStopped = true;
                Invoke(nameof(ResumeWander), Random.Range(3f, 7f));
                break;
        }
    }

    private void HuntClosestPrey()
    {
        spriteRenderer.color = huntingColor;
        agent.speed = huntingSpeed;

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
            Debug.Log($"Attacking prey (by hunt closest): {closestPrey.Key.name}");
            StartEating(closestPrey.Key.transform);
        }
        // Különben követjük
        else if (closestPrey.Key != null)
        {
            agent.SetDestination(closestPrey.Value);
        }
    }

    private void StartEating(Transform prey)
    {
        if (prey == null) return;

        Debug.Log($"Started eating prey: {prey.name}");

        // Állapotbeállítások
        currentState = StateCarnivore.Eating;
        spriteRenderer.color = eatingColor;

        // Ragadozó mozgás leállítása
        agent.isStopped = true;

        // Préda mozgás letiltása (ha van NavMeshAgent-je)
        NavMeshAgent preyAgent = prey.GetComponent<NavMeshAgent>();
        Herbivore herbivore = prey.GetComponent<Herbivore>();
        if (preyAgent != null) herbivore.beingAttacked = true;
        Debug.Log($"Stopped prey agent: {prey.name}");
        currentTargetAnimal = prey.gameObject; //valamiért újra be kell állítani, mert ha nincs akkor a préda nem tûnik el
        // 10 másodperc után vége az evésnek
        Invoke(nameof(FinishEating), eatingDuration);
    }
    private void FinishEating()
    {
        if (currentTargetAnimal != null)
        {
            Debug.Log($"Finished eating prey: {currentTargetAnimal.name}");
            Destroy(currentTargetAnimal); // Préda megsemmisítése
        }

        // Visszaállítások
        hungerTimer = hungerInterval;
        currentTargetAnimal = null;
        currentState = StateCarnivore.Rest;
        spriteRenderer.color = restingColor;
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
            agent.SetDestination(currentTargetTile.transform.position);
            StartCoroutine(CheckIfReachedWater());
        }
        else MoveToRandomPosition();
    }

    private IEnumerator CheckIfReachedWater()
    {
        while (currentTargetTile != null &&
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
        spriteRenderer.color = drinkingColor;
        currentState = StateCarnivore.Drinking;
        agent.isStopped = true;

        thirstTimer = thirstInterval;
        dehydrationTimer = dehydrationTime;

        currentState = StateCarnivore.Rest;
        spriteRenderer.color = restingColor;
        currentTargetTile = null;
    }
    private void ResumeWander()
    {
        agent.isStopped = false;
        currentState = StateCarnivore.Wander;
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
    private void Die()
    {
        Debug.Log($"{name} died at age {age}.");
        Destroy(gameObject);
    }
}

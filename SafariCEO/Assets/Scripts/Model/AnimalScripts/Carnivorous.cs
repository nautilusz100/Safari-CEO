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
    public float slowedSpeedWater = 0.5f;
    public float slowedSpeedHills = 0.7f;
    public float huntingSpeed = 5f;
    public float normalSpeed = 3f;

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
    private Dictionary<GameObject, Vector3> spottedPreyPositions = new Dictionary<GameObject, Vector3>();

    private enum State { Wander, SearchFood, SearchWater, Eating, Drinking, Hunt, Rest }
    private State currentState = State.Wander;

    //épen aktuális célpontok
    private GameObject currentTargetAnimal;
    private Tile currentTargetTile;


    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = normalSpeed;

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
        // Ha a prédát elkapta, akkor az állapotot át kell állítani, ez a agent coliderrel mûkdöik, TODO nem így kene sztem
        if (currentState == State.Hunt && currentTargetAnimal != null)
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

        if (currentState != State.Eating && currentState != State.Hunt)
            hungerTimer -= Time.deltaTime;

        if (currentState != State.Drinking && currentState != State.SearchWater)
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

    private void UpdateVision()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Herbivore"))
            {
                GameObject prey = hit.gameObject;
                Vector3 preyPosition = prey.transform.position;
                Vector3 directionToPrey = (preyPosition - transform.position).normalized;
                float angleToPrey = Vector3.Angle(transform.right, directionToPrey);

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

        switch (currentState)
        {
            case State.Wander:
                spriteRenderer.color = normalColor;
                MoveToRandomPosition();
                break;

            case State.SearchFood:
                spriteRenderer.color = searchingColor;
                if (spottedPreyPositions.Count > 0) //ha látott már állatot
                {
                    currentState = State.Hunt;
                    HuntClosestPrey();
                }
                else MoveToRandomPosition();
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
        spriteRenderer.color = huntingColor;
        agent.speed = huntingSpeed;

        // Frissítjük a prédák pozícióját (eltávolítjuk a null elemeket)
        spottedPreyPositions = spottedPreyPositions
            .Where(p => p.Key != null)
            .ToDictionary(p => p.Key, p => p.Key.transform.position);

        if (spottedPreyPositions.Count == 0)
        {
            currentState = State.Wander;
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
    /* elõzõ koncepció, de nem mûködik
    private IEnumerator ChasePrey(GameObject prey)
    {
        while (prey != null && currentState == State.Hunt)
        {
            // Frissítsük a cél pozíciót
            agent.SetDestination(prey.transform.position);
            currentTargetAnimal = prey.transform;

            // Ha közel vagyunk, a trigger kezelje
            if (Vector3.Distance(transform.position, prey.transform.position) < 1.5f)
            {
                break;
            }

            yield return new WaitForSeconds(0.2f);
        }

        // Ha még mindig hunt állapotban vagyunk, de a préda eltûnt
        if (currentState == State.Hunt)
        {
            currentState = State.Wander;
        }
    }*/

    private void StartEating(Transform prey)
    {
        if (prey == null) return;

        Debug.Log($"Started eating prey: {prey.name}");

        // Állapotbeállítások
        currentState = State.Eating;
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
        Invoke("FinishEating", eatingDuration);
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
        currentState = State.Rest;
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
            currentState = State.Wander;
    }

    private void StartDrinking()
    {
        spriteRenderer.color = drinkingColor;
        currentState = State.Drinking;
        agent.isStopped = true;

        thirstTimer = thirstInterval;
        dehydrationTimer = dehydrationTime;

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
        // Tile kezelése
        Tile tile = other.GetComponent<Tile>();
        if (tile != null)
        {
            if (tile.Type == ShopType.Lake || tile.Type == ShopType.River)
            {
                slowZoneCountWater++;
                agent.speed = slowedSpeedWater;
            }
            else if (tile.Type == ShopType.Hills)
            {
                slowZoneCountHills++;
                agent.speed = slowedSpeedHills;
            }
            return;
        }

        // Préda kezelése, ez nem nagyon mûködik
        /*
        if (other.CompareTag("Herbivore") && currentState == State.Hunt)
        {
            Debug.Log($"Caught prey: {other.name}");
            StartEating(other.transform);

            // Távolítsuk el a prédát a látottak listájából
            if (spottedPreyPositions.ContainsKey(other.gameObject))
            {
                spottedPreyPositions.Remove(other.gameObject);
            }
        }*/
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

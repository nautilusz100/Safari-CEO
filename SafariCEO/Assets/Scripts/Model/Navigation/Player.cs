using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Tile;

public class Player : MonoBehaviour
{
    public NavMeshAgent agent; // A player NavMeshAgent-je
    public float moveInterval = 1f; // Milyen gyakran válasszon új célpontot
    public float moveRange = 40f; // A célpont távolsága a player-tõl
    public float normalSpeed = 1f;
    public float slowedSpeedWater = 0.5f;
    public float slowedSpeedHills = 0.7f;
    private int slowZoneCountWater = 0;
    private int slowZoneCountHills = 0;

    private void Start()
    {
        // Ha nincs hozzáadva a NavMeshAgent, adjuk hozzá
        agent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component
        agent.updateRotation = false; // Disable rotation update
        agent.updateUpAxis = false; // Enable position update
        agent.speed = normalSpeed;

        // Indítsunk egy coroutine-t, hogy idõszakosan új célpontot válasszunk
        InvokeRepeating("MoveToRandomPosition", 0f, moveInterval);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        Tile tile = other.GetComponent<Tile>();
        if (tile == null) return;
        //water slow zone 
        if (tile.Type == ShopType.Lake || tile.Type == ShopType.River)
        {
            slowZoneCountWater++; //tobbe megy bele mint 1
            agent.speed = slowedSpeedWater;
        }
        else if(tile.Type == ShopType.Hills)
        {
            //hills slow zone
            slowZoneCountHills++;
            agent.speed = slowedSpeedHills;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Tile tile = other.GetComponent<Tile>();
        if (tile == null) return;
        //water slow zone   
        if (tile.Type == ShopType.Lake || tile.Type == ShopType.River)
        {
            slowZoneCountWater--;
            if (slowZoneCountWater <= 0)
            {
                agent.speed = normalSpeed;
            }
        }else if (tile.Type == ShopType.Hills)
        {
            slowZoneCountHills--;
            if (slowZoneCountHills <= 0)
            {
                agent.speed = normalSpeed;
            }
        }
    }


    private void MoveToRandomPosition()
    {
        Vector3 randomPosition = GetRandomPosition();
        agent.SetDestination(randomPosition);

        // Válassz új idõintervallumot
        float nextInterval = Random.Range(2f, 5f);
        CancelInvoke("MoveToRandomPosition");
        Invoke("MoveToRandomPosition", nextInterval);
    }


    // Véletlenszerû pozíció generálása
    private Vector3 GetRandomPosition()
    {
        // Véletlenszerû pozíció generálása a player körül
        Vector3 randomDirection = Random.insideUnitSphere * moveRange;

        // A véletlenszerû célpont a player aktuális pozíciójához adva
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, moveRange, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position; // Ha nincs érvényes célpont, maradjon a jelenlegi helyen
    }
}

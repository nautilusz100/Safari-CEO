using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    public NavMeshAgent agent; // A player NavMeshAgent-je
    public float moveInterval = 3f; // Milyen gyakran válasszon új célpontot
    public float moveRange = 10f; // A célpont távolsága a player-tõl

    private void Start()
    {
        // Ha nincs hozzáadva a NavMeshAgent, adjuk hozzá
        agent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component
        agent.updateRotation = false; // Disable rotation update
        agent.updateUpAxis = false; // Enable position update

        // Indítsunk egy coroutine-t, hogy idõszakosan új célpontot válasszunk
        InvokeRepeating("MoveToRandomPosition", 0f, moveInterval);
    }

    private void MoveToRandomPosition()
    {
        // Válasszunk egy véletlenszerû célpontot a player-tõl meghatározott távolságra
        Vector3 randomPosition = GetRandomPosition();

        // Állítsuk be a NavMeshAgent célpontját
        agent.SetDestination(randomPosition);
    }

    // Véletlenszerû pozíció generálása
    private Vector3 GetRandomPosition()
    {
        // Véletlenszerû pozíció generálása a player körül
        Vector3 randomDirection = Random.insideUnitSphere * moveRange;

        // A véletlenszerû célpont a player aktuális pozíciójához adva
        randomDirection += transform.position;

        // Ha szükséges, végezzünk egy Raycast-ot, hogy biztosítsuk, hogy a célpont a NavMesh-en legyen
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, moveRange, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position; // Ha nincs érvényes célpont, maradjon a jelenlegi helyen
    }
}

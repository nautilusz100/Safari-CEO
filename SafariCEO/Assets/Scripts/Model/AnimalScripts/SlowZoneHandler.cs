using Assets.Scripts.Model.Map;
using UnityEngine;
using UnityEngine.AI;
using static Tile;

public class SlowZoneHandler : MonoBehaviour
{
    public float slowedSpeedWater = 0.5f;
    public float slowedSpeedHills = 0.7f;

    public float visionRadiusNormal = 5f;
    public float visionRadiusHills = 10f;

    private float normalSpeed;
    private NavMeshAgent agent;
    private IHasVision visionOwner;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        visionOwner = GetComponent<IHasVision>();
        normalSpeed = agent.speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Tile tile = other.GetComponent<Tile>();
        if (tile == null) return;

        switch (tile.Type)
        {
            case ShopType.Lake:
            case ShopType.River:
                agent.speed = slowedSpeedWater;
                visionOwner.SetVisionRadius(visionRadiusNormal);
                break;

            case ShopType.Hills:
                agent.speed = slowedSpeedHills;
                visionOwner.SetVisionRadius(visionRadiusHills);
                break;

            default:
                agent.speed = normalSpeed;
                visionOwner.SetVisionRadius(visionRadiusNormal);
                break;
        }
    }
}

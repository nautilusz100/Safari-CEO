using Assets.Scripts.Model.Map;
using UnityEngine;
using UnityEngine.AI;
using static Tile;

public class FakeNavMeshAgent : MonoBehaviour
{
    public float speed;
}
public class SlowZoneHandler : MonoBehaviour
{
    public float slowedSpeedWater = 0.5f;
    public float slowedSpeedHills = 0.7f;

    public float visionRadiusNormal = 5f;
    public float visionRadiusHills = 10f;

    private float normalSpeed = 1f;
    private NavMeshAgent agent;
    private IHasVision visionOwner;

    public float? testSpeedOverride;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        visionOwner = GetComponent<IHasVision>();
        normalSpeed = 1f;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        Tile tile = other.GetComponent<Tile>();
        if (tile == null) return;

        
        switch (tile.Type)
        {
            case ShopType.Lake:
            case ShopType.River:
                float newSpeed = slowedSpeedWater * (int)GameManager.Instance.CurrentGameSpeed;
                if (agent != null)
                {
                    agent.speed = newSpeed;
                    visionOwner.SetVisionRadius(visionRadiusNormal);
                }
                else
                    testSpeedOverride = newSpeed;
                break;

            case ShopType.Hills:

                newSpeed = slowedSpeedHills * (int)GameManager.Instance.CurrentGameSpeed;
                if (agent != null)
                {
                    agent.speed = newSpeed;
                    visionOwner.SetVisionRadius(visionRadiusNormal);
                }
                else
                    testSpeedOverride = newSpeed;
                break;

            default:
                newSpeed = normalSpeed * (int)GameManager.Instance.CurrentGameSpeed;
                if (agent != null)
                {
                    agent.speed = newSpeed;
                    visionOwner.SetVisionRadius(visionRadiusNormal);
                }
                else
                    testSpeedOverride = newSpeed;

                break;
        }

    }
}

using Assets.Scripts.Model.Map;
using UnityEngine;
using UnityEngine.AI;
using static Tile;

/// <summary>
/// Dummy component for testing speed override without NavMeshAgent.
/// </summary>
public class FakeNavMeshAgent : MonoBehaviour
{
    public float speed;
}
/// <summary>
/// Adjusts speed and vision radius when entering different terrain types.
/// </summary>
public class SlowZoneHandler : MonoBehaviour
{
    public float SlowedSpeedWater { get; set; } = 0.5f;
    public float SlowedSpeedHills { get; set; } = 0.7f;

    public float VisionRadiusNormal { get; set; } = 5f;
    public float VisionRadiusHills { get; set; } = 10f;

    private float normalSpeed = 1f;
    private NavMeshAgent agent;
    private IHasVision visionOwner;

    // Used for testing when there's no NavMeshAgent
    public float? TestSpeedOverride { get; private set; }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        visionOwner = GetComponent<IHasVision>();
        normalSpeed = 1f;
    }
    /// <summary>
    /// Triggered when the player enters a new tile. Adjusts speed and vision based on tile type.
    /// </summary>
    public void OnTriggerEnter2D(Collider2D other)
    {
        Tile tile = other.GetComponent<Tile>();
        if (tile == null) return;

        
        switch (tile.Type)
        {
            case ShopType.Lake:
            case ShopType.River:
                float newSpeed = SlowedSpeedWater * (int)GameManager.Instance.CurrentGameSpeed;
                if (agent != null)
                {
                    agent.speed = newSpeed;
                    visionOwner.SetVisionRadius(VisionRadiusNormal);
                }
                else
                    TestSpeedOverride = newSpeed;
                break;

            case ShopType.Hills:

                newSpeed = SlowedSpeedHills * (int)GameManager.Instance.CurrentGameSpeed;
                if (agent != null)
                {
                    agent.speed = newSpeed;
                    visionOwner.SetVisionRadius(VisionRadiusNormal);
                }
                else
                    TestSpeedOverride = newSpeed;
                break;

            default:
                newSpeed = normalSpeed * (int)GameManager.Instance.CurrentGameSpeed;
                if (agent != null)
                {
                    agent.speed = newSpeed;
                    visionOwner.SetVisionRadius(VisionRadiusNormal);
                }
                else
                    TestSpeedOverride = newSpeed;

                break;
        }

    }
}

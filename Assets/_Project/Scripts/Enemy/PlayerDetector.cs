using UnityEngine;
using ImprovedTimers;

/// <summary>
/// 检测玩家
/// </summary>
public class PlayerDetector : MonoBehaviour
{
    [SerializeField] float detectionAngle = 60f; // 前方扇形检测角度
    [SerializeField] float detectionRadius = 10f; // 周围圆形检测范围半径
    [SerializeField] float innerDetectionRadius = 5f; // 内部近身检测半径
    [SerializeField] float detectionCooldown = 1f; // 检测间隔时间
    [SerializeField] float attackRange = 2f; // 攻击距离

    public Transform Player { get; private set; }
    public Health PlayerHealth { get; private set; }

    CountdownTimer detectionTimer;

    IDetectionStrategy detectionStrategy;

    void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player").transform; // Make sure to TAG the player!
        PlayerHealth = Player.GetComponent<Health>();
    }

    void Start()
    {
        detectionTimer = new CountdownTimer(detectionCooldown);
        detectionStrategy = new ConeDetectionStrategy(detectionAngle, detectionRadius, innerDetectionRadius);
    }

    void Update() => detectionTimer.Tick();

    public bool CanDetectPlayer()
    {
        return detectionTimer.IsRunning || detectionStrategy.Execute(Player, transform, detectionTimer);
    }

    public bool CanAttackPlayer()
    {
        var directionToPlayer = Player.position - transform.position;
        return directionToPlayer.magnitude <= attackRange;
    }

    public void SetDetectionStrategy(IDetectionStrategy detectionStrategy) =>
        this.detectionStrategy = detectionStrategy;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // Draw a spheres for the radii
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.DrawWireSphere(transform.position, innerDetectionRadius);

        // Calculate our cone directions
        Vector3 forwardConeDirection = Quaternion.Euler(0, detectionAngle / 2, 0) * transform.forward * detectionRadius;
        Vector3 backwardConeDirection =
            Quaternion.Euler(0, -detectionAngle / 2, 0) * transform.forward * detectionRadius;

        // Draw lines to represent the cone
        Gizmos.DrawLine(transform.position, transform.position + forwardConeDirection);
        Gizmos.DrawLine(transform.position, transform.position + backwardConeDirection);
    }
}
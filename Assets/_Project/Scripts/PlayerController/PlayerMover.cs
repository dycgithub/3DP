using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMover : MonoBehaviour
{
    #region Fields

    [Header("Collider Settings:")] [Range(0f, 1f)] [SerializeField]
    float stepHeightRatio = 0.1f; //步高,台阶高度

    [SerializeField] float colliderHeight = 2f;
    [SerializeField] float colliderThickness = 1f;
    [SerializeField] Vector3 colliderOffset = Vector3.zero;

    Rigidbody rb;
    Transform tr;
    CapsuleCollider col;
    RaycastSensor sensor;

    bool isGrounded;
    float baseSensorRange;
    Vector3 currentGroundAdjustmentVelocity; //调整玩家位置保持接触地面
    int currentLayer;

    [Header("Sensor Settings:")] [SerializeField]
    bool isInDebugMode;

    bool isUsingExtendedSensorRange = true; // 使用拓展范围实现平滑过渡

    #endregion

    void Awake()
    {
        Setup();
        RecalculateColliderDimensions();
    }

    void OnValidate()
    {
        if (gameObject.activeInHierarchy)
        {
            RecalculateColliderDimensions();
        }
    }

    void LateUpdate()
    {
        if (isInDebugMode)
        {
            sensor.DrawDebug();
        }
    }

    public void CheckForGround()
    {
        if (currentLayer != gameObject.layer)
        {
            RecalculateSensorLayerMask();
        }

        currentGroundAdjustmentVelocity = Vector3.zero;
        sensor.castLength = isUsingExtendedSensorRange
            ? baseSensorRange + colliderHeight * tr.localScale.x * stepHeightRatio
            : baseSensorRange;
        sensor.Cast();

        isGrounded = sensor.HasDetectedHit();
        if (!isGrounded) return;

        float distance = sensor.GetDistance();
        float upperLimit = colliderHeight * tr.localScale.x * (1f - stepHeightRatio) * 0.5f;
        float middle = upperLimit + colliderHeight * tr.localScale.x * stepHeightRatio;
        float distanceToGo = middle - distance;

        currentGroundAdjustmentVelocity = tr.up * (distanceToGo / Time.fixedDeltaTime);
    }

    public bool IsGrounded() => isGrounded;
    public Vector3 GetGroundNormal() => sensor.GetNormal();

    // NOTE: Older versions of Unity use rb.velocity instead
    public void SetVelocity(Vector3 velocity) => rb.velocity = velocity + currentGroundAdjustmentVelocity;
    public void SetExtendSensorRange(bool isExtended) => isUsingExtendedSensorRange = isExtended;

    void Setup()
    {
        //初始化
        tr = transform;
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;
        rb.useGravity = false;
    }

    void RecalculateColliderDimensions()
    {
        //计算碰撞体尺寸
        if (col == null)
        {
            Setup();
        }

        col.height = colliderHeight * (1f - stepHeightRatio);
        col.radius = colliderThickness / 2f;
        col.center = colliderOffset * colliderHeight + new Vector3(0f, stepHeightRatio * col.height / 2f, 0f);

        if (col.height / 2f < col.radius)
        {
            col.radius = col.height / 2f;
        }

        RecalibrateSensor();
    }

    /// <summary>
    /// 重新校准传感器
    /// </summary>
    void RecalibrateSensor()
    {
        sensor ??= new RaycastSensor(tr);//如果sensor为null，则创建一个新的RaycastSensor对象并赋值给sensor

        sensor.SetCastOrigin(col.bounds.center);
        sensor.SetCastDirection(RaycastSensor.CastDirection.Down);
        RecalculateSensorLayerMask();

        const float
            safetyDistanceFactor =
                0.001f; // 在计算传感器距离时，为了防止clipping(穿模)问题，增加了一个小因素

        float length = colliderHeight * (1f - stepHeightRatio) * 0.5f + colliderHeight * stepHeightRatio;
        baseSensorRange = length * (1f + safetyDistanceFactor) * tr.localScale.x;
        sensor.castLength = length * tr.localScale.x;
    }

    /// <summary>
    /// 获取可检测的层
    /// </summary>
    void RecalculateSensorLayerMask()
    {
        int objectLayer = gameObject.layer;
        int layerMask = Physics.AllLayers;

        for (int i = 0; i < 32; i++)
        {
            if (Physics.GetIgnoreLayerCollision(objectLayer, i))
            {
                layerMask &= ~(1 << i);
            }
        }

        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        layerMask &= ~(1 << ignoreRaycastLayer);

        sensor.layermask = layerMask;
        currentLayer = objectLayer;
    }
}
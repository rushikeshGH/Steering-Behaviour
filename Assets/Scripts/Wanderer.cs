using UnityEngine;

class Wanderer: MonoBehaviour, ISteerable
{
    private const int SteerableLayerMask = ~(1 << 3);

    [SerializeField] private float aversionDistance = 5f;
    [SerializeField] private float aversionRadius = 2f;
    [SerializeField] private float wanderAngle = 240f;
    [SerializeField] private float wanderCircleRadius = 40f;

    private Vector3 wanderCircleCenter = Vector3.zero;

    [field: SerializeField] public float MaximumVelocity { get; set; } = 16f;
    [field: SerializeField] public float MaximumSteeringForce { get; set; } = 0.2f;

    public Vector3 Velocity { get; set; }
    public Vector3 SteeringForce { get; set; }

    public Vector3 Position { get => transform.position; set => transform.position = value; }

    private void Awake()
    {
        Velocity = Vector3.ClampMagnitude(transform.forward * MaximumVelocity, MaximumVelocity * Time.deltaTime);
        wanderCircleCenter = transform.position;
    }

    private void Update()
    {
        this.Steer(
            this.WanderWithinCircle(wanderAngle, wanderCircleCenter, wanderCircleRadius),
            this.Avert(aversionDistance, aversionRadius, SteerableLayerMask));
    }

    public void Rotate()
    {
        Vector3 forward = Velocity.normalized;
        if (forward.magnitude != 0)
            transform.forward = forward;
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        SteeringBehaviourDebugger.DrawSteer(this);
        SteeringBehaviourDebugger.DrawWanderWithinCircle(this, wanderAngle, wanderCircleCenter, wanderCircleRadius);
        SteeringBehaviourDebugger.DrawAvert(this, aversionDistance, aversionRadius, SteerableLayerMask);
    }
}

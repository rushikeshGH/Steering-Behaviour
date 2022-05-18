using UnityEngine;

class Flocker : MonoBehaviour, ISteerable
{
    private const int SteerableLayerMask = ~(1 << 3);

    [SerializeField] private int aversionDistance = 5;
    [SerializeField] private int aversionRadius = 2;
    [SerializeField] private int neighbourhoodRadius = 3;

    [field: SerializeField] public float MaximumVelocity { get; set; } = 16;
    [field: SerializeField] public float MaximumSteeringForce { get; set; } = 0.2f;

    public Vector3 Velocity { get; set; }
    public Vector3 SteeringForce { get; set; }

    public Vector3 Position { get => transform.position; set => transform.position = value; }

    private Vector3 MouseWorldPosition
    {
        get
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 20;
            return Camera.main.ScreenToWorldPoint(mousePosition);
        }
    }

    private void Awake()
    {
        Velocity = Vector3.ClampMagnitude(transform.forward * MaximumVelocity, MaximumVelocity * Time.deltaTime);
    }

    private void Update()
    {
        this.Steer(
            this.Flock(neighbourhoodRadius, aversionRadius, ~SteerableLayerMask),
            this.Seek(MouseWorldPosition, 0),
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
        SteeringBehaviourDebugger.DrawFlock(this, neighbourhoodRadius, ~SteerableLayerMask);
        SteeringBehaviourDebugger.DrawAvert(this, aversionDistance, aversionRadius, SteerableLayerMask);
    }
}

using UnityEngine;

class Flocker : MonoBehaviour, ISteerable
{
    private const int LayerMask = ~(1 << 2);

    [SerializeField] private int aversionDistance = 5;
    [SerializeField] private int aversionRadius = 2;
    [SerializeField] private int neighbourhoodRadius = 3;

    [field: SerializeField] public float MaximumVelocity { get; set; }
    [field: SerializeField] public float MaximumSteeringForce { get; set; }

    public Vector3 Velocity { get; set; }
    public Vector3 SteeringForce { get; set; }

    public Vector3 Position { get => transform.position; set => transform.position = value; }

    private static Vector3 MouseWorldPosition
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
            this.Flock(neighbourhoodRadius, aversionRadius, ~LayerMask),
            this.Seek(MouseWorldPosition, 0),
            this.Avert(aversionDistance, aversionRadius, LayerMask));
    }

    public void Rotate()
    {
        Vector3 forward = Velocity.normalized;
        if (forward.magnitude != 0)
            transform.forward = forward;
    }

    private void OnDrawGizmosSelected()
    {
        SteeringBehaviourDebugger.DrawSteer(this);
        SteeringBehaviourDebugger.DrawFlock(this, neighbourhoodRadius, ~LayerMask);
        SteeringBehaviourDebugger.DrawAvert(this, aversionDistance, aversionRadius, LayerMask);
    }
}

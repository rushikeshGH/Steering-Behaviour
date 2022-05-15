using UnityEngine;

class Agent47 : MonoBehaviour, ISteerable
{
    private const int LayerMask = ~(1 << 2);
    private const int NeighbourhoodRadius = 3;

    [SerializeField] private int aversionDistance = 5;
    [SerializeField] private int aversionRadius = 2;

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
            this.Flock(NeighbourhoodRadius, aversionRadius, ~LayerMask),
            this.Seek(MouseWorldPosition, 0),
            this.Avert(aversionDistance, aversionRadius, LayerMask));
    }

    public void Rotate()
    {
        Vector3 forward = Velocity.normalized;
        if (forward.magnitude != 0)
            transform.forward = forward;
    }

    //private void OnDrawGizmos()
    //{
    //    SteeringBehaviourDebugger.DrawAvert(this, aversionDistance, aversionRadius, LayerMask);
    //    SteeringBehaviourDebugger.DrawFlock(this, NeighbourhoodRadius, ~LayerMask);
    //}
}

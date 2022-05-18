using UnityEngine;

public class Pursuer : MonoBehaviour, ISteerable
{
    private const int SteerableLayerMask = ~(1 << 3);

    [SerializeField] private int aversionDistance = 5;
    [SerializeField] private int aversionRadius = 2;
    [SerializeField] private int neighbourhoodRadius = 3;

    private Evader evader;

    [field: SerializeField] public float MaximumVelocity { get; set; } = 16;
    [field: SerializeField] public float MaximumSteeringForce { get; set; } = 0.2f;

    public Vector3 Velocity { get; set; }
    public Vector3 SteeringForce { get; set; }
    public Vector3 Position { get => transform.position; set => transform.position = value; }

    private void Awake() => evader = FindObjectOfType<Evader>();

    private void Update()
    {
        this.Steer(
            this.Pursue(evader, 0),
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
        SteeringBehaviourDebugger.DrawPursue(this, evader);
        SteeringBehaviourDebugger.DrawAvert(this, aversionDistance, aversionRadius, SteerableLayerMask);
    }
}

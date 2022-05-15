using UnityEditor;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] private float maximumSteeringForce;
    [SerializeField] private float maximumVelocity;
    [SerializeField] private float arrivalDistance;
    [Header("Wander")]
    [SerializeField] private float wanderCircleDistance = 5;
    [SerializeField] private float wanderCircleRadius = 1;
    [Header("Aversion")]
    [SerializeField] private float aversionDistance = 5;

    private Vector3 steeringForce;
    private Vector3 wanderDisplacement;
    private Vector3 targetFuturePosition;
    private RaycastHit[] raycastHits = new RaycastHit[0];

    public static Target Target => GameObject.Find("Target")?.GetComponent<Target>();

    public Vector3 Position => transform.position;

    public Vector3 Velocity { get; private set; }

    private void Awake()
    {
        Velocity = Vector3.ClampMagnitude(transform.forward * maximumVelocity, maximumVelocity * Time.deltaTime);
    }

    private void Update()
    {
        steeringForce = Vector3.zero;
        steeringForce += Seek(Target.Position, arrivalDistance);
        steeringForce += Avert();

        steeringForce = Vector3.ClampMagnitude(steeringForce, maximumSteeringForce * Time.deltaTime);
        Velocity = Vector3.ClampMagnitude(Velocity + steeringForce, maximumVelocity * Time.deltaTime);

        Vector3 forward = Velocity.normalized;
        if (forward.magnitude != 0)
            transform.forward = forward;
        transform.position += Velocity;
    }

    private Vector3 Avert()
    {
        Ray ray = new Ray(transform.position, Velocity.normalized);
        if (raycastHits.Length <= 0)
            raycastHits = Physics.RaycastAll(ray, aversionDistance);

        if (raycastHits.Length > 0)
        {
            RaycastHit raycastHit = raycastHits[0];
            Bounds obstacleBounds = raycastHit.collider.bounds;
            float rayMagnitude = (ray.direction * aversionDistance).magnitude;
            Vector3 aversion = Vector3.zero;
            const int precision = 10;
            float step = rayMagnitude / precision;
            for (int i = 0; i < precision; i++)
            {
                Vector3 point = ray.GetPoint(step * i);
                if (obstacleBounds.Contains(point))
                {
                    aversion = ray.origin + (ray.direction * aversionDistance) - obstacleBounds.center;
                    break;
                }
            }

            if (aversion.magnitude == 0)
                raycastHits = new RaycastHit[0];

            return aversion.normalized * int.MaxValue;
        }

        return Vector3.zero;
    }

    private Vector3 Wander()
    {
        Vector3 center = Position + (transform.forward * wanderCircleDistance);
        wanderDisplacement = transform.forward * wanderCircleRadius;
        wanderDisplacement = Quaternion.AngleAxis(Random.Range(-120, 120), Vector3.up) * wanderDisplacement;
        return center + wanderDisplacement;
    }

    private void Arrive()
    {
        float scale = Mathf.Min(1, (Target.Position - Position).sqrMagnitude / (arrivalDistance * arrivalDistance));
        Velocity = Vector3.ClampMagnitude(Velocity, scale * maximumVelocity * Time.deltaTime);
    }

    private Vector3 Seek(Vector3 targetPosition, float arrivalDistance)
    {
        Vector3 desiredVeclocity = targetPosition - transform.position;
        if (desiredVeclocity.sqrMagnitude <= (arrivalDistance * arrivalDistance))
            Arrive();
        return desiredVeclocity - Velocity;
    }

    private Vector3 Flee(Vector3 targetPositon)
    {
        Vector3 desiredVeclocity = Position - targetPositon;
        return desiredVeclocity - Velocity;
    }

    private Vector3 Pursue()
    {
        float distance = (Target.Position - Position).magnitude;
        float delta = 150 * Mathf.Min(1, distance / 5);
        //float delta = 150 * (distance / maximumVelocity);
        targetFuturePosition = Target.Position + Target.Velocity * delta;
        return Seek(targetFuturePosition, 0);
    }

    private Vector3 Evade()
    {
        float distance = (Target.Position - Position).magnitude;
        float delta = 100 * Mathf.Min(1, distance / maximumVelocity);
        //float delta = 150 * (distance / maximumVelocity);
        targetFuturePosition = Target.Position + Target.Velocity * delta;
        return Flee(targetFuturePosition);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        const int Thickness = 4;
        const int Scale = 1;

        DrawVelocityLine(Thickness, Scale);
        DrawSteeringForceLine(Thickness, Scale);

        DrawCollisionAvoidaneCube();
        //DrawWanderCircle(Thickness);
        //DrawPursuitEvadeCircle(Thickness);
    }

    private void DrawCollisionAvoidaneCube()
    {
        if (raycastHits.Length <= 0)
            return;

        Handles.color = Color.red;
        RaycastHit raycastHit = raycastHits[0];
        Bounds obstacleBounds = raycastHit.collider.bounds;
        Handles.DrawWireCube(obstacleBounds.center, obstacleBounds.size);
    }

    private void DrawPursuitEvadeCircle(int Thickness)
    {
        Handles.color = Color.red;
        Handles.DrawWireDisc(targetFuturePosition, Vector3.up, .25f, Thickness);
    }

    private void DrawVelocityLine(int Thickness, int Scale)
    {
        Vector3 end = Position + (Velocity.normalized * Scale);
        Handles.color = Color.blue;
        Handles.DrawLine(Position, end, Thickness);
    }

    private void DrawSteeringForceLine(int Thickness, int Scale)
    {
        Vector3 start = Position + (Velocity.normalized * Scale);
        Handles.color = Color.green;
        Handles.DrawLine(start, start + (steeringForce.normalized * Scale), Thickness);
    }

    private void DrawWanderCircle(int Thickness)
    {
        Vector3 center = Position + (transform.forward * wanderCircleDistance);
        Handles.color = Color.red;
        Handles.DrawWireDisc(center, Vector3.up, 1, Thickness);
        Handles.color = Color.yellow;
        Handles.DrawLine(center, center + wanderDisplacement, Thickness);
    }
}

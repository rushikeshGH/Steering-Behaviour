using UnityEditor;
using UnityEngine;

public class Sandbox : MonoBehaviour
{
    private static Vector3 TargetPosition
    {
        get
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 20;
            return Camera.main.ScreenToWorldPoint(mousePosition);
        }
    }

    [SerializeField] private float maximumVelocity = 10;
    [Range(0.01f, 0.05f)] [SerializeField] private float steeringMultiplier = 0.03f;
    [SerializeField] private float arrivalDistance = 5;
    [SerializeField] private float circleDistance = 10;
    [SerializeField] private float circleRadius = 4;
    [SerializeField] private float lookAheadDistance = 1;
    [SerializeField] private float avoidanceForce;

    private Vector3 desieredVelocity;
    private Vector3 steering;
    private Vector3 velocity;
    private RaycastHit[] raycastHits = new RaycastHit[0];

    public Vector3 Velocity
    {
        get
        {
            velocity.y = 0;
            return velocity;
        }
        private set => velocity = value;
    }

    private float MaximumSteering => maximumVelocity * steeringMultiplier;

    private void Update()
    {
        steering = Vector3.zero;
        steering += Seek(TargetPosition);
        steering += AvoidCollision();

        steering = Vector3.ClampMagnitude(steering, MaximumSteering * Time.deltaTime);
        Velocity = Vector3.ClampMagnitude(Velocity + steering, maximumVelocity * Time.deltaTime);

        Vector3 forward = Velocity.normalized;
        if (forward.magnitude != 0)
            transform.forward = forward;
        transform.position += Velocity;
    }

    private Vector3 AvoidCollision()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (raycastHits.Length <= 0)
        {
            raycastHits = Physics.RaycastAll(ray, lookAheadDistance);
        }

        if(raycastHits.Length > 0)
        {
            RaycastHit raycastHit = raycastHits[0];
            Bounds obstacleBounds = raycastHit.collider.bounds;
            Vector3 raycast = transform.position + (ray.direction * lookAheadDistance);
            Vector3 avoidance = Vector3.zero;
            float step = raycast.magnitude / 4;
            for (int i = 0; i < 4; i++)
            {
                Vector3 point = ray.GetPoint(step * i);
                if (obstacleBounds.Contains(point))
                {
                    avoidance = raycast - obstacleBounds.center;
                    break;
                }
            }

            if (avoidance.magnitude == 0)
                raycastHits = new RaycastHit[0];
            
            return avoidance.normalized * avoidanceForce;
        }

        return Vector3.zero;
    }

    private void Pursuit()
    {
        var distance = TargetPosition - transform.position;
        var t = distance.magnitude / maximumVelocity;
        Vector3 targetPosition = TargetPosition /* * target velocity */ * t;
        Seek(targetPosition);
    }

    private void Evade()
    {
        var distance = TargetPosition - transform.position;
        var t = distance.magnitude / maximumVelocity;
        Vector3 targetPosition = TargetPosition /* * target velocity */ * t;
        Flee(targetPosition);
    }

    private Vector3 Wander()
    {
        Vector3 circleCenter = transform.position + (transform.forward * circleDistance);
        Vector3 displacement = transform.forward * circleRadius;
        displacement = Quaternion.AngleAxis(Random.Range(-120, 120), Vector3.up) * displacement;
        return circleCenter + displacement;
    }

    private void SeekArrival()
    {
        desieredVelocity = TargetPosition - transform.position;
        float velocityMultiplier = Mathf.Min(desieredVelocity.magnitude / arrivalDistance, 1.0f);
        steering = Vector3.ClampMagnitude(desieredVelocity - Velocity, MaximumSteering * Time.deltaTime);
        Velocity = Vector3.ClampMagnitude(Velocity + steering, velocityMultiplier * maximumVelocity * Time.deltaTime);
    }

    private Vector3 Seek(Vector3 targetPosition)
    {
        desieredVelocity = targetPosition - transform.position;
        return desieredVelocity - Velocity;
    }

    private void Flee(Vector3 targetPosition)
    {
        desieredVelocity = transform.position - targetPosition;
        steering = Vector3.ClampMagnitude(desieredVelocity - Velocity, MaximumSteering * Time.deltaTime);
        Velocity = Vector3.ClampMagnitude(Velocity + steering, maximumVelocity * Time.deltaTime);
    }

    //private void OnDrawGizmos()
    //{
    //    Vector3 circleCenter = transform.position + transform.forward * circleDistance;
    //    Handles.DrawWireDisc(circleCenter, Vector3.up, circleRadius);
    //}
}

using UnityEngine;

public static class SteeringBehaviour
{
    public static void Steer(this ISteerable steerable, params Vector3[] steeringForces)
    {
        steerable.SteeringForce = Vector3.zero;
        foreach (Vector3 steeringForce in steeringForces)
            steerable.SteeringForce += steeringForce;

        steerable.SteeringForce = Vector3.ClampMagnitude(steerable.SteeringForce, steerable.MaximumSteeringForce * Time.deltaTime);
        steerable.Velocity = Vector3.ClampMagnitude(steerable.Velocity + steerable.SteeringForce, steerable.MaximumVelocity * Time.deltaTime);
        steerable.Velocity = new Vector3(steerable.Velocity.x, 0, steerable.Velocity.z);
        steerable.Position += steerable.Velocity;

        steerable.Rotate();
    }

    public static Vector3 Flock(this ISteerable steerable, float neighbourhoodRadius, float seperationRadius, LayerMask layerMask)
    {
        Vector3 flocking = Vector3.zero;
        flocking += AlignWithNeighbours(steerable, neighbourhoodRadius, layerMask);
        flocking += CohereWithNeighbours(steerable, neighbourhoodRadius, layerMask);
        flocking += SeperateFromNeighbours(steerable, seperationRadius, layerMask);
        return flocking;
    }

    public static Vector3 AlignWithNeighbours(this ISteerable steerable, float neighbourhoodRadius, LayerMask layerMask)
    {
        Vector3 averageVelocity = Vector3.zero;
        int neighbourCount = 0;
        Collider[] colliders = Physics.OverlapSphere(steerable.Position, neighbourhoodRadius, layerMask);

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out ISteerable neighbour))
            {
                averageVelocity += neighbour.Velocity;
                neighbourCount++;
            }
        }

        if (neighbourCount > 0)
            averageVelocity /= neighbourCount;

        return averageVelocity.normalized;
    }

    public static Vector3 CohereWithNeighbours(this ISteerable steerable, float neighbourhoodRadius, LayerMask layerMask)
    {
        Vector3 averagePosition = Vector3.zero;
        int neighbourCount = 0;
        Collider[] colliders = Physics.OverlapSphere(steerable.Position, neighbourhoodRadius, layerMask);

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out ISteerable neighbour))
            {
                averagePosition += neighbour.Position;
                neighbourCount++;
            }
        }

        if (neighbourCount > 0)
            averagePosition /= neighbourCount;

        Vector3 cohesion = averagePosition - steerable.Position;
        return cohesion.normalized;
    }

    public static Vector3 SeperateFromNeighbours(this ISteerable steerable, float neighbourhoodRadius, LayerMask layerMask)
    {
        Vector3 averageDistance = Vector3.zero;
        int neighbourCount = 0;
        Collider[] colliders = Physics.OverlapSphere(steerable.Position, neighbourhoodRadius, layerMask);

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out ISteerable neighbour))
            {
                averageDistance += neighbour.Position - steerable.Position;
                neighbourCount++;
            }
        }

        if (neighbourCount > 0)
            averageDistance /= neighbourCount;

        return -averageDistance.normalized * int.MaxValue;
    }

    public static Vector3 Seek(this ISteerable steerable, Vector3 position, float arrivalDistance)
    {
        Vector3 desiredVeclocity = position - steerable.Position;
        if (desiredVeclocity.sqrMagnitude <= (arrivalDistance * arrivalDistance))
        {
            float scale = Mathf.Min(1, (position - steerable.Position).sqrMagnitude / (arrivalDistance * arrivalDistance));
            steerable.Velocity = Vector3.ClampMagnitude(steerable.Velocity, scale * steerable.MaximumVelocity * Time.deltaTime);
        }
        return desiredVeclocity - steerable.Velocity;
    }

    public static Vector3 Avert(this ISteerable steerable, float aversionDistance, float aversionRadius, LayerMask layerMask)
    {
        Vector3 aversion = Vector3.zero;
        Ray ray = new Ray(steerable.Position, steerable.Velocity.normalized);
        RaycastHit[] raycastHits = Physics.RaycastAll(ray, aversionDistance, layerMask);

        if (raycastHits.Length > 0)
        {
            RaycastHit raycastHit = raycastHits[0];
            Collider collider = raycastHit.collider;
            Bounds bounds = collider.bounds;
            float rayMagnitude = (ray.direction * aversionDistance).magnitude;
            const int precision = 10;
            float step = rayMagnitude / precision;
            for (int i = 0; i < precision; i++)
            {
                Vector3 point = ray.GetPoint(step * i);
                if (bounds.Contains(point))
                {
                    aversion = ray.origin + (ray.direction * aversionDistance) - bounds.center;
                    break;
                }
            }
        }
        else
        {
            Collider[] colliders = Physics.OverlapSphere(steerable.Position, aversionRadius, layerMask);

            foreach (Collider collider in colliders)
            {
                Bounds bounds = collider.bounds;
                aversion += ray.origin + (ray.direction * aversionDistance) - bounds.center;
            }
        }

        return aversion.normalized * int.MaxValue;
    }
}

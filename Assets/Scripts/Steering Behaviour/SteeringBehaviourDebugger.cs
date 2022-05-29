#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public static class SteeringBehaviourDebugger
{
    public static void DrawSteer(ISteerable steerable)
    {
        Handles.color = Color.blue;
        Handles.DrawLine(steerable.Position, steerable.Position + (steerable.Velocity.normalized * 2));
        Handles.color = Color.green;
        Handles.DrawLine(steerable.Position, steerable.Position + (steerable.SteeringForce.normalized * 2));
    }

    public static void DrawFlock(ISteerable steerable, float neighbourhoodRadius, LayerMask layerMask)
    {
        Handles.color = Color.yellow;
        Handles.DrawWireDisc(steerable.Position, Vector3.up, neighbourhoodRadius);
        DrawAlignWithNeghbours(steerable, neighbourhoodRadius, layerMask);
        DrawCohereWithNeighbours(steerable, neighbourhoodRadius, layerMask);
        DrawSeperateFromNeighbours(steerable, neighbourhoodRadius, layerMask);
    }

    public static void DrawAlignWithNeghbours(ISteerable steerable, float neighbourhoodRadius, LayerMask layerMask)
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

        Handles.color = Color.cyan;
        Handles.DrawLine(steerable.Position, steerable.Position + averageVelocity.normalized);
    }

    public static void DrawCohereWithNeighbours(ISteerable steerable, float neighbourhoodRadius, LayerMask layerMask)
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
        Handles.color = Color.magenta;
        Handles.DrawLine(steerable.Position, steerable.Position + cohesion.normalized);
    }

    public static void DrawSeperateFromNeighbours(ISteerable steerable, float neighbourhoodRadius, LayerMask layerMask)
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

        Handles.color = Color.red;
        Handles.DrawLine(steerable.Position, steerable.Position - averageDistance.normalized);
    }

    public static void DrawPursue(ISteerable steerable, ISteerable target)
    {
        float distance = (target.Position - steerable.Position).magnitude;
        float delta = 150 * Mathf.Min(1, distance / 5);
        Vector3 pursuitPosition = target.Position + target.Velocity * delta;
        Handles.color = Color.green;
        Handles.DrawWireDisc(pursuitPosition, Vector3.up, 0.25f);
    }

    public static void DrawEvade(ISteerable steerable, ISteerable target)
    {
        float distance = (target.Position - steerable.Position).magnitude;
        float delta = 150 * Mathf.Min(1, distance / 5);
        Vector3 evasionPosition = target.Position + target.Velocity * delta;
        Handles.color = Color.red;
        Handles.DrawWireDisc(evasionPosition, Vector3.up, 0.25f);
    }

    public static void DrawAvert(ISteerable steerable, float aversionDistance, float aversionRadius, LayerMask layerMask)
    {
        Ray ray = new Ray(steerable.Position, steerable.Velocity.normalized);

        RaycastHit[] raycastHits = Physics.RaycastAll(ray, aversionDistance, layerMask);

        Handles.color = Color.red;
        Handles.DrawLine(steerable.Position, steerable.Position + (ray.direction * aversionDistance));
        Handles.DrawWireDisc(steerable.Position, Vector3.up, aversionRadius);

        if (raycastHits.Length > 0)
        {
            foreach (RaycastHit raycastHit in raycastHits)
            {
                Collider collider = raycastHit.collider;
                if (collider.TryGetComponent(out ISteerable other) && steerable == other)
                    continue;
                Bounds bounds = collider.bounds;
                Handles.DrawWireCube(bounds.center, bounds.size);
            }
        }
        else
        {
            Collider[] colliders = Physics.OverlapSphere(steerable.Position, aversionRadius, layerMask);
            foreach (Collider collider in colliders)
            {
                if (collider.TryGetComponent(out ISteerable other) && steerable == other)
                    continue;

                Bounds bounds = collider.bounds;
                Handles.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }

    public static void DrawWander(ISteerable steerable, float wanderAngle)
    {
        Vector3 arcStart = Quaternion.AngleAxis(-(wanderAngle / 2), Vector3.up) * steerable.Velocity.normalized;
        Vector3 arcEnd = Quaternion.AngleAxis(wanderAngle / 2, Vector3.up) * steerable.Velocity.normalized;
        Handles.color = Color.red;
        Handles.DrawWireArc(steerable.Position, Vector3.up, arcStart, wanderAngle, 1);
        Handles.DrawLine(steerable.Position, steerable.Position + arcStart);
        Handles.DrawLine(steerable.Position, steerable.Position + arcEnd);
    }

    public static void DrawWanderWithinCircle(ISteerable steerable, float wanderAngle, Vector3 circleCenter, float circleRadius)
    {
        DrawWander(steerable, wanderAngle);
        Handles.color = Color.red;
        Handles.DrawWireDisc(circleCenter, Vector3.up, circleRadius);
    }
}

#endif
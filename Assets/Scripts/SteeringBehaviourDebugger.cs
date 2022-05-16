using System;
using UnityEditor;
using UnityEngine;

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

    public static void DrawAvert(ISteerable steerable, float aversionDistance, float aversionRadius, LayerMask layerMask)
    {
        Ray ray = new Ray(steerable.Position, steerable.Velocity.normalized);

        RaycastHit[] raycastHits = Physics.RaycastAll(ray, aversionDistance, layerMask);

        Handles.color = Color.red;
        Handles.DrawLine(steerable.Position, steerable.Position + (ray.direction * aversionDistance));
        Handles.DrawWireDisc(steerable.Position, Vector3.up, aversionRadius);

        if (raycastHits.Length > 0)
        {
            RaycastHit raycastHit = raycastHits[0];
            Collider collider = raycastHit.collider;
            Bounds bounds = collider.bounds;
            Handles.DrawWireCube(bounds.center, bounds.size);
        }
        else
        {
            Collider[] colliders = Physics.OverlapSphere(steerable.Position, aversionRadius, layerMask);
            foreach (Collider collider in colliders)
            {
                Bounds bounds = collider.bounds;
                Handles.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }
}

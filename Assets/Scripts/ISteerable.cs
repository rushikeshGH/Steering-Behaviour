using UnityEngine;

public interface ISteerable
{
    Vector3 Position { get; set; }
    Vector3 Velocity { get; set; }
    Vector3 SteeringForce { get; set; }
    float MaximumVelocity { get; set; }
    float MaximumSteeringForce { get; set; }

    void Rotate();
}

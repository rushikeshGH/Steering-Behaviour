using UnityEditor;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private float maximumVelocity;

    public Vector3 Position => transform.position;

    public Vector3 Velocity { get; private set; }

    private void Awake()
    {
        Velocity = transform.forward * maximumVelocity * Time.deltaTime;
    }

    private void Update()
    {
        Vector3 forward = Velocity.normalized;
        if (forward.magnitude != 0)
            transform.forward = forward;
        transform.position += Velocity;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        const int Thickness = 4;
        const int Scale = 2;

        DrawVelocityLine(Thickness, Scale);
    }

    private Vector3 DrawVelocityLine(int Thickness, int Scale)
    {
        Vector3 velocityEnd = Position + (Velocity.normalized * Scale);
        Handles.color = Color.blue;
        Handles.DrawLine(Position, velocityEnd, Thickness);
        return velocityEnd;
    }
}

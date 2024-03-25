using UnityEngine;

public class OBB : MonoBehaviour
{
    public float objectMass = 1;
    public float restitution = 0.7f;
    public Vector3 size = Vector3.one;
    public Vector3 centerOffset = Vector3.zero;
    private Vector3 velocity;
    private Vector3 angularVelocity;

    // Additional variable to track all OBBs in the scene
    private static OBB[] allOBBs;

    private void Awake()
    {
        // On Awake, find all OBB components in the scene
        allOBBs = FindObjectsOfType<OBB>();
    }

    private void FixedUpdate()
    {
        CheckForCollisions();
    }

    private void Update()
    {

    }

    private void CheckForCollisions()
    {
        foreach (var other in allOBBs)
        {
            if (other != this && IsCollidingWith(other))
            {
                OnCollisionDetected(other, CalculateMTD(other));
                Debug.Log($"{gameObject.name} is colliding with {other.gameObject.name}");
            }
        }
    }

    private bool IsCollidingWith(OBB other)
    {
        // Convert this OBB's local axes to world space
        Vector3[] axes1 = new Vector3[]
        {
            transform.right,
            transform.up,
            transform.forward
        };

        // Convert the other OBB's local axes to world space
        Vector3[] axes2 = new Vector3[]
        {
            other.transform.right,
            other.transform.up,
            other.transform.forward
        };

        // Get vertices of both OBBs in world space
        Vector3[] thisVertices = GetWorldSpaceVertices();
        Vector3[] otherVertices = other.GetWorldSpaceVertices();

        // Check all axes for both OBBs
        foreach (var axis in axes1)
        {
            if (!OverlapOnAxis(thisVertices, otherVertices, axis))
                return false;
        }
        foreach (var axis in axes2)
        {
            if (!OverlapOnAxis(thisVertices, otherVertices, axis))
                return false;
        }

        // Check cross product of edges (for non-parallel axes)
        foreach (var axis1 in axes1)
        {
            foreach (var axis2 in axes2)
            {
                Vector3 crossAxis = Vector3.Cross(axis1, axis2);
                if (crossAxis != Vector3.zero && !OverlapOnAxis(thisVertices, otherVertices, crossAxis.normalized))
                    return false;
            }
        }

        // If all axes have overlap, then OBBs are colliding
        return true;
    }

    private Vector3 CalculateMTD(OBB other)
    {
        float minOverlap = float.MaxValue;
        Vector3 mtd = Vector3.zero;

        // Convert this OBB's local axes to world space
        Vector3[] axes1 = new Vector3[]
        {
        transform.right,
        transform.up,
        transform.forward
        };

        // Convert the other OBB's local axes to world space
        Vector3[] axes2 = new Vector3[]
        {
        other.transform.right,
        other.transform.up,
        other.transform.forward
        };

        // Get vertices of both OBBs in world space
        Vector3[] thisVertices = GetWorldSpaceVertices();
        Vector3[] otherVertices = other.GetWorldSpaceVertices();

        // Check all axes for both OBBs
        foreach (var axis in axes1)
        {
            float overlap = GetOverlapOnAxis(thisVertices, otherVertices, axis);
            if (overlap <= 0)
                return Vector3.zero; // No collision or no overlap on this axis

            if (overlap < minOverlap)
            {
                minOverlap = overlap;
                mtd = axis * overlap;
            }
        }
        foreach (var axis in axes2)
        {
            float overlap = GetOverlapOnAxis(thisVertices, otherVertices, axis);
            if (overlap <= 0)
                return Vector3.zero; // No collision or no overlap on this axis

            if (overlap < minOverlap)
            {
                minOverlap = overlap;
                mtd = axis * overlap;
            }
        }

        // Check cross product of edges (for non-parallel axes)
        foreach (var axis1 in axes1)
        {
            foreach (var axis2 in axes2)
            {
                Vector3 crossAxis = Vector3.Cross(axis1, axis2);
                if (crossAxis != Vector3.zero)
                {
                    float overlap = GetOverlapOnAxis(thisVertices, otherVertices, crossAxis.normalized);
                    if (overlap <= 0)
                        return Vector3.zero; // No collision or no overlap on this axis

                    if (overlap < minOverlap)
                    {
                        minOverlap = overlap;
                        mtd = crossAxis.normalized * overlap;
                    }
                }
            }
        }

        return mtd;
    }

    private float GetOverlapOnAxis(Vector3[] verticesA, Vector3[] verticesB, Vector3 axis)
    {
        // Project all vertices on the axis and find the min and max
        float minA = float.MaxValue, maxA = float.MinValue;
        float minB = float.MaxValue, maxB = float.MinValue;

        foreach (var vertex in verticesA)
        {
            float projection = Vector3.Dot(vertex, axis);
            minA = Mathf.Min(minA, projection);
            maxA = Mathf.Max(maxA, projection);
        }

        foreach (var vertex in verticesB)
        {
            float projection = Vector3.Dot(vertex, axis);
            minB = Mathf.Min(minB, projection);
            maxB = Mathf.Max(maxB, projection);
        }

        // Check for overlap
        return Mathf.Min(maxA - minB, maxB - minA);
    }


    private Vector3[] GetWorldSpaceVertices()
    {
        Matrix4x4 localToWorld = transform.localToWorldMatrix;
        Bounds bounds = new Bounds(centerOffset, size);
        Vector3[] vertices = GetOBBVertices(bounds);

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = localToWorld.MultiplyPoint3x4(vertices[i]);
        }

        return vertices;
    }

    private bool OverlapOnAxis(Vector3[] verticesA, Vector3[] verticesB, Vector3 axis)
    {
        // Project all vertices on the axis and find the min and max
        float minA = float.MaxValue, maxA = float.MinValue;
        float minB = float.MaxValue, maxB = float.MinValue;

        foreach (var vertex in verticesA)
        {
            float projection = Vector3.Dot(vertex, axis);
            minA = Mathf.Min(minA, projection);
            maxA = Mathf.Max(maxA, projection);
        }

        foreach (var vertex in verticesB)
        {
            float projection = Vector3.Dot(vertex, axis);
            minB = Mathf.Min(minB, projection);
            maxB = Mathf.Max(maxB, projection);
        }

        // Check for overlap
        return maxA >= minB && maxB >= minA;
    }

    private void OnCollisionDetected(OBB other, Vector3 mtd)
    {
        // Normalized direction of the collision
        Vector3 collisionNormal = mtd.normalized;

        // Calculate relative velocity in the direction of the collision normal
        Vector3 relativeVelocity = other.velocity - velocity;
        float velocityAlongNormal = Vector3.Dot(relativeVelocity, collisionNormal);

        // Do not resolve if velocities are separating
        if (velocityAlongNormal > 0) return;

        // Calculate restitution (elasticity) of the collision
        float restitution = Mathf.Min(this.restitution, other.restitution); // Assuming restitution is defined in both objects

        // Calculate impulse scalar
        float impulseScalar = -(1 + restitution) * velocityAlongNormal;
        impulseScalar /= (1 / this.objectMass) + (1 / other.objectMass);

        // Apply impulse to the objects
        Vector3 impulse = impulseScalar * collisionNormal;
        velocity -= (1 / this.objectMass) * impulse;
        other.velocity += (1 / other.objectMass) * impulse;

        // Move objects out of collision based on their mass ratio
        float totalMass = this.objectMass + other.objectMass;
        transform.position -= mtd * (this.objectMass / totalMass);
        other.transform.position += mtd * (other.objectMass / totalMass);
    }


    private void ApplyMomentum(Vector3 momentum, Vector3 angularMomentum)
    {
        // Apply translational momentum to the object
        velocity += momentum / objectMass;

        // Apply angular momentum to the object
        angularVelocity += angularMomentum / objectMass;
    }

    private void OnDrawGizmosSelected()
    {
        // Get the bounds of the OBB
        Bounds bounds = new Bounds(centerOffset, size);

        // Draw the OBB
        DrawOBB(bounds, transform.localToWorldMatrix);
    }

    private void DrawOBB(Bounds bounds, Matrix4x4 matrix)
    {
        Vector3[] corners = GetOBBVertices(bounds);

        // Transform the corners to world space
        for (int i = 0; i < 8; i++)
        {
            corners[i] = matrix.MultiplyPoint3x4(corners[i]);
        }

        // Draw the lines of the OBB
        Gizmos.color = Color.cyan;
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
            Gizmos.DrawLine(corners[i + 4], corners[((i + 1) % 4) + 4]);
            Gizmos.DrawLine(corners[i], corners[i + 4]);
        }
    }

    private Vector3[] GetOBBVertices(Bounds bounds)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        Vector3[] corners = new Vector3[8];
        corners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
        corners[1] = center + new Vector3(-extents.x, -extents.y, extents.z);
        corners[2] = center + new Vector3(extents.x, -extents.y, extents.z);
        corners[3] = center + new Vector3(extents.x, -extents.y, -extents.z);
        corners[4] = center + new Vector3(-extents.x, extents.y, -extents.z);
        corners[5] = center + new Vector3(-extents.x, extents.y, extents.z);
        corners[6] = center + new Vector3(extents.x, extents.y, extents.z);
        corners[7] = center + new Vector3(extents.x, extents.y, -extents.z);

        return corners;
    }
}

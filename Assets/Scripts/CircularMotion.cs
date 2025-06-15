using UnityEngine;

/// <summary>
/// This component moves the GameObject it is attached to in various paths.
/// It supports 2D circular motion on the XZ plane and 3D spherical motion.
/// </summary>
public class CircularMotion : MonoBehaviour
{
    // Enum to define the type of movement path.
    public enum MovementType { Circle, Sphere }
    // Enum to define the rotation direction. The integer values are used as multipliers.
    public enum RotationDirection { CounterClockwise = 1, Clockwise = -1 }

    [Header("Movement Path")]
    [Tooltip("The type of path the object will follow.")]
    [SerializeField] private MovementType movementType = MovementType.Circle;

    [Header("Movement Settings")]
    [Tooltip("The speed at which the object moves along its path.")]
    [SerializeField] private float speed = 2f;

    [Tooltip("The radius of the circular or spherical path.")]
    [SerializeField] private float radius = 3f;

    [Tooltip("The direction of rotation.")]
    [SerializeField] private RotationDirection direction = RotationDirection.CounterClockwise;

    [Header("Orientation")]
    [Tooltip("If true, the object will always face the center of its path.")]
    [SerializeField] private bool lookAtCenter = false;

    // The point around which the object will orbit.
    private Vector3 _centrePoint;

    // Angles used to calculate the position on the circle or sphere.
    private float _horizontalAngle;
    private float _verticalAngle;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// We use this to establish the center of our path based on
    /// where the object is placed in the scene.
    /// </summary>
    void Start()
    {
        // Set the center point for the path to the object's initial position.
        _centrePoint = transform.position;
    }

    /// <summary>
    /// Update is called once per frame. We use it to calculate and update
    /// the object's position to keep it moving along the selected path.
    /// </summary>
    void Update()
    {
        // Increment angles over time to create continuous motion.
        // The direction is cast to an int (1 or -1) to control the rotation.
        _horizontalAngle += (float)direction * speed * Time.deltaTime;
        
        // We vary the vertical angle speed slightly for a more dynamic spherical path.
        _verticalAngle += speed * 0.75f * Time.deltaTime;

        // Calculate the new position based on the chosen movement type.
        switch (movementType)
        {
            case MovementType.Circle:
                CalculateCircularPosition();
                break;
            case MovementType.Sphere:
                CalculateSphericalPosition();
                break;
        }

        // Optionally make the object look towards the center of its orbit.
        if (lookAtCenter)
        {
            transform.LookAt(_centrePoint);
        }
    }

    /// <summary>
    /// Calculates the object's new position for a 2D circular path.
    /// </summary>
    private void CalculateCircularPosition()
    {
        float x = Mathf.Cos(_horizontalAngle) * radius;
        float z = Mathf.Sin(_horizontalAngle) * radius;

        // Create the new position vector on the XZ plane, using the original Y position.
        Vector3 newPosition = new Vector3(_centrePoint.x + x, _centrePoint.y, _centrePoint.z + z);

        transform.position = newPosition;
    }

    /// <summary>
    /// Calculates the object's new position for a 3D spherical path.
    /// </summary>
    private void CalculateSphericalPosition()
    {
        // Using spherical coordinate formulas to calculate X, Y, and Z.
        // _horizontalAngle acts as the azimuth.
        // _verticalAngle acts as the polar angle.
        float x = radius * Mathf.Sin(_verticalAngle) * Mathf.Cos(_horizontalAngle);
        float y = radius * Mathf.Cos(_verticalAngle);
        float z = radius * Mathf.Sin(_verticalAngle) * Mathf.Sin(_horizontalAngle);

        // The new position is an offset from the original center point.
        transform.position = _centrePoint + new Vector3(x, y, z);
    }
}

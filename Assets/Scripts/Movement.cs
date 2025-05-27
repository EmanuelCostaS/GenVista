using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 7f;
    [SerializeField] float groundDrag = 5f; // This will use rb.drag for a linear damping effect when grounded
    [SerializeField] float jumpForce = 12f;
    [SerializeField] float jumpCooldown = 0.25f;
    [SerializeField] float airMultiplier = 0.4f; // Affects horizontal movement control in air
    bool readyToJump;

    [Header("Flying")]
    [SerializeField] float flySpeed = 5f; // Speed for vertical flight
    private bool isFlyingMode = false; // Tracks if the player is in active flight mode

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;     // Jump on ground, fly up / initiate flight in air
    public KeyCode descendKey = KeyCode.LeftShift; // Fly down

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found on the player!", this);
            enabled = false;
            return;
        }
        rb.freezeRotation = true;
        ResetJump();

        if (orientation == null)
        {
            Debug.LogError("Orientation Transform is not assigned in the Inspector!", this);
        }
    }

    private void Update()
    {
        if (orientation == null) return;

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl(); // Controls max horizontal speed using rb.velocity
        HandleDrag();   // Applies groundDrag using rb.drag

        // Manage flying state and gravity
        if (grounded)
        {
            isFlyingMode = false; // Exit flying mode when grounded
        }
        else
        {
            // If jump key is pressed in air, initiate or continue flying mode
            if (Input.GetKey(jumpKey))
            {
                isFlyingMode = true;
            }
        }

        // Apply or remove gravity based on flying mode
        if (isFlyingMode)
        {
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true; // Gravity active if grounded or just falling (not in flight mode)
        }
    }

    private void FixedUpdate()
    {
        if (orientation == null) return;
        MovePlayer(); // Handles movement forces and velocities
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
            isFlyingMode = false; // Ensure not in flying mode immediately after a ground jump
        }
    }

    private void HandleDrag()
    {
        if (grounded && !isFlyingMode) // Apply ground drag only when on ground and not trying to fly off it
        {
            rb.linearDamping  = groundDrag; // rb.drag provides linear damping
        }
        else
        {
            rb.linearDamping  = 0; // No drag in air or when flying
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Horizontal Movement (using AddForce for acceleration-based feel)
        if (grounded && !isFlyingMode) // Normal ground movement
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded) // Air control (applies whether flying or just falling)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
        // If grounded and trying to fly (isFlyingMode might become true), horizontal forces still apply.

        // Vertical Flying Movement
        if (isFlyingMode) // This implies !grounded generally, or just took off
        {
            float currentXVel = rb.linearVelocity.x; // Preserve horizontal velocity from AddForce
            float currentZVel = rb.linearVelocity.z;

            if (Input.GetKey(jumpKey)) // Fly Up
            {
                // Setting rb.velocity directly for precise vertical control
                rb.linearVelocity = new Vector3(currentXVel, flySpeed, currentZVel);
            }
            else if (Input.GetKey(descendKey)) // Fly Down
            {
                rb.linearVelocity = new Vector3(currentXVel, -flySpeed, currentZVel);
            }
            else // No vertical fly input, so hover
            {
                rb.linearVelocity = new Vector3(currentXVel, 0f, currentZVel);
            }
        }
        // If not isFlyingMode and !grounded, gravity (enabled in Update) handles falling.
        // Jump() in MyInput gives initial upward velocity for ground jumps.
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Limit horizontal velocity
        // This applies to velocity from AddForce, ensuring it doesn't exceed moveSpeed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            // Apply the limited horizontal velocity, preserving the current vertical velocity
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // Reset y for consistent jump
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down * (playerHeight * 0.5f + 0.2f));
    }
}
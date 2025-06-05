using UnityEngine;
using System.Collections;
public class Movement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 7f;
    [SerializeField] float groundDrag = 5f;
    [SerializeField] float jumpForce = 12f;
    [SerializeField] float jumpCooldown = 0.25f;
    [SerializeField] float airMultiplier = 0.4f;
    bool readyToJump;

    [Header("Flying")]
    [SerializeField] float flySpeed = 5f;
    public bool isFlyingMode = false;
    // private bool isTrajectoryMode = false; // This was in your script but not used, removed for brevity unless needed

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode descendKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask whatIsGround;
    public bool grounded;

    public Transform orientation;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;

    [HideInInspector] public bool useExternalInput = false;
    [HideInInspector] public float externalHorizontalInput = 0f;
    [HideInInspector] public float externalVerticalInput = 0f;
    [HideInInspector] public bool externalJumpKeyActive = false; // True if external "jump key" is held
    [HideInInspector] public bool externalDescendKeyActive = false; // True if external "descend key" is held
    private bool externalJumpTriggered = false; // For single press jump

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
            // Consider disabling if orientation is crucial and missing
            // enabled = false;
            // return;
        }
    }

    private void Update()
    {
        if (orientation == null && !useExternalInput) // Only critical if not externally controlled for basic movement
        {
            if (orientation == null)
            {
                // If still null, it's a persistent issue
                if(rb != null) rb.isKinematic = true; // Stop physics if control is impossible
                return;
            }
        }


        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        HandleDrag();

        // Manage flying state and gravity
        if (grounded)
        {
            isFlyingMode = false;
        }
        else // In Air
        {
            bool jumpPressed = useExternalInput ? externalJumpKeyActive : Input.GetKey(jumpKey);
            if (jumpPressed && !isFlyingMode) // If jump key is pressed in air for the first time, initiate flying
            {
                isFlyingMode = true;
            }
        }

        if (isFlyingMode)
        {
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;
        }
    }

    private void FixedUpdate()
    {
        // if (orientation == null && !useExternalInput) return; // Allow movement if externally controlled even without orientation
        MovePlayer();
    }

    // Public method to set a single jump trigger for external control
    public void TriggerExternalJump()
    {
        if (useExternalInput)
        {
            externalJumpTriggered = true;
        }
    }

    private void MyInput()
    {
        if (useExternalInput)
        {
            horizontalInput = externalHorizontalInput;
            verticalInput = externalVerticalInput;

            // Grounded jump logic for external control
            if (externalJumpTriggered && readyToJump && grounded)
            {
                readyToJump = false;
                Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
                isFlyingMode = false;
                externalJumpTriggered = false; // Consume the trigger
            }
        }
        else // Original Input
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");

            if (Input.GetKeyDown(jumpKey) && readyToJump && grounded) // Use GetKeyDown for ground jump
            {
                readyToJump = false;
                Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
                isFlyingMode = false;
            }
        }
    }

    private void HandleDrag()
    {
        if (grounded && !isFlyingMode)
        {
            rb.linearDamping = groundDrag; // Corrected to use rb.drag
        }
        else
        {
            rb.linearDamping = 0; // Corrected to use rb.drag
        }
    }

    private void MovePlayer()
    {
        // If orientation is missing, use world coordinates for external control as a fallback
        if (orientation != null)
        {
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        }
        else if (useExternalInput) // Fallback for external control if orientation is missing
        {
            moveDirection = Vector3.forward * verticalInput + Vector3.right * horizontalInput;
        } else {
            moveDirection = Vector3.zero; // No orientation, no external control, no movement
        }


        if (grounded && !isFlyingMode)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded) // Air control (applies whether flying or just falling, or actively flying horizontally)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        if (isFlyingMode)
        {
            float currentXVel = rb.linearVelocity.x; // Use rb.velocity
            float currentZVel = rb.linearVelocity.z; // Use rb.velocity

            bool flyUpActive = useExternalInput ? externalJumpKeyActive : Input.GetKey(jumpKey);
            bool flyDownActive = useExternalInput ? externalDescendKeyActive : Input.GetKey(descendKey);

            if (flyUpActive)
            {
                rb.linearVelocity = new Vector3(currentXVel, flySpeed, currentZVel); // Use rb.velocity
            }
            else if (flyDownActive)
            {
                rb.linearVelocity = new Vector3(currentXVel, -flySpeed, currentZVel); // Use rb.velocity
            }
            else // Hover or maintain horizontal movement
            {
                // Only set to zero if not also moving horizontally from AddForce.
                // AddForce still applies for horizontal, so we just need to counteract gravity / provide hover.
                // If no horizontal input, vertical velocity becomes 0 for hover.
                // If there IS horizontal input, AddForce above handles XZ, vertical is set to 0 here for hover.
                rb.linearVelocity = new Vector3(currentXVel, 0f, currentZVel); // Use rb.velocity
            }
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // Use rb.velocity

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z); // Use rb.velocity
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // Use rb.velocity
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (rb != null) // Check rb to avoid error if Start() fails early
        {
            Gizmos.DrawRay(transform.position, Vector3.down * (playerHeight * 0.5f + 0.2f));
        }
    }
}
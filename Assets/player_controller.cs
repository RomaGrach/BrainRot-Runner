using UnityEngine;

public class player_controller : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player Transform that will slide, yaw, and jump.")]
    public Transform playerTransform;

    [Header("Forward Movement")]
    [Tooltip("Forward movement speed (units per second).")]
    public float forwardSpeed = 5f;

    [Header("Lateral Movement")]
    [Tooltip("Speed of sliding between lanes (units per second).")]
    public float lateralSpeed = 10f;
    [Tooltip("Local X-offset for the left lane.")]
    public float leftLaneX = -2f;
    [Tooltip("Local X-offset for the right lane.")]
    public float rightLaneX = 2f;

    [Header("Turn/Yaw Settings")]
    [Tooltip("Maximum yaw angle (degrees) when changing lanes.")]
    public float yawAngle = 30f;
    [Tooltip("Speed (deg/sec) at which the player yaws into the turn.")]
    public float yawSpeed = 300f;
    [Tooltip("Speed (deg/sec) at which the player returns to forward facing.")]
    public float returnYawSpeed = 120f;

    [Header("Swipe Settings")]
    [Tooltip("Enable swipe-to-change-lanes on touch screens?")]
    public bool enableSwipe = true;
    [Tooltip("What fraction of screen width counts as a swipe?")]
    [Range(0.05f, 0.5f)]
    public float swipeThresholdFraction = 0.2f;

    [Header("Animation Settings")]
    [Tooltip("Animator component controlling player animations.")]
    public Animator animator;
    [Tooltip("Trigger name for jump animation.")]
    public string jumpTrigger = "Jump";
    [Tooltip("Trigger name for slide (dash) animation.")]
    public string dashTrigger = "Dash";
    [Tooltip("Trigger name for run animation.")]
    public string runTrigger = "Run";

    [Header("Jump Physics")]
    [Tooltip("Upward impulse applied when jumping.")]
    public float jumpForce = 8f;
    [Tooltip("Gravity acceleration (negative).")]
    public float gravity = -20f;
    [Tooltip("Raycast distance threshold to consider grounded.")]
    public float groundCheckDistance = 0.1f;
    [Tooltip("Layer mask for what is considered ground.")]
    public LayerMask groundLayer;
    [Tooltip("Minimum time between jumps (seconds).")]
    public float jumpCooldown = 1f;
    [Tooltip("Local offset from PlayerTransform for ground check origin.")]
    public Vector3 groundCheckOffset = new Vector3(0f, 0.1f, 0f);

    [Header("Debug Ground Info")]
    [Tooltip("Is the player currently on the ground?")]
    public bool inspectorIsGrounded;
    [Tooltip("Current distance from playerTransform down to ground.")]
    public float inspectorGroundDistance;

    // Internals
    private Vector3[] laneOffsets;
    private int currentLane = 1;
    private float targetYaw = 0f;

    private bool isSwiping = false;
    private Vector2 swipeStart;
    private float swipeThresholdX;
    private float swipeThresholdY;

    public float verticalVelocity = 0f;
    private bool isGrounded = false;
    private bool wasGrounded = false;
    private float lastJumpTime = -Mathf.Infinity;

    void Start()
    {
        laneOffsets = new Vector3[3]
        {
            new Vector3(leftLaneX, 0f, 0f),
            Vector3.zero,
            new Vector3(rightLaneX, 0f, 0f)
        };

        swipeThresholdX = Screen.width * swipeThresholdFraction;
        swipeThresholdY = Screen.height * swipeThresholdFraction;

        if (playerTransform == null) Debug.LogError("Assign playerTransform in inspector.");
        if (animator == null) Debug.LogError("Assign animator in inspector.");
    }

    void Update()
    {
        HandleInput();
        CheckGround();
        ApplyVerticalMovement();
        MoveForward();      // only GameObject with script moves forward
        MoveLateral();      // only playerTransform moves sideways
        ApplyYaw();         // only playerTransform yaws

        // Debug visual ray
        Vector3 origin = playerTransform.position + groundCheckOffset;
        Vector3 dir = Vector3.down;
        Debug.DrawRay(origin, dir * inspectorGroundDistance, isGrounded ? Color.green : Color.red);

        inspectorIsGrounded = isGrounded;
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            BeginLaneChange(-1);
            Debug.Log("Lane change left: new lane " + currentLane);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            BeginLaneChange(1);
            Debug.Log("Lane change right: new lane " + currentLane);
        }
        else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            TryJump();
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            animator.SetTrigger(dashTrigger);
            Debug.Log("Slide triggered");
        }

        if (!enableSwipe || Input.touchCount == 0) return;
        Touch t = Input.GetTouch(0);
        if (t.phase == TouchPhase.Began)
        {
            swipeStart = t.position;
            isSwiping = true;
        }
        else if (t.phase == TouchPhase.Moved && isSwiping)
        {
            Vector2 delta = t.position - swipeStart;
            if (Mathf.Abs(delta.y) > swipeThresholdY && Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
            {
                if (delta.y > 0) TryJump();
                else { animator.SetTrigger(dashTrigger); Debug.Log("Slide triggered by swipe"); }
                isSwiping = false;
            }
            else if (Mathf.Abs(delta.x) > swipeThresholdX)
            {
                int dir = delta.x > 0 ? 1 : -1;
                BeginLaneChange(dir);
                Debug.Log("Lane change " + (dir > 0 ? "right" : "left") +
                          " by swipe: new lane " + currentLane);
                isSwiping = false;
            }
        }
        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            isSwiping = false;
    }

    private void TryJump()
    {
        if (isGrounded && Time.time - lastJumpTime >= jumpCooldown)
        {
            verticalVelocity = jumpForce;
            lastJumpTime = Time.time;
            animator.SetTrigger(jumpTrigger);
            Debug.Log("Jump triggered");
        }
        else if (!isGrounded)
        {
            Debug.Log("Cannot jump: not grounded");
        }
    }

    private void CheckGround()
    {
        wasGrounded = isGrounded;
        RaycastHit hit;
        Vector3 origin = playerTransform.position + groundCheckOffset;
        if (Physics.Raycast(origin, Vector3.down, out hit, Mathf.Infinity, groundLayer))
        {
            inspectorGroundDistance = hit.distance;
            isGrounded = hit.distance <= groundCheckDistance;
            Debug.Log($"Ray hit {hit.collider.name} at distance {hit.distance:F2}");
        }
        else
        {
            inspectorGroundDistance = Mathf.Infinity;
            isGrounded = false;
            Debug.Log("Ray did not hit ground layer");
        }

        if (!wasGrounded && isGrounded)
        {
            Debug.Log("Player has landed on the ground.");
            // stop jump animation, start run
            animator.ResetTrigger(jumpTrigger);
            animator.SetTrigger(runTrigger);
        }
        else if (wasGrounded && !isGrounded)
        {
            Debug.Log("Player has left the ground.");
        }

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = 0f;
        }
    }

    private void ApplyVerticalMovement()
    {
        if (!isGrounded)
            verticalVelocity += gravity * Time.deltaTime;

        Vector3 newPos = playerTransform.position + Vector3.up * verticalVelocity * Time.deltaTime;
        playerTransform.position = newPos;
    }

    private void MoveForward()
    {
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.Self);
    }

    private void BeginLaneChange(int dir)
    {
        currentLane = Mathf.Clamp(currentLane + dir, 0, 2);
        targetYaw = yawAngle * dir;
    }

    private void MoveLateral()
    {
        if (playerTransform == null) return;
        Vector3 lp = playerTransform.localPosition;
        float newX = Mathf.MoveTowards(lp.x, laneOffsets[currentLane].x, lateralSpeed * Time.deltaTime);
        playerTransform.localPosition = new Vector3(newX, lp.y, lp.z);
        if (Mathf.Approximately(newX, laneOffsets[currentLane].x)) targetYaw = 0f;
    }

    private void ApplyYaw()
    {
        if (playerTransform == null) return;
        float speed = Mathf.Approximately(targetYaw, 0f) ? returnYawSpeed : yawSpeed;
        Quaternion cur = playerTransform.localRotation;
        Quaternion dst = Quaternion.Euler(0f, targetYaw, 0f);
        playerTransform.localRotation = Quaternion.RotateTowards(cur, dst, speed * Time.deltaTime);
    }
}

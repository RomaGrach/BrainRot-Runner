using UnityEngine;

public class player_controller : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player Transform that will slide and yaw.")]
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

    // Internals
    private Vector3[] laneOffsets;    // local X positions: [0]=left,1=middle,2=right
    private int currentLane = 1;
    private float targetYaw = 0f;   // desired yaw angle
    private bool isSwiping = false;
    private Vector2 swipeStart;
    private float swipeThreshold;

    void Start()
    {
        // Prepare lane offsets (local to this GameObject)
        laneOffsets = new Vector3[3]
        {
            new Vector3(leftLaneX, 0f, 0f),
            Vector3.zero,
            new Vector3(rightLaneX, 0f, 0f)
        };

        swipeThreshold = Screen.width * swipeThresholdFraction;

        if (playerTransform == null)
            Debug.LogError("Player Transform not assigned in player_controller.");
    }

    void Update()
    {
        HandleInput();
        MoveForward();
        MoveLateral();
        ApplyYaw();
    }

    private void HandleInput()
    {
        // Keyboard input
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            BeginLaneChange(-1);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            BeginLaneChange(+1);

        // Swipe input
        if (enableSwipe && Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                swipeStart = t.position;
                isSwiping = true;
            }
            else if (t.phase == TouchPhase.Moved && isSwiping)
            {
                Vector2 delta = t.position - swipeStart;
                if (Mathf.Abs(delta.x) > swipeThreshold)
                {
                    BeginLaneChange(delta.x > 0 ? +1 : -1);
                    isSwiping = false;
                }
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                isSwiping = false;
            }
        }
    }

    private void BeginLaneChange(int direction)
    {
        // Update lane index (0..2)
        currentLane = Mathf.Clamp(currentLane + direction, 0, 2);
        // Set target yaw into the turn (positive = turn right)
        targetYaw = yawAngle * direction;
    }

    private void MoveForward()
    {
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.Self);
    }

    private void MoveLateral()
    {
        if (playerTransform == null) return;

        // Smoothly slide player local X to the target lane
        Vector3 localPos = playerTransform.localPosition;
        float targetX = laneOffsets[currentLane].x;
        localPos.x = Mathf.MoveTowards(localPos.x, targetX, lateralSpeed * Time.deltaTime);
        playerTransform.localPosition = localPos;

        // Once at the lane, reset target yaw to zero
        if (Mathf.Approximately(localPos.x, targetX))
            targetYaw = 0f;
    }

    private void ApplyYaw()
    {
        if (playerTransform == null) return;

        // Choose rotation speed: into turn or return
        float speed = Mathf.Approximately(targetYaw, 0f)
            ? returnYawSpeed
            : yawSpeed;

        // Current local rotation
        Quaternion current = playerTransform.localRotation;
        // Desired yaw around Y-axis
        Quaternion desired = Quaternion.Euler(0f, targetYaw, 0f);
        // Rotate toward desired
        playerTransform.localRotation = Quaternion.RotateTowards(
            current, desired, speed * Time.deltaTime
        );
    }
}

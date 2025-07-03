using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform визуальной модели (используется только для поворота/наклона).")]
    public Transform playerTransform;
    [Tooltip("Коллайдер, активный во время бега и прыжка.")]
    public Collider runningCollider;
    [Tooltip("Коллайдер, активный во время слайда (dash).")]
    public Collider slidingCollider;

    [Header("Forward Movement")]
    [Tooltip("Скорость движения вперёд (единиц в секунду).")]
    public float forwardSpeed = 5f;

    [Header("Time Acceleration")]
    [Tooltip("Начальный множитель времени (Time.timeScale).")]
    public float initialTimeScale = 1f;
    [Tooltip("Скорость, с которой ускоряется время (единиц timeScale в секунду).")]
    public float timeAccelerationRate = 0.1f;
    [Tooltip("Максимальный множитель времени.")]
    public float maxTimeScale = 3f;
    [Tooltip("Текущий множитель времени (для отображения в инспекторе).")]
    public float currentTimeScale;

    [Header("Speed Acceleration (disabled)")]
    [Tooltip("—")]
    public float baseSpeed = 5f; // оставлено для сброса скорости при необходимости

    [Header("Obstacle Handling")]
    [Tooltip("Tag препятствий (на них должен стоять BoxCollider с isTrigger).")]
    public string barrierTag = "Barrier";
    [Tooltip("Сообщение при попадании в консоль.")]
    public string barrierHitMessage = "Hit barrier!";

    [Header("Lateral Movement")]
    [Tooltip("Скорость смены дорожки влево/вправо (единиц в секунду).")]
    public float lateralSpeed = 10f;
    [Tooltip("Смещение по X для левой дорожки.")]
    public float leftLaneX = -2f;
    [Tooltip("Смещение по X для правой дорожки.")]
    public float rightLaneX = 2f;

    [Header("Turn/Yaw Settings")]
    [Tooltip("Максимальный угол наклона (градусов) при смене дорожки.")]
    public float yawAngle = 30f;
    [Tooltip("Скорость поворота модели (градусов в секунду).")]
    public float yawSpeed = 300f;
    [Tooltip("Скорость возврата модели в прямое положение (градусов в секунду).")]
    public float returnYawSpeed = 120f;

    [Header("Swipe Settings")]
    [Tooltip("Разрешить свайпы для смены дорожек.")]
    public bool enableSwipe = true;
    [Range(0.05f, 0.5f)]
    [Tooltip("Доля экрана для распознавания свайпа.")]
    public float swipeThresholdFraction = 0.2f;

    [Header("Animation Settings")]
    [Tooltip("Animator для управления анимациями.")]
    public Animator animator;
    public string jumpTrigger = "Jump";
    public string dashTrigger = "Dash";
    public string runTrigger = "Run";

    [Header("Jump Physics")]
    public float jumpForce = 8f;
    public float gravity = -20f;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;
    public float jumpCooldown = 1f;
    public Vector3 groundCheckOffset = new Vector3(0f, 0.1f, 0f);

    [Header("Collider Timing")]
    [Tooltip("Сколько секунд активен коллайдер для слайда.")]
    public float slidingColliderDuration = 1f;
    [Tooltip("Задержка перед реактивацией бегового коллайдера.")]
    public float runningColliderReactivateDelay = 0f;

    [Header("Debug Info")]
    public bool inspectorIsGrounded;
    public float inspectorGroundDistance;

    // Внутренние
    private Vector3[] laneOffsets;
    private int currentLane = 1;
    private float targetYaw = 0f;

    private bool isSwiping = false;
    private Vector2 swipeStart;
    private float swipeThresholdX;
    private float swipeThresholdY;

    private float verticalVelocity = 0f;
    private bool isGrounded = false, wasGrounded = false;
    private float lastJumpTime = -Mathf.Infinity;
    private Coroutine colliderSwitchRoutine;

    void Start()
    {
        // Инициализируем дорожки
        laneOffsets = new Vector3[3]
        {
            new Vector3(leftLaneX, 0f, 0f),
            Vector3.zero,
            new Vector3(rightLaneX, 0f, 0f)
        };

        // Настраиваем пороги свайпа
        swipeThresholdX = Screen.width * swipeThresholdFraction;
        swipeThresholdY = Screen.height * swipeThresholdFraction;

        // Проверяем ссылки
        if (playerTransform == null) Debug.LogError("Assign playerTransform in inspector.");
        if (animator == null) Debug.LogError("Assign animator in inspector.");
        if (runningCollider == null) Debug.LogError("Assign runningCollider in inspector.");
        if (slidingCollider == null) Debug.LogError("Assign slidingCollider in inspector.");

        // Начальные коллайдеры
        runningCollider.enabled = true;
        slidingCollider.enabled = false;

        // Инициализируем timeScale
        Time.timeScale = initialTimeScale;
        currentTimeScale = Time.timeScale;

        // Сохраняем базовую скорость
        baseSpeed = forwardSpeed;
    }

    void Update()
    {
        HandleInput();
        CheckGround();
        ApplyVerticalMovement();

        AccelerateTime();   // ускоряем внутриигровое время
        MoveForward();
        MoveLateral();
        ApplyYaw();

        // Отладочный луч
        Vector3 origin = transform.position + groundCheckOffset;
        Debug.DrawRay(origin, Vector3.down * inspectorGroundDistance,
                      isGrounded ? Color.green : Color.red);
        inspectorIsGrounded = isGrounded;
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            BeginLaneChange(-1);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            BeginLaneChange(1);

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            TryJump();

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            animator.SetTrigger(dashTrigger);
            StartColliderSwitch();
        }

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
                if (Mathf.Abs(delta.y) > swipeThresholdY &&
                    Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                {
                    if (delta.y > 0) TryJump();
                    else
                    {
                        animator.SetTrigger(dashTrigger);
                        StartColliderSwitch();
                    }
                    isSwiping = false;
                }
                else if (Mathf.Abs(delta.x) > swipeThresholdX)
                {
                    BeginLaneChange(delta.x > 0 ? 1 : -1);
                    isSwiping = false;
                }
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                isSwiping = false;
        }
    }

    private void TryJump()
    {
        if (isGrounded && Time.time - lastJumpTime >= jumpCooldown)
        {
            verticalVelocity = jumpForce;
            lastJumpTime = Time.time;
            animator.SetTrigger(jumpTrigger);
        }
    }

    private void CheckGround()
    {
        wasGrounded = isGrounded;
        RaycastHit hit;
        Vector3 origin = transform.position + groundCheckOffset;
        if (Physics.Raycast(origin, Vector3.down, out hit, Mathf.Infinity, groundLayer))
        {
            inspectorGroundDistance = hit.distance;
            isGrounded = hit.distance <= groundCheckDistance;
        }
        else
        {
            inspectorGroundDistance = Mathf.Infinity;
            isGrounded = false;
        }

        if (!wasGrounded && isGrounded)
        {
            animator.ResetTrigger(jumpTrigger);
            animator.SetTrigger(runTrigger);
        }

        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = 0f;
    }

    private void ApplyVerticalMovement()
    {
        if (!isGrounded)
            verticalVelocity += gravity * Time.deltaTime;
        transform.position += Vector3.up * verticalVelocity * Time.deltaTime;
    }

    private void AccelerateTime()
    {
        // Увеличиваем Time.timeScale по unscaledDeltaTime
        Time.timeScale += timeAccelerationRate * Time.unscaledDeltaTime;
        Time.timeScale = Mathf.Min(Time.timeScale, maxTimeScale);
        currentTimeScale = Time.timeScale;
    }

    private void MoveForward()
    {
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.Self);
    }

    private void BeginLaneChange(int dir)
    {
        currentLane = Mathf.Clamp(currentLane + dir, 0, laneOffsets.Length - 1);
        targetYaw = yawAngle * dir;
    }

    private void MoveLateral()
    {
        Vector3 pos = transform.localPosition;
        pos.x = Mathf.MoveTowards(pos.x, laneOffsets[currentLane].x, lateralSpeed * Time.deltaTime);
        transform.localPosition = pos;
        if (Mathf.Approximately(pos.x, laneOffsets[currentLane].x))
            targetYaw = 0f;
    }

    private void ApplyYaw()
    {
        if (playerTransform == null) return;
        float speed = Mathf.Approximately(targetYaw, 0f) ? returnYawSpeed : yawSpeed;
        Quaternion desired = Quaternion.Euler(0f, targetYaw, 0f);
        playerTransform.localRotation = Quaternion.RotateTowards(
            playerTransform.localRotation, desired, speed * Time.deltaTime
        );
    }

    private void StartColliderSwitch()
    {
        if (colliderSwitchRoutine != null)
            StopCoroutine(colliderSwitchRoutine);
        colliderSwitchRoutine = StartCoroutine(SlideColliderRoutine());
    }

    private IEnumerator SlideColliderRoutine()
    {
        runningCollider.enabled = false;
        slidingCollider.enabled = true;
        yield return new WaitForSeconds(slidingColliderDuration);
        slidingCollider.enabled = false;
        yield return new WaitForSeconds(runningColliderReactivateDelay);
        runningCollider.enabled = true;
        colliderSwitchRoutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(barrierTag))
        {
            Debug.Log(barrierHitMessage + " (" + other.name + ")");
            Time.timeScale = initialTimeScale;
            currentTimeScale = Time.timeScale;
        }
    }
}

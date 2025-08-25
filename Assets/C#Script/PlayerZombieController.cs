using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerZombieController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float dashSpeed = 5f;
    public FixedJoystick joystick;
    public Rigidbody rb;

    [Header("Animation")]
    public Animator animator;

    [Header("Dash")]
    public Button dashButton;
    public CanvasGroup dashButtonCanvasGroup;
    public float dashDuration = 5f;
    public float buttonHideTime = 3f;
    public float fadeDuration = 1f;

    [Header("Camera")]
    public CameraController cameraController;

    [Header("Attack")]
    public float attackCooldown = 1f;
    private float attackTimer = 0f;

    [Header("Wall/Climb Settings")]
    [SerializeField] private float maxClimbAngle = 45f;
    [SerializeField] private float rayLength = 0.6f;

    private bool isDashing = false;
    private float dashTimer = 0f;

    private Image dashImage;
    private Collider selfCollider;
    private readonly Dictionary<Collider, Coroutine> collisionIgnoreCoroutines = new Dictionary<Collider, Coroutine>();
    [SerializeField] private float collisionIgnoreDuration = 0.25f;

    private enum DashState { Ready, Dashing, Cooling }
    private DashState dashState = DashState.Ready;

    private bool fadeComplete = false;

    public enum TeamType { Blue, Yellow }
    public TeamType team;

    void Awake()
    {
        selfCollider = GetComponent<Collider>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        if (dashButton != null)
        {
            dashButton.onClick.AddListener(OnDashButtonPressed);

            if (dashButtonCanvasGroup == null)
                dashButtonCanvasGroup = dashButton.gameObject.AddComponent<CanvasGroup>();

            dashImage = dashButton.GetComponent<Image>();

            dashButtonCanvasGroup.alpha = 1f;
            dashButtonCanvasGroup.interactable = true;
            dashButtonCanvasGroup.blocksRaycasts = true;
            if (dashImage != null) dashImage.color = Color.red;
        }

        if (cameraController != null)
            cameraController.ResetCamera();
    }

    void Update()
    {
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (dashState == DashState.Dashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                dashState = DashState.Cooling;
                TryEnableDashButtonIfReady();
            }
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    // ===== Movement =====
    void HandleMovement()
    {
        if (Camera.main == null || joystick == null || rb == null) return;

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f; camRight.y = 0f;
        camForward.Normalize(); camRight.Normalize();

        Vector3 inputDir = camForward * joystick.Vertical + camRight * joystick.Horizontal;

        if (inputDir.magnitude > 0.1f)
        {
            inputDir.Normalize();
            float speed = isDashing ? dashSpeed : moveSpeed;

            if (!IsBlocked(inputDir))
            {
                rb.MovePosition(rb.position + inputDir * speed * Time.fixedDeltaTime);
                transform.rotation = Quaternion.LookRotation(inputDir);
            }

            animator.SetBool("isWalking", true);
            animator.SetBool("isDashing", isDashing);
            animator.speed = isDashing ? 2.0f : 1.2f;
        }
        else
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isDashing", false);
            animator.speed = 0.2f;
        }
    }

    bool IsBlocked(Vector3 direction)
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(origin, direction, out hit, rayLength))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle > maxClimbAngle) return true;
        }

        if (Physics.Raycast(origin, Vector3.down, out hit, rayLength))
        {
            float slope = Vector3.Angle(hit.normal, Vector3.up);
            if (slope > maxClimbAngle) return true;
        }

        return false;
    }

    // ===== Dash =====
    public void OnDashButtonPressed()
    {
        if (dashState != DashState.Ready) return;

        fadeComplete = false;
        if (dashButtonCanvasGroup != null)
        {
            dashButtonCanvasGroup.alpha = 0f;
            dashButtonCanvasGroup.interactable = false;
            dashButtonCanvasGroup.blocksRaycasts = false;
        }
        if (dashImage != null) dashImage.color = Color.white;

        isDashing = true;
        dashTimer = dashDuration;
        dashState = DashState.Dashing;

        StartCoroutine(DashButtonVisualRoutine());
    }

    private IEnumerator DashButtonVisualRoutine()
    {
        if (buttonHideTime > 0f)
            yield return new WaitForSeconds(buttonHideTime);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (dashButtonCanvasGroup != null)
                dashButtonCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        if (dashButtonCanvasGroup != null)
            dashButtonCanvasGroup.alpha = 1f;

        fadeComplete = true;
        TryEnableDashButtonIfReady();
    }

    private void TryEnableDashButtonIfReady()
    {
        if (dashState == DashState.Cooling && fadeComplete)
        {
            if (dashButtonCanvasGroup != null)
            {
                dashButtonCanvasGroup.interactable = true;
                dashButtonCanvasGroup.blocksRaycasts = true;
                dashButtonCanvasGroup.alpha = 1f;
            }
            if (dashImage != null) dashImage.color = Color.red;

            dashState = DashState.Ready;
        }
    }

    // ===== Attack / Infect =====
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Human")) return;

        // 反発を弱める
        if (rb != null) rb.linearVelocity = Vector3.zero; // ← 修正済み

        // 一時的に衝突無効化
        if (selfCollider != null && other != null && !collisionIgnoreCoroutines.ContainsKey(other))
        {
            Physics.IgnoreCollision(selfCollider, other, true);
            Coroutine c = StartCoroutine(ReenableCollisionAfterDelay(other));
            collisionIgnoreCoroutines.Add(other, c);
        }

        if (attackTimer <= 0f)
        {
            animator.SetTrigger("Attack");
            InfectionManager.Instance?.Infect(other.gameObject, false);
            attackTimer = attackCooldown;
        }
    }

    private IEnumerator ReenableCollisionAfterDelay(Collider otherCol)
    {
        yield return new WaitForSeconds(collisionIgnoreDuration);
        if (selfCollider != null && otherCol != null)
            Physics.IgnoreCollision(selfCollider, otherCol, false);
        collisionIgnoreCoroutines.Remove(otherCol);
    }

    // ===== Camera =====
    public void OnCameraResetButtonPressed()
    {
        if (cameraController != null)
            cameraController.ResetCamera();
    }
}

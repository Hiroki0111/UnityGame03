using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TutorialPlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float dashSpeed = 5f;
    [SerializeField] private FixedJoystick joystick;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;

    private bool isDashing = false;
    private float dashDuration = 5f;
    private float dashTimer = 0f;

    [Header("Wall/Climb Settings")]
    [SerializeField] private float maxClimbAngle = 45f; // この角度以上は登れない
    [SerializeField] private float rayLength = 0.6f;    // 足元と前方のRay距離

    [Header("Dash Button Settings")]
    [SerializeField] private Button dashButton;
    [SerializeField] private float buttonHideTime = 3f;
    [SerializeField] private float fadeDuration = 1f;

    private CanvasGroup dashCanvasGroup;
    private Image dashImage;

    // 追加: 自分のコライダー
    private Collider selfCollider;

    // 追加: 人間ごとに衝突を一時的に無効化する管理
    private Dictionary<Collider, Coroutine> collisionIgnoreCoroutines = new Dictionary<Collider, Coroutine>();
    [SerializeField] private float collisionIgnoreDuration = 0.25f; // 衝突を無効化する時間

    void Awake()
    {
        // ここで必須コンポーネントを確実に取得
        selfCollider = GetComponent<Collider>();
    }

    void Start()
    {
        if (dashButton != null)
        {
            dashCanvasGroup = dashButton.GetComponent<CanvasGroup>();
            if (dashCanvasGroup == null)
                dashCanvasGroup = dashButton.gameObject.AddComponent<CanvasGroup>();

            dashImage = dashButton.GetComponent<Image>();
            if (dashImage != null)
                dashImage.color = Color.red;

            dashButton.onClick.AddListener(OnDashButtonPressed);
        }
    }

    void OnDisable()
    {
        // 衝突無効化コルーチンを安全に停止してクリーンアップ
        foreach (var kvp in collisionIgnoreCoroutines)
        {
            if (kvp.Value != null)
                StopCoroutine(kvp.Value);
        }
        collisionIgnoreCoroutines.Clear();
    }

    void FixedUpdate()
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize(); camRight.Normalize();

        Vector3 input = camForward * joystick.Vertical + camRight * joystick.Horizontal;

        if (input.magnitude > 0.1f)
        {
            input.Normalize();
            float speed = isDashing ? dashSpeed : moveSpeed;

            if (!IsBlocked(input))
            {
                rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);
                transform.rotation = Quaternion.LookRotation(input);
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

        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                dashTimer = 0f;
            }
        }
    }

    public void StartDash()
    {
        if (!isDashing)
        {
            isDashing = true;
            dashTimer = dashDuration;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Human 以外は処理しない
        if (!other.CompareTag("Human")) return;

        // 吹っ飛ぶ挙動を抑制: 衝突反発を一時的に無効化
        rb.linearVelocity = Vector3.zero;

        // 同じ相手との衝突を一時的に無効化して衝突応答を抑制
        if (selfCollider != null && other != null && !collisionIgnoreCoroutines.ContainsKey(other))
        {
            Physics.IgnoreCollision(selfCollider, other, true);
            Coroutine c = StartCoroutine(ReenableCollisionAfterDelay(other));
            collisionIgnoreCoroutines.Add(other, c);
        }

        // 攻撃・感染処理は従来どおり実行
        animator.SetTrigger("Attack");
        bool infectedByCPU = false;
        InfectionManager.Instance?.Infect(other.gameObject, infectedByCPU);

        TutorialManager.Instance?.OnHumanInfected(!infectedByCPU);
    }

    // 衝突を一定時間後に元に戻す
    private IEnumerator ReenableCollisionAfterDelay(Collider otherCol)
    {
        yield return new WaitForSeconds(collisionIgnoreDuration);

        if (selfCollider != null && otherCol != null)
            Physics.IgnoreCollision(selfCollider, otherCol, false);

        collisionIgnoreCoroutines.Remove(otherCol);
    }

    bool IsBlocked(Vector3 direction)
    {
        RaycastHit hit;

        // 足元前方チェック
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(origin, direction, out hit, rayLength))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle > maxClimbAngle)
                return true;
        }

        // 足元チェック（段差）
        origin = transform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(origin, Vector3.down, out hit, rayLength))
        {
            float slope = Vector3.Angle(hit.normal, Vector3.up);
            if (slope > maxClimbAngle)
                return true;
        }

        return false;
    }

    // ------------------------------
    // Dash Button
    // ------------------------------
    public void OnDashButtonPressed()
    {
        StartDash();

        if (dashButton == null) return;

        dashCanvasGroup.alpha = 0f;
        dashCanvasGroup.interactable = false;
        dashCanvasGroup.blocksRaycasts = false;

        StartCoroutine(FadeButtonBack());
    }

    IEnumerator FadeButtonBack()
    {
        yield return new WaitForSeconds(buttonHideTime);

        if (dashImage != null)
            dashImage.color = Color.white;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            dashCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        dashCanvasGroup.alpha = 1f;

        dashCanvasGroup.interactable = true;
        dashCanvasGroup.blocksRaycasts = true;

        if (dashImage != null)
            dashImage.color = Color.red;
    }
}
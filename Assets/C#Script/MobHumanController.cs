using UnityEngine;
using UnityEngine.AI;  // NavMesh用API

// NavMeshの生成（ベイク）を管理するスクリプト
// これをNavMeshSurfaceが付いたGameObjectにアタッチします
[RequireComponent(typeof(NavMeshSurface))]
public class NavMeshSetup : MonoBehaviour
{
    private NavMeshSurface surface;

    void Awake()
    {
        // NavMeshSurfaceコンポーネントを取得
        surface = GetComponent<NavMeshSurface>();
        if (surface == null)
            Debug.LogError("NavMeshSurfaceがありません！");
    }

    void Start()
    {
        // シーン開始時にNavMeshをベイク（生成）する
        BakeNavMesh();
    }

    /// <summary>
    /// NavMeshを生成（ベイク）するメソッド
    /// </summary>
    public void BakeNavMesh()
    {
        if (surface != null)
        {
            Debug.Log("NavMeshのベイクを開始します...");
            surface.BuildNavMesh();
            Debug.Log("NavMeshのベイクが完了しました！");
        }
    }
}


// NavMeshAgentを使い、ゾンビから逃げる敵キャラの挙動を制御するスクリプト
public class MobHumanController : MonoBehaviour
{
    [Header("移動速度")]
    public float moveSpeed = 2f;        // 通常移動速度
    public float escapeSpeed = 5.5f;    // 逃走時の速度

    [Header("逃走時間")]
    public float escapeDuration = 5f;   // 逃げ続ける時間（秒）

    [Header("感知範囲")]
    public float detectRange = 5f;      // ゾンビを感知する範囲

    [Header("逃走地点")]
    public Transform escapeTarget;      // 逃げるべきポイント（Inspectorからセット）

    [Header("アニメーター")]
    public Animator animator;           // アニメーション制御用

    private NavMeshAgent agent;         // NavMeshAgentコンポーネント
    private Transform playerZombie;     // ゾンビのTransform

    private bool isEscaping = false;    // 現在逃げているかどうか
    private float escapeTimer = 0f;     // 逃走時間のカウントダウン

    // ▼ 追加（クラスの上部にフィールドを追加） ▼
    [Header("徘徊行動")]
    public float wanderRadius = 10f;           // 徘徊の半径
    public float wanderInterval = 5f;          // 徘徊地点変更の間隔
    private float wanderTimer = 0f;            // 徘徊用タイマー
    private Vector3 currentWanderTarget;       // 現在の徘徊目標地点

    void Start()
    {
        // プレイヤー（ゾンビ）をタグ検索で取得
        GameObject playerObj = GameObject.FindWithTag("zombie");
        if (playerObj != null)
            playerZombie = playerObj.transform;

        // NavMeshAgentを取得。必ずアタッチが必要
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.LogError("NavMeshAgentコンポーネントが必要です。");

        // 初期速度は通常速度に設定
        agent.speed = moveSpeed;
    }

    void Update()
    {
        if (playerZombie == null || escapeTarget == null) return;

        // ゾンビとの距離を計算
        float distToZombie = Vector3.Distance(transform.position, playerZombie.position);

        // ゾンビが感知範囲に入ったら逃走開始
        if (distToZombie < detectRange && !isEscaping)
        {
            isEscaping = true;
            escapeTimer = escapeDuration;
        }

        if (isEscaping)
        {
            EscapeBehavior();
        }
        else
        {
            NormalBehavior();
        }
    }

    /// <summary>
    /// 逃走時の挙動
    /// </summary>
    void EscapeBehavior()
    {
        // 逃走時間を減らす
        escapeTimer -= Time.deltaTime;

        // 逃走方向を計算（逃走地点に向かう）
        Vector3 escapeDir = (escapeTarget.position - transform.position).normalized;

        // NavMeshAgent に目的地を設定して移動
        agent.speed = escapeSpeed;
        agent.SetDestination(transform.position + escapeDir * 5f); // 5m先の方向へ逃げる

        // アニメーションを走るモードに
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", true);
            animator.speed = 1.5f;
        }

        // 逃走地点に近づいたらオブジェクトを消す（ゲームから排除）
        float distToEscape = Vector3.Distance(transform.position, escapeTarget.position);
        if (distToEscape < 1f)
        {
            Destroy(gameObject);
            return;
        }

        // 逃走時間が尽きたら状態を切り替え
        if (escapeTimer <= 0f)
        {
            float distToZombie = Vector3.Distance(transform.position, playerZombie.position);

            // まだゾンビが近ければ逃走継続
            if (distToZombie < detectRange)
            {
                escapeTimer = escapeDuration;
            }
            else
            {
                // 逃走終了、通常モードに戻る
                isEscaping = false;
                agent.speed = moveSpeed;

                if (animator != null)
                {
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isWalking", true);
                    animator.speed = 1f;
                }
            }
        }
    }

    /// <summary>
    /// 通常時の挙動（今は停止状態）
    /// </summary>
    void NormalBehavior()
    {
        // タイマー更新
        wanderTimer += Time.deltaTime;

        // 一定時間ごとに新しい目標を設定
        if (wanderTimer >= wanderInterval || agent.remainingDistance < 0.5f)
        {
            currentWanderTarget = GetRandomWanderTarget();
            agent.SetDestination(currentWanderTarget);
            wanderTimer = 0f;
        }

        // アニメーションを歩くモードに
        if (animator != null)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", false);
            animator.speed = 1f;
        }

        // ゆっくり歩く速度
        agent.speed = moveSpeed;
    }

    /// <summary>
    /// NavMesh内のランダムな徘徊地点を取得
    /// </summary>
    Vector3 GetRandomWanderTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, wanderRadius, NavMesh.AllAreas))
        {
            return navHit.position;
        }

        // NavMesh内が見つからなかった場合は現在地
        return transform.position;
    }

}

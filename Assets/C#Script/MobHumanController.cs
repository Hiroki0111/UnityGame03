using UnityEngine;

public class MobHumanController : MonoBehaviour
{
    [Header("�ړ����x")]
    public float moveSpeed = 2f;           // �ʏ��������
    public float escapeSpeed = 3.5f;       // �����鑬��

    [Header("�����鎞��")]
    public float escapeDuration = 5f;      // �]���r���m���ɓ�����b��

    [Header("���m�͈�")]
    public float detectRange = 5f;         // �]���r�����m���鋗��

    [Header("�A�j���[�^�[")]
    public Animator animator;              // Animator���Z�b�g�iInspector����j

    private Vector3 targetDir;             // �ʏ펞�̈ړ�����
    private float moveTimer;               // �ʏ�s���̕����ύX�^�C�}�[

    private bool isEscaping = false;       // �����Ă��邩
    private float escapeTimer = 0f;         // ������c�莞��

    private Transform playerZombie;         // �]���r��Transform

    void Start()
    {
        // �^�O�uPlayer�v�̃]���r��T���iStart��1�񂾂��j
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerZombie = playerObj.transform;

        // �ŏ��̈ړ����������߂Ă���
        SetRandomDirection();
    }

    void Update()
    {
        if (animator != null)
        {
            Debug.Log($"isWalking: {animator.GetBool("isWalking")}, isRunning: {animator.GetBool("isRunning")}");
        }
        if (playerZombie == null) return;  // �]���r�����Ȃ���Ή������Ȃ�

        float dist = Vector3.Distance(transform.position, playerZombie.position);

        if (dist < detectRange)
        {
            // �]���r�����m�����瓦����
            if (!isEscaping)
            {
                isEscaping = true;
                escapeTimer = escapeDuration;

                // �U������i�]���r�̋t�����j
                Vector3 dir = (transform.position - playerZombie.position).normalized;
                if (dir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(dir);
            }
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

    void EscapeBehavior()
    {
        escapeTimer -= Time.deltaTime;

        // �]���r���瓦�������
        Vector3 escapeDir = (transform.position - playerZombie.position).normalized;
        transform.Translate(escapeDir * escapeSpeed * Time.deltaTime, Space.World);

        // ����A�j���[�V�����i��j
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", true);
            animator.speed = 1.5f;  // �A�j���[�V�������x�����\
        }

        if (escapeTimer <= 0f)
        {
            // �����鎞�ԏI��
            isEscaping = false;

            // �]���r���܂����邩�`�F�b�N�i�K�v�Ȃ�j
            float dist = Vector3.Distance(transform.position, playerZombie.position);
            if (dist < detectRange)
            {
                // ������x�����邩�A�����͎��R�ɒ������Ă�������
                isEscaping = true;
                escapeTimer = escapeDuration;
            }
            else
            {
                // �ʏ�s���֖߂�
                SetRandomDirection();

                if (animator != null)
                {
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isWalking", true);
                    animator.speed = 1f;
                }
            }
        }
    }

    void NormalBehavior()
    {
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0f)
        {
            SetRandomDirection();
        }

        if (targetDir != Vector3.zero)
        {
            transform.Translate(targetDir * moveSpeed * Time.deltaTime, Space.World);
            transform.rotation = Quaternion.LookRotation(targetDir);
        }

        if (animator != null)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", false);
            animator.speed = 1f;
        }
    }

    void SetRandomDirection()
    {
        targetDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        moveTimer = Random.Range(1f, 3f);
    }
}

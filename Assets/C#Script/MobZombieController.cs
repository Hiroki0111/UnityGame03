using UnityEngine;
using UnityEngine.AI;

public class MobZombieController : MonoBehaviour
{
    [Header("�ǐՐݒ�")]
    public float speed = 1.2f;
    public float infectionDistance = 1.0f;

    [Header("�{���ݒ�")]
    public float searchInterval = 1.0f;

    [Header("�A�j���[�^�[")]
    public Animator animator;

    private GameObject targetHuman;
    private NavMeshAgent agent;
    private float searchTimer = 0f;
    private bool isAttacking = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent ���K�v�ł��I");
            return;
        }

        agent.speed = speed;
        agent.stoppingDistance = infectionDistance;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        if (animator != null)
        {
            Debug.Log("Attack�g���K�[����");
            animator.SetTrigger("Attack");
        }
        else
        {
            Debug.LogError("Animator��null");
        }
    }

    void Update()
    {
        if (animator == null)
            Debug.LogError("Animator ���擾�ł��Ă��܂���I");

        if (agent == null || isAttacking) return;

        searchTimer += Time.deltaTime;

        // �l�Ԃ�T��
        if (targetHuman == null && searchTimer >= searchInterval)
        {
            GameObject[] humans = GameObject.FindGameObjectsWithTag("Human");
            if (humans.Length > 0)
            {
                targetHuman = GetClosestHuman(humans);
            }
            searchTimer = 0f;
        }

        // �ǐՂƍU������
        if (targetHuman != null)
        {
            agent.SetDestination(targetHuman.transform.position);

            float dist = Vector3.Distance(transform.position, targetHuman.transform.position);
            if (dist <= infectionDistance)
            {
                StartCoroutine(AttackAndInfect());
            }
        }

        // �A�j���[�V�����F�������~�܂�
        if (animator != null)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f;
            animator.SetBool("isWalking", isMoving);
        }

    }

    GameObject GetClosestHuman(GameObject[] humans)
    {
        GameObject closest = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject human in humans)
        {
            float dist = Vector3.Distance(currentPos, human.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = human;
            }
        }

        return closest;
    }

    System.Collections.IEnumerator AttackAndInfect()
    {
        isAttacking = true;
        agent.isStopped = true;
        Debug.LogError("attack���\�b�h�͍s���Ă�");
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // �����܂őҋ@�i�U�����[�V�����ɍ��킹�Ē����j
        yield return new WaitForSeconds(0.7f); // �A�j���[�V�����ɍ��킹�Ē���

        if (targetHuman != null)
        {
            InfectionManager.Instance.Infect(targetHuman);
        }

        targetHuman = null;
        agent.isStopped = false;
        isAttacking = false;
    }
}

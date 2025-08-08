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

        Debug.Log("Attack�g���K�[����");
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // �U���A�j���[�V�����ɍ��킹�đҋ@
        yield return new WaitForSeconds(0.7f);

        if (targetHuman != null)
        {
            InfectionManager.Instance.Infect(targetHuman);
            targetHuman = null;
        }

        agent.isStopped = false;
        isAttacking = false;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Human"))
        {
            Debug.Log("�l�ԂɐڐG�I�U���J�n�I");
            StartCoroutine(AttackAndInfect());
        }
    }

}

using UnityEngine;

public class EscapeTargetTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Human"))
        {
            Debug.Log("[EscapeTarget] Human�������؂�܂���: " + other.name);
            Destroy(other.gameObject);
        }
    }
}

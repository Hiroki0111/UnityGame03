using UnityEngine;

public class EscapeTargetTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Human"))
        {
            Debug.Log("[EscapeTarget] Human‚ª“¦‚°Ø‚è‚Ü‚µ‚½: " + other.name);
            Destroy(other.gameObject);
        }
    }
}

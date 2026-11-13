using UnityEngine;

public class BossActivationZone : MonoBehaviour
{
    [SerializeField] private Boss boss;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (boss == null)
        {
            Debug.LogWarning("BossZone: el campo Boss está vacío.", this);
            return;
        }

        if (other.CompareTag("Player"))
        {
            boss.Activate();
            gameObject.SetActive(false); // se usa una sola vez
        }
    }
}
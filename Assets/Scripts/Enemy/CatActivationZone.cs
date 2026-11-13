using UnityEngine;

public class CatActivationZone : MonoBehaviour
{
    [SerializeField] private HellCat cat;

    private void Awake()
    {
        // Si no arrastraste el gato, lo busca entre los hermanos
        if (cat == null && transform.parent != null)
            cat = transform.parent.GetComponentInChildren<HellCat>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (cat == null) return;

        if (other.CompareTag("Player"))
        {
            cat.Activate();
            gameObject.SetActive(false); // la zona se usa una sola vez
        }
    }
}
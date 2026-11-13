using UnityEngine;

public class Spikes : MonoBehaviour
{
    public int damage = 10; // Cantidad de daño que los pinchos infligen al jugador

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Jugador ha entrado en las pinchos");
            collision.GetComponent<Player>().TakeDamage(damage);
            return;
        }

        // Enemigos (fantasma)
        Enemy enemy = collision.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            enemy.Kill();
            return;
        }

        // Enemigos (gato)
        HellCat cat = collision.GetComponentInParent<HellCat>();
        if (cat != null)
        {
            cat.Kill();
        }

        Skeleton skeleton = collision.GetComponentInParent<Skeleton>();
        if (skeleton != null)
        {
            skeleton.Kill();
        }
    }
}
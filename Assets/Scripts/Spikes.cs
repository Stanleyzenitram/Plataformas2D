using UnityEngine;

public class Spikes : MonoBehaviour
{

    public int damage = 10; // Cantidad de daño que los pinchos infligen al jugador
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Aquí puedes agregar la lógica para reiniciar el nivel o restar vidas al jugador
            Debug.Log("Jugador ha entrado en las pinchos");
            collision.GetComponent<Player>()
                 .TakeDamage(damage);
        }
    }
}

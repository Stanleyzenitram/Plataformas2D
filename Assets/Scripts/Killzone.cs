using UnityEngine;
using UnityEngine.SceneManagement;

public class Killzone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Aquí puedes agregar la lógica para reiniciar el nivel o restar vidas al jugador
            Debug.Log("Jugador ha entrado en la zona de muerte");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reinicia el nivel actual
        }
    }
}

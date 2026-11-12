using UnityEngine;
using UnityEngine.UI;

public class BarraVida : MonoBehaviour
{
    public Image rellenoVida;
     private Player player;
     private int maxHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        maxHealth = player.health;
    }

    // Update is called once per frame
    void Update()
    {
        rellenoVida.fillAmount = (float)player.health / maxHealth;
    }
}

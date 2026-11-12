using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinZone : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;

    [SerializeField] private string nextLevelName;

    private bool completed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (completed) return;

        if (other.CompareTag("Player"))
        {
            completed = true;

            StartCoroutine(CompleteLevel(other.gameObject));
        }
    }

    private IEnumerator CompleteLevel(GameObject player)
    {
        Player playerScript = player.GetComponent<Player>();

        if (playerScript != null)
        {
            playerScript.enabled = false;
        }

        messageText.text = "NIVEL 1 COMPLETADO";

        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene(nextLevelName);
    }
}
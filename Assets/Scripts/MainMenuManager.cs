using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject controlsPanel; // panel con los controles (opcional)
    [SerializeField] private GameObject closeButton; // botón Close

    void Start()
{
#if UNITY_WEBGL
    closeButton.SetActive(false);
#endif
}
    public void Play()
    {
        SceneManager.LoadScene("Nivel1");
    }

    public void ShowControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(true);
    }

    public void HideControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
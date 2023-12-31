using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMenu : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private AudioClip SelectSound, CancelSound;
    
    
    private void Start()
    {
        playButton.onClick.AddListener(GoToPlayScene);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void GoToPlayScene()
    {
        SoundManager.PlaySound(SelectSound);
        SceneManager.LoadScene(1);
    }
    
    public void OpenPanel(GameObject panel)
    {
        SoundManager.PlaySound(SelectSound);
        panel.SetActive(true);
    }
    public void ClosePanel(GameObject panel)
    {
        SoundManager.PlaySound(CancelSound);
        panel.SetActive(false);
    }

    private void QuitGame()
    {
        SoundManager.PlaySound(CancelSound);
        Application.Quit();
    }

}

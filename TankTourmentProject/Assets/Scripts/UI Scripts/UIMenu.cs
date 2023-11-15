using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMenu : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    
    private void Start()
    {
        playButton.onClick.AddListener(GoToPlayScene);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void GoToPlayScene()
    {
        SceneManager.LoadScene(1);
    }

    private void QuitGame()
    {
        Application.Quit();
    }

}

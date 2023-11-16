using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private TextMeshProUGUI fullScreenText;
    [SerializeField] private Button restartButton;

    public event Action OnGameCDFinished;
    public event Action CanGenerateMap;

    
    // Start is called before the first frame update
    void Start()
    {
        winnerText.gameObject.SetActive(false);
        fullScreenText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CountdownForGameStart()
    {
        fullScreenText.gameObject.SetActive(true);
        StartCoroutine(CountdownForGameStart(3));
        
        return;
        
        IEnumerator CountdownForGameStart(int seconds)
        {
            for (int i = seconds; i > 0; i--)
            {
                fullScreenText.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }
            OnGameCDFinished?.Invoke();
            fullScreenText.gameObject.SetActive(false);
        }
    }

    public void CountdownFirstTo(int points)
    {
        winnerText.gameObject.SetActive(true);
        winnerText.text = $"First to {points} wins!";
        StartCoroutine(Countdown());
        
        return;
        
        IEnumerator Countdown()
        {
            yield return new WaitForSeconds(3);
            CanGenerateMap?.Invoke();
            winnerText.gameObject.SetActive(false);
        }
    }
    
    public void ShowWinner(PlayerController playerController)
    {
        winnerText.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
        
        
        winnerText.text = $"{playerController.Layer} wins!";
        winnerText.color = playerController.TankSelectionData.SelectedColor;
    }
}

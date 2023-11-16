using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private TextMeshProUGUI tipsText;

    public event Action CanGenerateMap;
    public event Action RestartGame;

    
    // Start is called before the first frame update
    void Start()
    {
        winnerText.gameObject.SetActive(false);
        tipsText.gameObject.SetActive(false);
    }

    public void CountdownFirstTo(int points)
    {
        winnerText.gameObject.SetActive(true);
        winnerText.text = $"First to {points} wins!";
        StartCoroutine(Countdown());
        
        return;
        
        IEnumerator Countdown()
        {
            yield return new WaitForSeconds(1.5f);
            winnerText.gameObject.SetActive(false);
            CanGenerateMap?.Invoke();
        }
    }
    
    public void ShowWinner(PlayerController playerController)
    {
        winnerText.gameObject.SetActive(true);
        tipsText.gameObject.SetActive(true);
        
        winnerText.text = $"{LayerMask.LayerToName(playerController.Layer)} wins!";
        winnerText.color = playerController.TankSelectionData.SelectedColor;
        
        playerController.PlayerInput.actions["Accept"].performed += Restart;
        
        return;
        
        void Restart(UnityEngine.InputSystem.InputAction.CallbackContext _)
        {
            RestartGame?.Invoke();
        }
    }
}

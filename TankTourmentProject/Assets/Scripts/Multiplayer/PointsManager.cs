using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float pointsToWin = 3f;
    [SerializeField] private float pointsRate;
    [SerializeField] private float pointsPerKill;
    [Space]
    [SerializeField] private ControlPoint controlPointPrefab;
    [SerializeField] private float pointDuration;
    [SerializeField] private float pointSpawnTime;
    
    private List<PlayerController> playerPoints = new ();
    
    private List<ControlPoint> controlPoints = new ();
    
    public event Action<PlayerController> OnPlayerWin; 

    public void SetPlayers(IEnumerable<PlayerController> players)
    {
        playerPoints.Clear();
        foreach (var player in players)
        {
            playerPoints.Add(player);
            
            player.PointAmount.OnAmountChanged += CheckVictory;

            continue;
            
            void CheckVictory()
            {
                var notWin = player.PointAmount.Amount < pointsToWin;
                
                Debug.Log($"{player.gameObject.name} points : {player.PointAmount.Amount} ({!notWin})");
                
                if(notWin) return;
                
                OnPlayerWin?.Invoke(player);
            }
        }
    }

    public void SetPoints(List<Vector3> points,Vector3 scale)
    {
        foreach (var point in points)
        {
            var controlPoint = Instantiate(controlPointPrefab, point, Quaternion.identity);
            Vector3 newScale = controlPoint.transform.localScale;
            newScale.x *= scale.x;
            newScale.y *= scale.y;
            newScale.z *= scale.z;
            newScale *= 1.5f;
            
            controlPoint.transform.localScale = newScale;
            Debug.Log($"Spawned Point, (scale : {scale})", controlPoint);
            
            controlPoints.Add(controlPoint);
            
            controlPoint.OnTankStay += IncreaseScoreForTank;
        }
    }
    
    private void IncreaseScoreForTank(Tank tank)
    {
        tank.IncreaseScore(pointsRate * Time.deltaTime);
    }
    
    public class PointAmount
    {
        public float Amount { get; private set; }
        public event Action OnAmountChanged;
        public void IncreaseAmount(float amount)
        {
            Amount += amount;
            OnAmountChanged?.Invoke();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PointsManager : MonoBehaviour
{
    [field:Header("Settings")]
    [field:SerializeField] public int PointsToWin { get; private set; } = 3;
    [SerializeField] private float pointsRate = 1/45f;
    [SerializeField] private float pointsToCapture = 1;
    [Space]
    [SerializeField] private ControlPoint controlPointPrefab;
    [SerializeField] private Color controlPointIndicatorColor = Color.gray;
    [SerializeField] private Color controlPointPreviewColor = Color.grey;
    [SerializeField] private Color controlPointContestedColor = Color.black;
    [SerializeField] private float controlPointAlpha = 0.5f;

    public event Action<ControlPoint,ControlPoint> OnControlPointsSet;
    
    private ControlPoint currentPoint;
    private ControlPoint nextPoint;
    
    private List<PlayerController> playerPoints = new ();
    
    private List<ControlPoint> controlPoints = new ();
    
    public event Action<PlayerController> OnPlayerWin; 

    public void SetPlayers(IEnumerable<PlayerController> players)
    {
        playerPoints.Clear();
        
        var playersList = players.ToList();
        
        foreach (var player in playersList)
        {
            playerPoints.Add(player);
            
            player.ControlPointIndicator.SetControlPoints(currentPoint,nextPoint);
            
            player.ControlPointIndicator.SetColors(controlPointIndicatorColor,controlPointPreviewColor);
            
            player.PointAmount.OnPercentChanged += IncreaseCapture;

            player.PointAmount.OnCaptureIncreased += CheckVictory;
            
            player.PointAmount.OnCaptureIncreased += OnPointCaptured;

            OnControlPointsSet += player.ControlPointIndicator.SetControlPoints;

            continue;

            void IncreaseCapture()
            {
                var progress = player.PointAmount.PointPercent / pointsToCapture;
                
                var color = player.TankSelectionData.SelectedColor;
                color.a = controlPointAlpha;
                
                currentPoint.ShowProgress(progress, color);
                
                if (player.PointAmount.PointPercent < pointsToCapture) return;
                
                player.PointAmount.IncreaseCapture();
            }

            void CheckVictory()
            {
                var notWin = player.PointAmount.PointCaptured < PointsToWin;
                
                if(notWin) return;
                
                OnPlayerWin?.Invoke(player);
            }
        }
        
        return;

        void OnPointCaptured()
        {
            NextPoint();
            
            foreach (var player in playersList)
            {
                player.PointAmount.ResetCapturePercent();
            }
        }
    }

    public void SetPoints(List<Vector3> points,Vector3 scale)
    {
        foreach (var point in points)
        {
            var controlPoint = Instantiate(controlPointPrefab, point, Quaternion.identity);
            var newScale = controlPoint.transform.localScale;
            newScale.x *= scale.x;
            newScale.y *= scale.y;
            newScale.z *= scale.z;
            newScale *= 1.5f;
            
            controlPoint.transform.localScale = newScale;
            
            controlPoints.Add(controlPoint);
            
            controlPoint.OnTankStay += IncreaseScoreForTank;
            
            controlPoint.SetValues(pointsToCapture, controlPointPreviewColor, controlPointIndicatorColor,controlPointContestedColor);
        }

        nextPoint = controlPoints[Random.Range(0, controlPoints.Count)];
        NextPoint();
    }
    
    private void IncreaseScoreForTank(Tank tank)
    {
        tank.IncreaseCapturePercent(pointsRate * Time.deltaTime);
    }
    
    private void NextPoint()
    {
        if(currentPoint != null) currentPoint.Deactivate();
        
        currentPoint = nextPoint;
        
        currentPoint.Activate();
        
        var availablePoints = controlPoints.Where(point => point != currentPoint).ToList();
        
        nextPoint = availablePoints[Random.Range(0, availablePoints.Count)];
        
        nextPoint.ShowPreview();
        
        OnControlPointsSet?.Invoke(currentPoint,nextPoint);
    }
    
    public class PointAmount
    {
        public int PointCaptured { get; private set; }
        public float PointPercent { get; private set; }
        public event Action OnPercentChanged;
        public event Action OnCaptureIncreased;

        public void ResetCapturePercent()
        {
            PointPercent = 0;
            OnPercentChanged?.Invoke();
        }
        public void IncreaseCapturePercent(float amount)
        {
            PointPercent += amount;
            OnPercentChanged?.Invoke();
        }

        public void IncreaseCapture()
        {
            PointCaptured++;
            OnCaptureIncreased?.Invoke();
        }
    }
}

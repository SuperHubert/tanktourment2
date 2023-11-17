using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankFieldOfView : MonoBehaviour
{
    [Header("Components")] 
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private Camera planeCamera;
    [SerializeField] private Transform sphereMask;

    [Header("Settings")]
    [SerializeField] private float closeAreaMultiplier = 0.09f;
    [SerializeField] private float projectionRatio = 0.5f;
    [SerializeField] private int meshResolution = 10;
    [SerializeField] private int edgeResolveIterations = 4;
    [SerializeField] private float edgeDistanceThreshold = 4;
    private float ViewDistance => connectedTank.MaxVisibilityRange;
    private float ViewAngle => connectedTank.MaxVisibilityAngle;
    [SerializeField] private LayerMask obstacleMask;
    
    private Mesh mesh;
    private Tank connectedTank;
    private bool connected = false;

    public void SetTank(Tank tank,Camera cam)
    {
        connectedTank = tank;
        connected = connectedTank != null;
        planeCamera = cam;
    }
    
    private void Start()
    {
        mesh = new Mesh
        {
            name = "View Mesh"
        };
        
        meshFilter.mesh = mesh;
    }
    
    private void LateUpdate()
    {
        DrawFov();
    }

    private void DrawFov()
    {
        if(!connected) return;
        
        var rayCount = Mathf.RoundToInt(ViewAngle * meshResolution);
        var angleStep = ViewAngle / rayCount;
        
        var viewPoints = new List<Vector3>();

        var previousViewCastInfo = new ViewCastInfo();
        for (var i = 0; i <= rayCount; i++)
        {
            var currentAngle = transform.eulerAngles.y - ViewAngle / 2 + angleStep * i;
            
            var viewCastInfo = ViewCast(currentAngle);

            if (i > 0)
            {
                var edgeDistanceThresholdExceeded = Mathf.Abs(previousViewCastInfo.distance - viewCastInfo.distance) > edgeDistanceThreshold;
                if (previousViewCastInfo.hit != viewCastInfo.hit || (previousViewCastInfo.hit && edgeDistanceThresholdExceeded))
                {
                    var edge = FindEdge(previousViewCastInfo, viewCastInfo);
                    if(edge.pointA != Vector3.zero) viewPoints.Add(edge.pointA);
                    if(edge.pointB != Vector3.zero) viewPoints.Add(edge.pointB);
                }
            }
            
            viewPoints.Add(viewCastInfo.point);
            previousViewCastInfo = viewCastInfo;
        }
        
        var vertexCount = viewPoints.Count + 1;
        var vertices = new Vector3[vertexCount];
        var triangles = new int[(vertexCount - 2) * 3];
        
        vertices[0] = Vector3.zero;
        for (var i = 0; i < vertexCount-1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);
            
            if (i >= vertexCount - 2) continue;
            
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        var camPos = transform.InverseTransformPoint(planeCamera.transform.position);
        for (int i = 0; i < vertexCount; i++)
        {
            var vertice = vertices[i];
            
            vertice = Vector3.Lerp(vertice,camPos, projectionRatio);
            
            vertices[i] = vertice;
        }

        sphereMask.localPosition = vertices[0];
        sphereMask.localScale = Vector3.one * (closeAreaMultiplier * connectedTank.Stats.CloseAreaSize);
        
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        var minAngle = minViewCast.angle;
        var maxAngle = maxViewCast.angle;
        
        var minPoint = Vector3.zero;
        var maxPoint = Vector3.zero;
        
        for (var i = 0; i < edgeResolveIterations; i++) {
            var angle = (minAngle + maxAngle) / 2;
            var newViewCast = ViewCast (angle);

            var edgeDstThresholdExceeded = Mathf.Abs (minViewCast.distance - newViewCast.distance) > edgeDistanceThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded) {
                minAngle = angle;
                minPoint = newViewCast.point;
            } else {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo (minPoint, maxPoint);
    }
    
    private ViewCastInfo ViewCast(float globalAngle)
    {
        var dir = DirFromAngle(globalAngle, true);
        var didHit = Physics.Raycast(transform.position, dir, out var hit, ViewDistance, obstacleMask);
        return didHit ?
            new ViewCastInfo(true,hit.point,hit.distance,globalAngle) : 
            new ViewCastInfo(false,transform.position + dir * ViewDistance,ViewDistance,globalAngle);
    }

    private struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;
        
        public ViewCastInfo(bool hit, Vector3 point, float distance, float angle)
        {
            this.hit = hit;
            this.point = point;
            this.distance = distance;
            this.angle = angle;
        }
    }
    
    private struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;
        
        public EdgeInfo(Vector3 pointA, Vector3 pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
        }
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),0,Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}

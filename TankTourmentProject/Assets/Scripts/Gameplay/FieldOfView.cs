using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [Header("Components")] 
    [SerializeField] private MeshFilter meshFilter;
    
    [Header("Settings")]
    [SerializeField,Range(0,360)] private float viewAngle;
    [SerializeField] private float viewDistance = 50f;
    [SerializeField] private int meshResolution = 10;
    [SerializeField] private int edgeResolveIterations = 4;
    [SerializeField] private float edgeDistanceThreshold = 4;
    [SerializeField] private LayerMask obstacleMask;
    
    private Mesh mesh;
    
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
        var rayCount = Mathf.RoundToInt(viewAngle * meshResolution);
        var angleStep = viewAngle / rayCount;
        
        var viewPoints = new List<Vector3>();

        var previousViewCastInfo = new ViewCastInfo();
        for (var i = 0; i <= rayCount; i++)
        {
            var currentAngle = transform.eulerAngles.y - viewAngle / 2 + angleStep * i;
            
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
        var didHit = Physics.Raycast(transform.position, dir, out var hit, viewDistance, obstacleMask);
        return didHit ?
            new ViewCastInfo(true,hit.point,hit.distance,globalAngle) : 
            new ViewCastInfo(false,transform.position + dir * viewDistance,viewDistance,globalAngle);
    }

    public struct ViewCastInfo
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
    
    public struct EdgeInfo
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

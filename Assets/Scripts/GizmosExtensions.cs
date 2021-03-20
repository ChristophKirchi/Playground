using UnityEngine;

public static class GizmosExtensions
{
    public static void DrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.DrawRay(pos, direction);
       
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }
 
    public static void DrawArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.color = color;
        Gizmos.DrawRay(pos, direction);
       
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }

    public static void DrawHalfEdgeGizmos(HalfEdgeMesh heMesh)
    {
        if (heMesh != null)
        {
            foreach (var he in heMesh.halfEdges)
            {
                var originPos = he.origin.data.pos;
                var toPos = he.next.origin.data.pos;
                
                DrawArrow(originPos, toPos - originPos, .05f, 10f);
            }

            foreach (var vertex in heMesh.vertices)
            {
                Gizmos.color = vertex.data.color;
                Gizmos.DrawSphere(vertex.data.pos, .02f);
                Gizmos.color = Color.white;
            }

            foreach (var face in heMesh.faces)
            {
                Vector3 sum = new Vector3();
                Color colorSum = new Color();
                foreach (var he in face.GetAdjacentHalfEdges())
                {
                    sum += he.origin.data.pos;
                    colorSum += he.origin.data.color;
                }
                sum /= 3;
                colorSum /= 3;
                Gizmos.color = colorSum;
                Gizmos.DrawSphere(sum, .02f);
                Gizmos.color = Color.white;
            }
        }
    }
}
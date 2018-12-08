//获取屏幕点击位置并且转化到对应物体位置
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class screenToObjects : MonoBehaviour {

    public GameObject[] clickRecv;
    //[HideInInspector]
    public Vector2 screenPos = Vector2.zero;
    public bool needToCompute = true;

	void Update () {
        if (Input.GetMouseButton(0))
        {
            //1.纯数学方法，不需要添加任何其他组件辅助
            //Vector3 wordPoint = CountObjectPoint(Input.mousePosition);
            //2.添加MeshCollider进行射线Ray撞击来拾取世界坐标点
            Vector3 wordPoint = GetObjectPointUseRay(Input.mousePosition);
            for(int i = 0;i < clickRecv.Length; i++)
            {
                clickRecv[i].GetComponent<Renderer>().material.SetVector("_Point", wordPoint);
            }
            screenPos = Input.mousePosition;
            needToCompute = true;
        }
        else
        {
            needToCompute = false;
        }
    }

    #region 纯属数学计算获取，不需要添加碰撞网格
    Vector3 CountObjectPoint(Vector2 screenPoint)
    {
        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;

        List<Vector3> vertices = new List<Vector3>();
        mesh.GetVertices(vertices);
        Vector3[] worldVertices = new Vector3[vertices.Count];
        Vector3[] screenVertices = new Vector3[vertices.Count];
        
        int[] triangles = mesh.triangles;

        Vector3 worldPoint = Vector3.zero;
        float minDistance = float.MaxValue;
        float currentDistance = 0f;
        Vector3 currentPoint = Vector3.zero;

        for(int i = 0;i < vertices.Count; i++)
        {
            worldVertices[i] = transform.TransformPoint(vertices[i]);
            screenVertices[i] = Camera.main.WorldToScreenPoint(worldVertices[i]);
        }

        for (int i = 2; i < triangles.Length; i += 3)
        {
            if (JudgeAndCountPoint(screenPoint,

                            screenVertices[triangles[i - 2]],
                            screenVertices[triangles[i - 1]],
                            screenVertices[triangles[i]],

                            worldVertices[triangles[i - 2]],
                            worldVertices[triangles[i - 1]],
                            worldVertices[triangles[i]],
                            ref currentPoint))
            {
                currentDistance = Vector3.Distance(Camera.main.gameObject.transform.position, currentPoint);
                if(currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    worldPoint = currentPoint;
                }
            }
        }
        return worldPoint;
    }

    bool JudgeAndCountPoint(Vector2 point,
        Vector2 tri1, Vector2 tri2, Vector2 tri3,
        Vector3 wor1, Vector3 wor2, Vector3 wor3,
        ref Vector3 worldPoint)
    {
        worldPoint = Vector3.zero;
        float triArea = CountArea(tri1, tri2, tri3);

        float pointArea1 = CountArea(point, tri2, tri3);
        float pointArea2 = CountArea(tri1, point, tri3);
        float pointArea3 = CountArea(tri1, tri2, point);
        float pointArea = pointArea1 + pointArea2 + pointArea3;

        if (triArea < pointArea)
            return false;

        float weight1 = pointArea1 / triArea;
        float weight2 = pointArea2 / triArea;
        float weight3 = pointArea3 / triArea;

        worldPoint = wor1 * weight1 + wor2 * weight2 + wor3 * weight3;
        return true;
    }
    
    //vector cross
    float CountArea(Vector2 v1, Vector2 v2, Vector2 v3)
    {
        Vector2 a = v2 - v1;
        Vector2 b = v3 - v1;
        float area = a.x * b.y - a.y * b.x;
        return Mathf.Abs(area);
    }
    #endregion

    #region 借助物理系统中Ray的能力，获取在MeshCollider击中的具体世界坐标点
    Vector3 GetObjectPointUseRay(Vector2 screenPoint)
    {
        RaycastHit hit = new RaycastHit();
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        if (Physics.Raycast(ray, out hit, 1000f) == false)
            return Vector3.zero;
        if (hit.transform != transform)
            return Vector3.zero;
        return hit.point;
    }
    #endregion

}

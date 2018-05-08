/************************************************ 
 * 文件名:ExportNavMesh.cs 
 * 描述:导出NavMesh数据给服务器使用 
 * 创建人:陈鹏 
 * 创建日期：20160926 
 * http://blog.csdn.net/huutu/article/details/52672505 
 * ************************************************/

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class ExportNavMesh
{
    [MenuItem("NavMesh/Export")]
    static void Export()
    {
        Debug.Log("ExportNavMesh");

        UnityEngine.AI.NavMeshTriangulation tmpNavMeshTriangulation = UnityEngine.AI.NavMesh.CalculateTriangulation();

        //新建文件  
        string tmpPath = Application.dataPath + "/" + SceneManager.GetActiveScene().name + ".lua";
        Debug.Log("=====================" + tmpPath + tmpNavMeshTriangulation.vertices.Length);
        StreamWriter tmpStreamWriter = new StreamWriter(tmpPath);
        tmpStreamWriter.WriteLine("-------本文件是地图导航网格文本");
        tmpStreamWriter.WriteLine("local nav_triangle = {");

        Hashtable pointTable = new Hashtable();

        //顶点  
        for (int i = 0; i < tmpNavMeshTriangulation.vertices.Length; i++)
        {
            //tmpStreamWriter.WriteLine("v  " + tmpNavMeshTriangulation.vertices[i].x + " " + tmpNavMeshTriangulation.vertices[i].y + " " + tmpNavMeshTriangulation.vertices[i].z);
            if (!pointTable.ContainsKey(tmpNavMeshTriangulation.vertices[i]))
                pointTable.Add(tmpNavMeshTriangulation.vertices[i], new ArrayList());

            (pointTable[tmpNavMeshTriangulation.vertices[i]] as ArrayList).Add(i);
        }

        //tmpStreamWriter.WriteLine("g pPlane1");

        Hashtable lineTable = new Hashtable();
        List<NavTriangle> triangles = new List<NavTriangle>();
        
        int triangleIndex = 1;
        
        for (int i = 0; i < tmpNavMeshTriangulation.indices.Length;)
        {
            //tmpStreamWriter.WriteLine("f " + (tmpNavMeshTriangulation.indices[i] + 1) + " " + (tmpNavMeshTriangulation.indices[i + 1] + 1) + " " + (tmpNavMeshTriangulation.indices[i + 2] + 1));
            NavTriangle item = new NavTriangle();
            item.tLineKeyArr = new string[3];
            item.tIndex = triangleIndex;
            SetTriangleInfo(0, tmpNavMeshTriangulation.vertices[tmpNavMeshTriangulation.indices[i + 2]]
                , tmpNavMeshTriangulation.vertices[tmpNavMeshTriangulation.indices[i + 1]]
                , pointTable, item, lineTable);
            SetTriangleInfo(1, tmpNavMeshTriangulation.vertices[tmpNavMeshTriangulation.indices[i + 1]]
                , tmpNavMeshTriangulation.vertices[tmpNavMeshTriangulation.indices[i]]
                , pointTable, item, lineTable);
            SetTriangleInfo(2, tmpNavMeshTriangulation.vertices[tmpNavMeshTriangulation.indices[i + 2]]
                , tmpNavMeshTriangulation.vertices[tmpNavMeshTriangulation.indices[i]]
                , pointTable, item, lineTable);

            triangles.Add(item);
            triangleIndex += 1;
            i = i + 3;
        }

        for (int i = 0; i < triangles.Count; i++)
        {
            tmpStreamWriter.WriteLine(string.Format("\t[{0}] = {{", triangles[i].tIndex));

            tmpStreamWriter.WriteLine(string.Format("\t\tv = {{{{{0}, {1}, {2}}}, {{{3}, {4}, {5}}},{{{6}, {7}, {8}}}}},"
                , tmpNavMeshTriangulation.vertices[tmpNavMeshTriangulation.indices[(triangles[i].tIndex - 1) * 3 + 2]].x, tmpNavMeshTriangulation.vertices[tmpNavMeshTriangulation.indices[(triangles[i].tIndex - 1) * 3 + 2]].z, tmpNavMeshTriangulation.vertices[tmpNavMeshTriangulation.indices[(triangles[i].tIndex - 1) * 3 + 2]].y
                , tmpNavMeshTriangulation.vertices[tmpNavMeshTriangulation.indices[(triangles[i].tIndex - 1) * 3 + 1]].x, tmpNavMeshTriangulation.vertices[tmpNavMeshTriangulation.indices[(triangles[i].tIndex - 1) * 3 + 1]].z, tmpNavMeshTriangulation.vertices[tmpNavMeshTriangulation.indices[(triangles[i].tIndex - 1) * 3 + 1]].y
                , tmpNavMeshTriangulation.vertices[tmpNavMeshTriangulation.indices[(triangles[i].tIndex - 1) * 3]].x, tmpNavMeshTriangulation.vertices[tmpNavMeshTriangulation.indices[(triangles[i].tIndex - 1) * 3]].z, tmpNavMeshTriangulation.vertices[tmpNavMeshTriangulation.indices[(triangles[i].tIndex - 1) * 3]].y));

            tmpStreamWriter.WriteLine(string.Format("\t\tnedge = {{{0}, {1}, {2}}},"
                , GetSameLine(triangles[i].tLineKeyArr[0], triangles[i].tIndex, lineTable)
                , GetSameLine(triangles[i].tLineKeyArr[1], triangles[i].tIndex, lineTable)
                , GetSameLine(triangles[i].tLineKeyArr[2], triangles[i].tIndex, lineTable)));

            tmpStreamWriter.WriteLine("\t},\n");
        }

        tmpStreamWriter.WriteLine("}");

        tmpStreamWriter.Flush();
        tmpStreamWriter.Close();

        Debug.Log("ExportNavMesh Success");
    }

    public static void SetTriangleInfo(int index, Vector3 pos1, Vector3 pos2, Hashtable pointTable, NavTriangle item, Hashtable lineTable)
    {
        int point1, point2;
        string lineKey;
        point1 = Convert.ToInt32((pointTable[pos1] as ArrayList)[0]);
        point2 = Convert.ToInt32((pointTable[pos2] as ArrayList)[0]);

        lineKey = point1 < point2 ? string.Format("{0}_{1}", point1, point2) : string.Format("{0}_{1}", point2, point1);
        item.tLineKeyArr[index] = lineKey;
        if (!lineTable.ContainsKey(lineKey))
            lineTable.Add(lineKey, new ArrayList());

        (lineTable[lineKey] as ArrayList).Add(item.tIndex);
    }

    public static int GetSameLine(string lineKey, int curIndex, Hashtable lineTable)
    {
        ArrayList list = lineTable[lineKey] as ArrayList;
        if (list == null)
            return 0;

        for (int i = 0; i < list.Count; i++)
        {
            if (Convert.ToInt32(list[i]) != curIndex)
                return Convert.ToInt32(list[i]);
        }

        return 0;
    }
}

public class NavTriangle
{
    public int tIndex;

    public string[] tLineKeyArr;
}
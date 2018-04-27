using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public struct OrientedPoint
{
    public Vector3 point;
    public Quaternion angle;
}

[CreateAssetMenu(menuName = "U-Tad/Instancer/New Instancer", fileName = "New Instancer.asset")]
public class Instancer : ScriptableObject
{

    [Header("Instancer Settings")]

    public string name;

    public GameObject baseSurface;
    public List<GameObject> childrens;
    public int instancesNumber;

    public float scaleMin;
    public float scaleMax;

    public Color colorMin;
    public Color colorMax;

    public int materialNumber;

    public void Instantiate()
    {
        GameObject par = Instantiate(baseSurface);

        List<OrientedPoint> points = GetRandomPoints(par, instancesNumber);
        List<float> scale = GetRandomScale(scaleMin, scaleMax, instancesNumber);
        List<Color> colors = GetRandomColor(colorMin, colorMax, materialNumber);

        for (int i = 0; i < points.Count; ++i)
        {
            int childIndex = (int)(Random.value * childrens.Count);
            if (childIndex == childrens.Count)
            {
                childIndex -= 1;
            }

            GameObject child = Instantiate(childrens[childIndex]);
            child.transform.SetParent(par.transform);
            child.transform.localPosition = points[i].point;
            child.transform.localRotation = Quaternion.Euler(child.transform.localRotation.eulerAngles + points[i].angle.eulerAngles);
            child.transform.localScale *= scale[i];

        }
    }

    List<OrientedPoint> GetRandomPoints(GameObject obj, int n)
    {
        List<OrientedPoint> points = new List<OrientedPoint>();

        Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;

        int tricount = mesh.triangles.Length / 3;
        float[] sizes = new float[tricount];

        float[] cumulativeSizes = new float[sizes.Length];
        float total = 0;

        for (int i = 0; i < tricount; ++i)
        {
            Vector3 A = mesh.vertices[mesh.triangles[i * 3]];
            Vector3 B = mesh.vertices[mesh.triangles[i * 3 + 1]];
            Vector3 C = mesh.vertices[mesh.triangles[i * 3 + 2]];

            sizes[i] = 0.5f * Vector3.Cross(A - B, A - C).magnitude;

            total += sizes[i];
            cumulativeSizes[i] = total;
        }

        //n number of instances
        //p number of points
        for (int p = 0; p < n; ++p)
        {
            float randomSample = Random.value * total;

            int triIndex = -1;

            for (int i = 0; i < sizes.Length; ++i)
            {
                if (randomSample <= cumulativeSizes[i])
                {
                    triIndex = i;
                    break;
                }
            }

            if (triIndex == -1)
            {
                Debug.LogError("TriIndex should not be -1");
                return points;
            }

            Vector3 a = mesh.vertices[mesh.triangles[triIndex * 3]];
            Vector3 b = mesh.vertices[mesh.triangles[triIndex * 3 + 1]];
            Vector3 c = mesh.vertices[mesh.triangles[triIndex * 3 + 2]];

            Vector3 an = mesh.normals[mesh.triangles[triIndex * 3]];

            float r = Random.value;
            float s = Random.value;

            if (r + s >= 1)
            {
                r = 1 - r;
                s = 1 - s;
            }

            Vector3 pointOnMesh = a + r * (b - a) + s * (c - a);
            OrientedPoint point = new OrientedPoint();
            point.point = pointOnMesh;
            point.angle = Quaternion.LookRotation(an);

            points.Add(point);
        }

        return points;
    }

    List<float> GetRandomScale(float min, float max, int n)
    {
        List<float> scale = new List<float>();

        for (int i = 0; i < n; ++i)
        {
            scale.Add(min + Random.value * (max - min));
        }

        return scale;
    }
    List<Color> GetRandomColor(Color min, Color max, int n)
    {
        List<Color> colors = new List<Color>();

        for (int i = 0; i < n; ++i)
        {
            Color col = min + Random.value * (max - min);
            colors.Add(col);
        }

        return colors;
    }

}

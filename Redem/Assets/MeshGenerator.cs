using Assets;
using Assets.Functions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    public const int xVertice = 100;
    public const int yVertice = 100;
    public const int maxHeight = 10;
    void Start()
    {
        //ShapeMesh("x^2 + y^2");
    }

    public void ShapeMesh(string formula)
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        int k = 0;
        // Define vertices (positions in 3D space)
        Function function = FunctionParser.parse(formula);

        Vector3[] vertices = new Vector3[xVertice * yVertice * 2];
        for (int l = 0; l < 2; l++)
        {
            for (int i = 0; i < xVertice; i++)
            {
                for (int j = 0; j < yVertice; j++)
                {
                    float z = (float)function.calculate(i / 10f - 5, j / 10f - 5);
                    if(z < 0)
                    {
                        z = Mathf.Max(z, -maxHeight);
                    }
                    else
                    {
                        z = Mathf.Min(z, maxHeight);
                    }
                    vertices[k++] = new Vector3(i / 10f + 0f, z + 0f, j / 10f + 0f); //modify here to transform mesh generator position
                }
            }
        }

        mesh.vertices = vertices;

        // Define triangle indices (clockwise order)

        int[] triangles = new int[24 * (xVertice - 1) * (yVertice - 1)];
        Debug.Log(vertices[23]);
        Debug.Log(vertices[23 + xVertice * yVertice]);
        k = 0;
        for (int i = 0; i < xVertice - 1; i++)
        {
            for (int j = 0; j < yVertice - 1; j++)
            {
                int topLeft = i + j * xVertice;
                int topRight = (i + 1) + j * xVertice;
                int bottomLeft = i + (j + 1) * xVertice;
                int bottomRight = (i + 1) + (j + 1) * xVertice;
                int toReverse = xVertice * yVertice;


                if (Mathf.Abs(vertices[topLeft].y) < maxHeight - 0.001 || Mathf.Abs(vertices[bottomLeft].y) < maxHeight - 0.001 || Mathf.Abs(topLeft)  < maxHeight - 0.001)
                {
                    triangles[k++] = topLeft;
                    triangles[k++] = bottomLeft;
                    triangles[k++] = topRight;

                    triangles[k++] = topLeft + toReverse;
                    triangles[k++] = topRight + toReverse;
                    triangles[k++] = bottomLeft + toReverse;
                }
                if (Mathf.Abs(vertices[bottomRight].y) < maxHeight - 0.001 || Mathf.Abs(vertices[bottomLeft].y) < maxHeight - 0.001 || Mathf.Abs(topLeft) < maxHeight - 0.001)
                {
                    triangles[k++] = topRight;
                    triangles[k++] = bottomLeft;
                    triangles[k++] = bottomRight;

                    triangles[k++] = topRight + toReverse;
                    triangles[k++] = bottomRight + toReverse;
                    triangles[k++] = bottomLeft + toReverse;
                }
            }
        }

        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if(TryGetComponent<MeshCollider>(out MeshCollider existingMc))
        {
            Destroy(existingMc);
        }
        MeshCollider mc = gameObject.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh; // assign generated mesh
        mc.convex = false; // keep false for static concave mesh; true if moving
    }
}

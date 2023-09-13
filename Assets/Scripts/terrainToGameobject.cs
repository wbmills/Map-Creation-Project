using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class terrainToGameobject : MonoBehaviour
{
    public Terrain t;
    public GameObject empty;
    public MeshFilter meshFilter;

    void Start()
    {
        // get mesh filter of empty object with no mesh
        meshFilter = empty.GetComponent<MeshFilter>();

        // set mesh filter values to scripted mesh values
/*        Mesh newMesh = new Mesh();
        Vector3[] verticies = drawVerticies(100, 1000);
        newMesh.vertices = verticies;
        newMesh.triangles = drawTriangles(verticies);
        meshFilter.mesh = newMesh;*/


        // stuff for trialling
        meshFilter.mesh = meshGenerationtrial();
    }

    private Mesh meshGenerationtrial()
    {
        Mesh newMesh = new Mesh();

        Vector3[] verticies = new Vector3[62500];

        int index = 0;
        for (int col = 0; col < Mathf.Sqrt(verticies.Length); col++)
        {
            for (int row = 0; row < Mathf.Sqrt(verticies.Length); row++)
            {
                verticies[index] = new Vector3(row, t.SampleHeight(new Vector3(row, 0, col)), col);
                index++;
            }
        }

        int colLength = (int)Mathf.Sqrt(verticies.Length);
        // total number of triangles needed (colLength-1 ^2 * 2) * number of verticies needed per triangle (*3)
        // (colLength^2 * 2) * 3
        int[] tris = new int[((colLength - 1) * (colLength - 1) * 2 * 3) + ((colLength - 2) * 2 * 3)];

        int i = 0;
        for (int triIndex = 0; triIndex < tris.Length; triIndex += 6)
        {
            tris[triIndex] = i;
            tris[triIndex + 1] = i + colLength;
            tris[triIndex + 2] = i + 1;

            tris[triIndex + 3] = i + colLength;
            tris[triIndex + 4] = i + colLength + 1;
            tris[triIndex + 5] = i + 1;
            i++;
        }
        newMesh.vertices = verticies;
        newMesh.triangles = tris;
        foreach(int x in tris)
        {
            print($"{x} {verticies[x]}");
        }
        return newMesh;
    }

    private Mesh tutorialMeshGeneration()
    {
        Mesh terrainMesh = new Mesh();
        Vector3[] vertices = new Vector3[4];

        int width = 10;
        int height = 20;

        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(width, 0, 0);
        vertices[2] = new Vector3(0, height, 0);
        vertices[3] = new Vector3(width, height, 0);

        int[] triangles = new int[6] {0, 2, 1, 2, 3, 1 };

        terrainMesh.vertices = vertices;
        terrainMesh.triangles = triangles;

        return terrainMesh;
    }

    private int getMultipleOfThree(float num)
    {
        num = Mathf.RoundToInt(num);
        int attempts = 0;
        while(num % 3 != 0 && attempts < 20)
        {
            num++;
            attempts++;
        }
        return (int)num;
    }

    private Vector3[] drawVerticies(int totalVerticies, int meshArea)
    {
        int sqrtVerticies = Mathf.RoundToInt(Mathf.Sqrt(totalVerticies));
        Vector3[] verticies = new Vector3[sqrtVerticies * sqrtVerticies];
        float distance = Mathf.Sqrt(meshArea / verticies.Length);
        int index = 0;
        for (float col = 0; col < sqrtVerticies; col += 1)
        {
            for (float row = 0; row < sqrtVerticies; row += 1)
            {
                float x = row * distance;
                float z = col * distance;
                //float y = t.SampleHeight(new Vector3(x, 10, z));
                float y = 3;
                verticies[index] = new Vector3(x, y, z);
                index++;
            }
        }
        return verticies;
    }

    private int[] drawTriangles(Vector3[] verticies)
    {
        int[] triangles = new int[verticies.Length];
        int colLength = (int)Mathf.Sqrt(verticies.Length);

        for (int i = 0; i < verticies.Length / 4; i++)
        {
            triangles[i] = i;
            triangles[i + 1] = i + colLength;
            triangles[i + 2] = i + 1;

            triangles[i + 3] = i + colLength;
            triangles[i + 4] = i + colLength + 1;
            triangles[i + 5] = i + 1;
        }
        print(triangles[1]);
        return triangles;
    }

/*    private void OnDrawGizmos()
    {
        try
        {
            foreach (Vector3 point in meshFilter.mesh.vertices)
            {
                Gizmos.DrawSphere(point, .3f);
            }
        }
        catch (System.Exception)
        {

        }
    }*/
}

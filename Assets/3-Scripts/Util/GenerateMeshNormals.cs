using UnityEngine;

public class GenerateMeshNormals : MonoBehaviour
{
    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        if (meshFilter != null && meshFilter.mesh != null)
        {
            meshFilter.mesh.RecalculateNormals();
        }
    }
}

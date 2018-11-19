using UnityEngine;

[System.Serializable]
public struct FovInfo
{
    public float maxAngle;
    public float maxDistance;
    public LayerMask blockingLayerMask;
}

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteInEditMode]
public class FovDisplay : MonoBehaviour
{
    [SerializeField] uint numBaseRays = 3;
    [SerializeField] FovInfo _fov = new FovInfo {maxAngle = 30f, maxDistance = 30f, blockingLayerMask = Physics.DefaultRaycastLayers};
    
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    private new MeshRenderer renderer;
    private float baseAlpha;

    public FovInfo fov
    {
        get { return _fov; }
        set { _fov = value; }
    }

    public float fade
    {
        set
        {
            Color color = renderer.material.color;
            color.a = value * baseAlpha;
            renderer.material.color = color;
        }
    }
   
    void Start()
    {
        renderer = GetComponent<MeshRenderer>();
        baseAlpha = renderer.sharedMaterial.color.a;
        
        OnValidate();
    }

    void OnValidate()
    {
        if (vertices == null || vertices.Length != numBaseRays + 1)
        {
            mesh = new Mesh {name = "FovMesh"};
            GetComponent<MeshFilter>().mesh = mesh;

            vertices = new Vector3[numBaseRays + 1];
            triangles = new int[(numBaseRays - 1) * 3];
        }
    }
    
    void LateUpdate()
    {
        GenerateMesh();
    }

    private void GenerateMesh()
    {
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;
        vertices[0] = Vector3.zero;
        
        Vector3 axis = transform.up;
        var rotation  = Quaternion.AngleAxis(-fov.maxAngle * 0.5f, axis);
        var increment = Quaternion.AngleAxis(fov.maxAngle / (numBaseRays - 1), axis);
        for (int i = 0; i < numBaseRays; ++i, rotation *= increment)
        {
            var ray = new Ray(origin, rotation * forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, fov.maxDistance, fov.blockingLayerMask))
            {
                vertices[i + 1] = transform.InverseTransformPoint(hit.point);
            }
            else
            {
                vertices[i + 1] = transform.InverseTransformPoint(ray.GetPoint(fov.maxDistance));
            }
            
            Debug.DrawRay(origin, transform.rotation * vertices[i], Color.black);
        }

        for (int i = 0; i < numBaseRays - 1; ++i)
        {
            int triangleIndex = i * 3;
            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = i + 1;
            triangles[triangleIndex + 2] = i + 2;
        }
        
        mesh.MarkDynamic();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }
}

using UnityEngine;
using UnityEngine.Rendering;

public class ProceduralGrass : MonoBehaviour
{
    [Header("Rendering Properties")]

    [Tooltip("Compute shader for generating transformation matrices.")]
    public ComputeShader computeShader;

    private Mesh terrainMesh;
    [Tooltip("Mesh for individual grass blades.")]
    public Mesh grassMesh;
    [Tooltip("Material for rendering each grass blade.")]
    public Material material;

    [Space(10)]

    [Header("Lighting and Shadows")]

    [Tooltip("Should the procedural grass cast shadows?")]
    public ShadowCastingMode castShadows = ShadowCastingMode.On;
    [Tooltip("Should the procedural grass receive shadows from other objects?")]
    public bool receiveShadows = true;

    [Space(10)]

    [Header("Grass Blade Properties")]

    [Range(0.0f, 1.0f)]
    [Tooltip("Base size of grass blades in all three axes.")]
    public float scale = 0.1f;
    [Range(0.0f, 5.0f)]
    [Tooltip("Minimum height multiplier.")]
    public float minBladeHeight = 0.5f;
    [Range(0.0f, 5.0f)]
    [Tooltip("Maximum height multiplier.")]
    public float maxBladeHeight = 1.5f;

    [Range(-1.0f, 1.0f)]
    [Tooltip("Minimum random offset in the x- and z-directions.")]
    public float minOffset = -0.1f;
    [Range(-1.0f, 1.0f)]
    [Tooltip("Maximum random offset in the x- and z-directions.")]
    public float maxOffset = 0.1f;

    private GraphicsBuffer terrainTriangleBuffer;
    private GraphicsBuffer terrainVertexBuffer;

    private GraphicsBuffer transformMatrixBuffer;

    private GraphicsBuffer grassTriangleBuffer;
    private GraphicsBuffer grassVertexBuffer;
    private GraphicsBuffer grassUVBuffer;

    private Bounds bounds;
    private MaterialPropertyBlock properties;
    
    private int kernel;
    private uint threadGroupSize;
    private int terrainTriangleCount = 0;

    private void Start()
    {
        kernel = computeShader.FindKernel("CalculateBladePositions");

        terrainMesh = GetComponent<MeshFilter>().sharedMesh;

        // Terrain data for the compute shader.
        Vector3[] terrainVertices = terrainMesh.vertices;
        terrainVertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, terrainVertices.Length, sizeof(float) * 3);
        terrainVertexBuffer.SetData(terrainVertices);

        int[] terrainTriangles = terrainMesh.triangles;
        terrainTriangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, terrainTriangles.Length, sizeof(int));
        terrainTriangleBuffer.SetData(terrainTriangles);

        terrainTriangleCount = terrainTriangles.Length / 3;

        computeShader.SetBuffer(kernel, "_TerrainPositions", terrainVertexBuffer);
        computeShader.SetBuffer(kernel, "_TerrainTriangles", terrainTriangleBuffer);

        // Grass data for RenderPrimitives.
        Vector3[] grassVertices = grassMesh.vertices;
        grassVertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, grassVertices.Length, sizeof(float) * 3);
        grassVertexBuffer.SetData(grassVertices);

        int[] grassTriangles = grassMesh.triangles;
        grassTriangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, grassTriangles.Length, sizeof(int));
        grassTriangleBuffer.SetData(grassTriangles);

        Vector2[] grassUVs = grassMesh.uv;
        grassUVBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, grassUVs.Length, sizeof(float) * 2);
        grassUVBuffer.SetData(grassUVs);

        // Set up buffer for the grass blade transformation matrices.
        transformMatrixBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, terrainTriangleCount, sizeof(float) * 16);
        computeShader.SetBuffer(kernel, "_TransformMatrices", transformMatrixBuffer);

        // Set bounds.
        bounds = terrainMesh.bounds;
        bounds.center += transform.position;
        bounds.Expand(maxBladeHeight);

        // Bind buffers to a MaterialPropertyBlock which will get used for the draw call.
        properties = new MaterialPropertyBlock();
        properties.SetBuffer("_TransformMatrices", transformMatrixBuffer);
        properties.SetBuffer("_Positions", grassVertexBuffer);
        properties.SetBuffer("_UVs", grassUVBuffer);

        RunComputeShader();
    }

    private void RunComputeShader()
    {
        // Bind variables to the compute shader.
        computeShader.SetMatrix("_TerrainObjectToWorld", transform.localToWorldMatrix);
        computeShader.SetInt("_TerrainTriangleCount", terrainTriangleCount);
        computeShader.SetFloat("_MinBladeHeight", minBladeHeight);
        computeShader.SetFloat("_MaxBladeHeight", maxBladeHeight);
        computeShader.SetFloat("_MinOffset", minOffset);
        computeShader.SetFloat("_MaxOffset", maxOffset);
        computeShader.SetFloat("_Scale", scale);

        // Run the compute shader's kernel function.
        computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSize, out _, out _);
        int threadGroups = Mathf.CeilToInt(terrainTriangleCount / threadGroupSize);
        computeShader.Dispatch(kernel, threadGroups, 1, 1);
    }

    // Run a single draw call to render all the grass blade meshes each frame.
    private void Update()
    {
        /*
        RenderParams rp = new RenderParams(material);
        rp.worldBounds = bounds;
        rp.matProps = new MaterialPropertyBlock();
        rp.matProps.SetBuffer("_TransformMatrices", transformMatrixBuffer);
        rp.matProps.SetBuffer("_Positions", grassVertexBuffer);
        rp.matProps.SetBuffer("_UVs", grassUVBuffer);
        */

        //Graphics.RenderPrimitivesIndexed(rp, MeshTopology.Triangles, grassTriangleBuffer, grassTriangleBuffer.count, instanceCount: terrainTriangleCount);
        Graphics.DrawProcedural(material, bounds, MeshTopology.Triangles, grassTriangleBuffer, grassTriangleBuffer.count, 
            instanceCount: terrainTriangleCount, 
            properties: properties, 
            castShadows: castShadows, 
            receiveShadows: receiveShadows);
    }

    private void OnDestroy()
    {
        terrainTriangleBuffer.Dispose();
        terrainVertexBuffer.Dispose();
        transformMatrixBuffer.Dispose();

        grassTriangleBuffer.Dispose();
        grassVertexBuffer.Dispose();
        grassUVBuffer.Dispose();
    }
}

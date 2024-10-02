using AnyG.Render;
using Unity.Mathematics;
using UnityEngine;
using float4 = Unity.Mathematics.float4;

public class CPUShader
{
    public Attributes[] attributes;
    public Varyings[] varyings;
    public int[] indices;
    public float[] depthBuffer;
    public float4x4 modelMatrix;
    public float4x4 viewMatrix;
    public float4x4 projectionMatrix;
    public float4x4 mvpMatrix;
    public int2 screenParams;
    
    public CPUShader(Mesh mesh, int width, int height)
    {
        PopulateMesh(mesh);
        BindingRenderSurface(width, height);
    }
    
    public class Attributes
    {
        public float3 position;
    }
    
    public class Varyings
    {
        public float4 positionHCS;
    }
    
    public void VertexShader()
    {
        for (int i = 0; i < attributes.Length; i++)
        {
            varyings[i] = VertexShader(attributes[i]);
        }
    }
    
    public void FragmentShader()
    {
        for (int i = 0; i < indices.Length; i += 3)
        {
            float4 posHCS0 = varyings[indices[i]].positionHCS;
            float4 posHCS1 = varyings[indices[i + 1]].positionHCS;
            float4 posHCS2 = varyings[indices[i + 2]].positionHCS;
            RasterizeTriangle(indices[i], indices[i + 1], indices[i + 2], posHCS0, posHCS1, posHCS2);
        }
    }
    
    public void SetMatrix(float4x4 modelMatrix, float4x4 viewMatrix, float4x4 projectionMatrix)
    {
        this.modelMatrix = modelMatrix;
        this.viewMatrix = viewMatrix;
        this.projectionMatrix = projectionMatrix;
        this.mvpMatrix = math.mul(projectionMatrix, math.mul(viewMatrix, modelMatrix));
    }
    
    public void ClearDepthBuffer()
    {
        for (int i = 0; i < depthBuffer.Length; i++)
        {
            depthBuffer[i] = 0;
        }
    }
    
    private bool DepthTest(int x, int y, float z)
    {
        // Check if the pixel is inside the render surface
        if (z < 0 || z > 1)
        {
            return false;
        }
        
        // Check if the pixel is closer to the camera
        // In DirectX, the depth buffer is reversed, so we need to reverse the comparison
        if (z > depthBuffer[y * screenParams.x + x])
        {
            depthBuffer[y * screenParams.x + x] = z;
            return true;
        }
        
        return false;
    }
    
    private void RasterizeTriangle(int vid0, int vid1, int vid2, float4 posHCS0, float4 posHCS1, float4 posHCS2)
    {
        // Convert homogeneous coordinates to Cartesian coordinates
        float3 pos0 = posHCS0.xyz / posHCS0.w;
        float3 pos1 = posHCS1.xyz / posHCS1.w;
        float3 pos2 = posHCS2.xyz / posHCS2.w;
            
        // Get the bounding box of the triangle
        float2 min = (math.min(math.min(pos0, pos1), pos2).xy * 0.5f + 0.5f) * screenParams;
        float2 max = (math.max(math.max(pos0, pos1), pos2).xy * 0.5f + 0.5f) * screenParams;

        // Clamp the bounding box to the render surface
        int minX = Mathf.RoundToInt(math.clamp(min.x, 0, screenParams.x));
        int minY = Mathf.RoundToInt(math.clamp(min.y, 0, screenParams.y));
        int maxX = Mathf.RoundToInt(math.clamp(max.x, 0, screenParams.x));
        int maxY = Mathf.RoundToInt(math.clamp(max.y, 0, screenParams.y));
            
        // Rasterize the triangle
        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                float4 positionHCS = new float4(new float2(x, y) + 0.5f, 0, 1);
                float2 posP = positionHCS.xy / screenParams * 2 - 1;
                float3 barycentric = BarycentricCoordinates(pos0.xy, pos1.xy, pos2.xy, posP);
                if (barycentric is { x: >= 0, y: >= 0, z: >= 0 } || barycentric is { x: <= 0, y: <= 0, z: <= 0 })
                {
                    barycentric = math.abs(barycentric);
                    positionHCS.z = pos0.z * barycentric.x + pos1.z * barycentric.y + pos2.z * barycentric.z;
                    if (!DepthTest(x, y, positionHCS.z)) continue;
                    positionHCS.w = math.rcp(barycentric.x / posHCS0.w + barycentric.y / posHCS1.w + barycentric.z / posHCS2.w);
                    FragmentShader(vid0, vid1, vid2, positionHCS, barycentric);
                }
            }
        }
    }
    
    private static float3 BarycentricCoordinates(float2 pos0, float2 pos1, float2 pos2, float2 posP)
    {
        float2 v01 = pos1 - pos0;
        float2 v12 = pos2 - pos1;
        float2 v20 = pos0 - pos2;
            
        float2 v0p = posP - pos0;
        float2 v1p = posP - pos1;
        float2 v2p = posP - pos2;

        float area0 = math.cross(new float3(v12, 0), new float3(v1p, 0)).z;
        float area1 = math.cross(new float3(v20, 0), new float3(v2p, 0)).z;
        float area2 = math.cross(new float3(v01, 0), new float3(v0p, 0)).z;
        float area = math.abs(area2) + math.abs(area0) + math.abs(area1);
            
        return new float3(area0, area1, area2) / area;
    }
    
    private void PopulateMesh(Mesh mesh)
    {
        // check if the indices are triangles
        if (mesh.triangles.Length % 3 != 0)
        {
            throw new System.Exception("The mesh is not composed of triangles");
        }
        
        indices = mesh.triangles;
        attributes = new Attributes[mesh.vertexCount];
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            attributes[i] = new Attributes
            {
                position = mesh.vertices[i]
            };
        }
        
        varyings = new Varyings[mesh.vertexCount];
    }

    private void BindingRenderSurface(int width, int height)
    {
        this.depthBuffer = new float[width * height];
        screenParams = new int2(width, height);
    }
    
    private Varyings VertexShader(Attributes attribute)
    {
        Varyings varying = new Varyings();
        float4 positionHcs = math.mul(mvpMatrix, new float4(attribute.position, 1));
        varying.positionHCS = positionHcs;
        return varying;
    }
    
    private void FragmentShader(int vid0, int vid1, int vid2, float4 positionHCS, float3 barycentric)
    {
        Varyings varying = InterpolateVaryings(varyings[vid0], varyings[vid1], varyings[vid2], positionHCS, barycentric);
        depthBuffer[(int) positionHCS.y * screenParams.x + (int) positionHCS.x] = positionHCS.z;
    }
    
    private Varyings InterpolateVaryings(Varyings varying0, Varyings varying1, Varyings varying2, float4 positionHCS, float3 barycentric)
    {
        Varyings varying = new Varyings();
        varying.positionHCS = positionHCS;

        return varying;
    }
}

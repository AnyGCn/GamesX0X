using AnyG.Render;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class CPURenderMono : MonoBehaviour
{
    public Camera observer;
    public MeshRenderer target;
    public RawImage rawImage;

    private Mesh _targetMesh;
    private Texture2D _depthTexture;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (observer == null || target == null || rawImage == null)
        {
            Debug.LogError("Please assign observer, target, rawImage and customMaterial in the inspector.");
            Destroy(this);
            return;
        }

        _targetMesh = target.GetComponent<MeshFilter>().sharedMesh;
        _depthTexture = new Texture2D((int)rawImage.rectTransform.rect.width,
            (int)rawImage.rectTransform.rect.height, TextureFormat.RFloat, false)
        {
            name = "_CustomDepthRT",
        };
        
        rawImage.texture = _depthTexture;
    }

    [ContextMenu("Capture")]
    void Capture()
    {
        float4x4 modelMatrix = Coordinate3D.GetModelMatrix(target.transform);
        float4x4 viewMatrix = Coordinate3D.GetViewMatrix(observer.transform);
        float4x4 projectionMatrix = Coordinate3D.GetProjectionMatrix(observer);
        CPUShader shader = new CPUShader(_targetMesh, _depthTexture.width, _depthTexture.height);
        shader.SetMatrix(modelMatrix, viewMatrix, projectionMatrix);
        shader.VertexShader();
        shader.FragmentShader();
        _depthTexture.SetPixelData(shader.depthBuffer, 0);
        _depthTexture.Apply();
    }
}

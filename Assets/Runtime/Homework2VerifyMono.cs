using UnityEngine.Experimental.Rendering;

namespace AnyG
{
    using AnyG.Render;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.UI;

    public static class Homework2Constant
    {
        public static int _ModelMatrixCustomShaderId = Shader.PropertyToID("_ModelMatrixCustom");
        public static int _ViewMatrixCustomShaderId = Shader.PropertyToID("_ViewMatrixCustom");
        public static int _ProjectionMatrixCustomShaderId = Shader.PropertyToID("_ProjectionMatrixCustom");
    }
    
    public class Homework2VerifyMono : MonoBehaviour
    {
        public Camera observer;
        public MeshRenderer target;
        public RawImage rawImage;
        public Material customMaterial;

        private Mesh _targetMesh;
        private RenderTexture _customRT;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (observer == null || target == null || rawImage == null || customMaterial == null)
            {
                Debug.LogError("Please assign observer, target, rawImage and customMaterial in the inspector.");
                Destroy(this);
                return;
            }

            _targetMesh = target.GetComponent<MeshFilter>().sharedMesh;
            _customRT = new RenderTexture((int)rawImage.rectTransform.rect.width,
                (int)rawImage.rectTransform.rect.height, GraphicsFormat.None, GraphicsFormat.D32_SFloat)
            {
                name = "_CustomDepthRT",
            };
            
            rawImage.texture = _customRT;
        }

        private int _UselessColorTex = Shader.PropertyToID("_UselessColorTex");
        // Update is called once per frame
        void Update()
        {
            // set properties
            customMaterial.SetMatrix(Homework2Constant._ModelMatrixCustomShaderId, Coordinate3D.GetModelMatrix(target.transform));
            customMaterial.SetMatrix(Homework2Constant._ViewMatrixCustomShaderId, Coordinate3D.GetViewMatrix(observer.transform));
            customMaterial.SetMatrix(Homework2Constant._ProjectionMatrixCustomShaderId, Coordinate3D.GetProjectionMatrix(observer));
            
            CommandBuffer cmd = CommandBufferPool.Get("DrawCube");
            cmd.GetTemporaryRT(_UselessColorTex, _customRT.width, _customRT.height, 0, FilterMode.Point, GraphicsFormat.R8G8B8A8_UNorm,1, false, RenderTextureMemoryless.Color);
            cmd.SetRenderTarget(_UselessColorTex, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare, _customRT, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.ClearRenderTarget(true, true, Color.clear);
            cmd.DrawMesh(_targetMesh, target.transform.localToWorldMatrix, customMaterial);
            cmd.ReleaseTemporaryRT(_UselessColorTex);
            Graphics.ExecuteCommandBuffer(cmd);
            cmd.Release();

            rawImage.texture = _customRT;
        }
        
        void OnDestroy()
        {
            if (_customRT != null)
            {
                _customRT.Release();
                _customRT = null;
            }
        }
    }

}

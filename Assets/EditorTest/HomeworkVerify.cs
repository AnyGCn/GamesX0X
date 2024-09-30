using System.Collections;
using AnyG;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Mathematics;
using AnyG.Render;
using Unity.Collections;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.TestTools.Utils;

public class HomeworkVerify
{
    // 给定一个点P=(2,1),将该点绕原点先逆时针旋转45,再平移(1,2),计算出变换后点的坐标（要求用齐次坐标进行计算）
    [Test]
    public void Homework0_Verify()
    {
        // Use the Assert class to test conditions
        float rad = math.radians(90);
        float2 point = new float2(2, 1);
        float2 translate = new float2(1, 2);

        Assert.That((Vector2)point.Rotate(rad).Translate(translate), 
            Is.EqualTo(new Vector2(0, 4)).Using(Vector2EqualityComparer.Instance));
        
        Assert.That((Vector2)point.Rotate(rad).Translate(translate),
            Is.EqualTo((Vector2)point.RotateDirect(rad).TranslateDirect(translate)).Using(Vector2EqualityComparer.Instance));
        
        for (int i = 0; i < 100; ++i)
        {
            rad = math.radians(UnityEngine.Random.Range(0, 360));
            point = new float2(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100));
            translate = new float2(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100));
            
            Assert.That((Vector2)point.Rotate(rad).Translate(translate),
                Is.EqualTo((Vector2)point.RotateDirect(rad).TranslateDirect(translate)).Using(Vector2EqualityComparer.Instance));
        }
    }

    // 本次作业的任务是填写一个旋转矩阵和一个透视投影矩阵。给定三维下三个点 v0(2.0,0.0,−2.0),v1(0.0,2.0,−2.0),v2(−2.0,0.0,−2.0)
    // 你需要将这三个点的坐标变换为屏幕坐标并在屏幕上绘制出对应的线框三角形
    // 1. 使用 Unity 的矩阵表示
    // 2. 将我们自己计算的矩阵与 Unity 的矩阵进行比较
    // 3. 构建场景对比 Unity 的 Camera 渲染, 与我们自定义的渲染器是否一致
    [Test]
    public void Homework1_Transform_Verify()
    {
        // Create Objects
        GameObject camera = new GameObject("Camera");
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        camera.AddComponent<Camera>();

        for (int i = 0; i < 100; ++i)
        {
            // Set the camera transform
            camera.transform.position = new Vector3(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100));
            camera.transform.eulerAngles = new Vector3(UnityEngine.Random.Range(-180, 180), UnityEngine.Random.Range(-180, 180), UnityEngine.Random.Range(-180, 180));
            camera.GetComponent<Camera>().fieldOfView = UnityEngine.Random.Range(30, 90);
            camera.GetComponent<Camera>().nearClipPlane = UnityEngine.Random.Range(0.1f, 10);
            camera.GetComponent<Camera>().farClipPlane = UnityEngine.Random.Range(100, 1000);
            camera.GetComponent<Camera>().aspect = UnityEngine.Random.Range(0.5f, 2);

            // Verify View Matrix
            {
                Matrix4x4 expected = camera.GetComponent<Camera>().worldToCameraMatrix;
                Matrix4x4 actual = Coordinate3D.GetViewMatrix(camera.transform);
                Assert.That(actual.GetColumn(0), Is.EqualTo(expected.GetColumn(0)).Using(Vector4EqualityComparer.Instance));
                Assert.That(actual.GetColumn(1), Is.EqualTo(expected.GetColumn(1)).Using(Vector4EqualityComparer.Instance));
                Assert.That(actual.GetColumn(2), Is.EqualTo(expected.GetColumn(2)).Using(Vector4EqualityComparer.Instance));
                Assert.That(actual.GetColumn(3), Is.EqualTo(expected.GetColumn(3)).Using(Vector4EqualityComparer.Instance));
            }
            
            // Verify Projection Matrix
            {
                Matrix4x4 expected = GL.GetGPUProjectionMatrix(camera.GetComponent<Camera>().projectionMatrix, false);
                Matrix4x4 actual = Coordinate3D.GetProjectionMatrix(camera.GetComponent<Camera>());
                Assert.That(actual.GetColumn(0), Is.EqualTo(expected.GetColumn(0)).Using(Vector4EqualityComparer.Instance));
                Assert.That(actual.GetColumn(1), Is.EqualTo(expected.GetColumn(1)).Using(Vector4EqualityComparer.Instance));
                Assert.That(actual.GetColumn(2), Is.EqualTo(expected.GetColumn(2)).Using(Vector4EqualityComparer.Instance));
                Assert.That(actual.GetColumn(3), Is.EqualTo(expected.GetColumn(3)).Using(Vector4EqualityComparer.Instance));
            }
            
            // Set the cube transform
            cube.transform.position = new Vector3(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100));
            cube.transform.eulerAngles = new Vector3(UnityEngine.Random.Range(-180, 180), UnityEngine.Random.Range(-180, 180), UnityEngine.Random.Range(-180, 180));
            cube.transform.localScale = new Vector3(UnityEngine.Random.Range(1, 10), UnityEngine.Random.Range(1, 10), UnityEngine.Random.Range(1, 10));
            
            // Verify Model Matrix
            {
                Matrix4x4 expected = cube.transform.localToWorldMatrix;
                Matrix4x4 actual = Coordinate3D.GetModelMatrix(cube.transform);
                Assert.That(actual.GetColumn(0), Is.EqualTo(expected.GetColumn(0)).Using(Vector4EqualityComparer.Instance));
                Assert.That(actual.GetColumn(1), Is.EqualTo(expected.GetColumn(1)).Using(Vector4EqualityComparer.Instance));
                Assert.That(actual.GetColumn(2), Is.EqualTo(expected.GetColumn(2)).Using(Vector4EqualityComparer.Instance));
                Assert.That(actual.GetColumn(3), Is.EqualTo(expected.GetColumn(3)).Using(Vector4EqualityComparer.Instance));
            }
        }
        
        // Destroy Objects
        CoreUtils.Destroy(camera);
        CoreUtils.Destroy(cube);
    }

    // 通过与使用Unity自身提供的MVP矩阵及Shader绘制出的深度图的对比验证我们自身变换的正确性
    [UnityTest]
    public IEnumerator Homework1_DrawCube_Verify()
    {
        yield return null;
        
        Shader customDrawShader = Shader.Find("Example/URPUnlitShaderBasic");
        int uselessColorTex = Shader.PropertyToID("_UselessColorTex");
        int rtWidth = 480;
        int rtHeight = 270;
        
        // Create Objects
        Assert.IsNotNull(customDrawShader);

        GameObject cameraObject = new GameObject("Camera");
        GameObject quadObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Material customDrawMaterial = new Material(customDrawShader);
        RenderTexture expectedRT = new RenderTexture(rtWidth, rtHeight, 16, GraphicsFormat.R32_SFloat);
        RenderTexture actualRT = new RenderTexture(rtWidth, rtHeight, 16, GraphicsFormat.R32_SFloat);
        Material quadMaterial = new Material(quadObject.GetComponent<MeshRenderer>().sharedMaterial);

        Camera camera = cameraObject.AddComponent<Camera>();
        Mesh quadMesh = quadObject.GetComponent<MeshFilter>().sharedMesh;

        quadMaterial.SetFloat("_Cull", 0);
        customDrawMaterial.SetFloat("_Cull", 0);
        customDrawMaterial.SetPass(0);
        
        for (int execute = 0; execute < 10; ++execute)
        {
            // Set the camera transform
            cameraObject.transform.position = new Vector3(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1), -10 + UnityEngine.Random.Range(-1, 1));
            cameraObject.transform.eulerAngles = new Vector3(UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10));
            camera.fieldOfView = UnityEngine.Random.Range(50, 70);
            camera.nearClipPlane = UnityEngine.Random.Range(0.1f, 1);
            camera.farClipPlane = UnityEngine.Random.Range(100, 202);
            
            // Set the cube transform
            quadObject.transform.position = new Vector3(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1));
            quadObject.transform.eulerAngles = new Vector3(UnityEngine.Random.Range(-180, 180), UnityEngine.Random.Range(-180, 180), UnityEngine.Random.Range(-180, 180));
            quadObject.transform.localScale = new Vector3(UnityEngine.Random.Range(1, 10), UnityEngine.Random.Range(1, 10), UnityEngine.Random.Range(1, 10));
            
            // Unity Draw
            CommandBuffer cmd = CommandBufferPool.Get("DrawCube");
            cmd.SetRenderTarget(expectedRT, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.ClearRenderTarget(true, true, Color.clear);
            cmd.SetGlobalMatrix("unity_MatrixVP", GL.GetGPUProjectionMatrix(camera.projectionMatrix, true) * camera.worldToCameraMatrix);
            cmd.DrawMesh(quadMesh, quadObject.transform.localToWorldMatrix, quadMaterial, 0, quadMaterial.FindPass("DepthOnly"));
            
            // Custom Draw
            customDrawMaterial.SetMatrix(Homework2Constant._ModelMatrixCustomShaderId, Coordinate3D.GetModelMatrix(quadObject.transform));
            customDrawMaterial.SetMatrix(Homework2Constant._ViewMatrixCustomShaderId, Coordinate3D.GetViewMatrix(cameraObject.transform));
            customDrawMaterial.SetMatrix(Homework2Constant._ProjectionMatrixCustomShaderId, Coordinate3D.GetProjectionMatrix(cameraObject.GetComponent<Camera>()));
            
            cmd.SetRenderTarget(actualRT, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.ClearRenderTarget(true, true, Color.clear);
            cmd.DrawMesh(quadMesh, quadObject.transform.localToWorldMatrix, customDrawMaterial);
            cmd.ReleaseTemporaryRT(uselessColorTex);
            Graphics.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            Awaitable<AsyncGPUReadbackRequest>.Awaiter expected = AsyncGPUReadback.RequestAsync(expectedRT).GetAwaiter();
            Awaitable<AsyncGPUReadbackRequest>.Awaiter actual = AsyncGPUReadback.RequestAsync(actualRT).GetAwaiter();
            yield return new WaitUntil(() => expected.IsCompleted && actual.IsCompleted);

            AsyncGPUReadbackRequest expectedResult = expected.GetResult();
            AsyncGPUReadbackRequest actualResult = actual.GetResult();
            
            Assert.IsFalse(expectedResult.hasError || actualResult.hasError);
            NativeArray<float> expectedData = expectedResult.GetData<float>();
            NativeArray<float> actualData = actualResult.GetData<float>();
            Assert.IsTrue(expectedData.Length == actualData.Length);

            bool isAllblack = true;
            for (int i = 0; i < expectedData.Length; ++i)
            {
                if (expectedData[i] != 0)
                    isAllblack = false;
                Assert.That(actualData[i], Is.EqualTo(expectedData[i]).Using(FloatEqualityComparer.Instance));
            }
            
            Assert.IsFalse(isAllblack);
        }
        
        // Destroy Objects
        CoreUtils.Destroy(cameraObject);
        CoreUtils.Destroy(quadObject);
        CoreUtils.Destroy(customDrawMaterial);
        CoreUtils.Destroy(quadMaterial);
        CoreUtils.Destroy(actualRT);
        CoreUtils.Destroy(expectedRT);
        
        yield return null;
    }
}

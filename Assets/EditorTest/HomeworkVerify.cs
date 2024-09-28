using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Mathematics;
using AnyG.Graphics;
using Unity.Collections;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.TestTools.Utils;
using Debug = System.Diagnostics.Debug;

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
    public void Homework1_Verify()
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

    // Use Unity Default Shader and Graphics.DrawMeshNow to draw a cube
    [UnityTest]
    public IEnumerator Homework1_DrawCube_Verify()
    {
        // Create Objects
        Assert.IsNotNull(Camera.main);
        GameObject camera = Camera.main.gameObject;
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        camera.AddComponent<Camera>();
        
        // Set the camera transform
        camera.transform.position = new Vector3(0, 0, -10);
        camera.transform.eulerAngles = new Vector3(0, 0, 0);
        camera.GetComponent<Camera>().fieldOfView = 60;
        camera.GetComponent<Camera>().nearClipPlane = 0.3f;
        camera.GetComponent<Camera>().farClipPlane = 1000;
        
        // Set the cube transform
        cube.transform.position = new Vector3(0, 0, 0);
        cube.transform.eulerAngles = new Vector3(0, 0, 0);
        cube.transform.localScale = new Vector3(1, 1, 1);

        // Unity Draw
        RenderTexture expectedRT = new RenderTexture(480, 270, 32, RenderTextureFormat.Depth);
        CommandBuffer cmd = CommandBufferPool.Get("DrawCube");
        cmd.SetRenderTarget(expectedRT);
        cmd.ClearRenderTarget(true, true, Color.clear);
        cmd.DrawMesh(cube.GetComponent<MeshFilter>().sharedMesh, cube.transform.localToWorldMatrix, cube.GetComponent<MeshRenderer>().sharedMaterial);
        
        // Custom Draw
        Material mat = new Material(Shader.Find(""));
        mat.SetMatrix("_ModelMatrixCustom", Coordinate3D.GetModelMatrix(cube.transform));
        mat.SetMatrix("ViewMatrixCustom", Coordinate3D.GetViewMatrix(camera.transform));
        mat.SetMatrix("ProjectionMatrixCustom", Coordinate3D.GetProjectionMatrix(camera.GetComponent<Camera>()));
        mat.SetPass(0);
        
        RenderTexture actualRT = new RenderTexture(480, 270, 32, RenderTextureFormat.Depth);
        cmd.SetRenderTarget(actualRT);
        cmd.ClearRenderTarget(true, true, Color.clear);
        cmd.DrawMesh(cube.GetComponent<MeshFilter>().sharedMesh, cube.transform.localToWorldMatrix, mat);
        Graphics.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);

        Awaitable<AsyncGPUReadbackRequest>.Awaiter expected = AsyncGPUReadback.RequestAsync(expectedRT).GetAwaiter();
        Awaitable<AsyncGPUReadbackRequest>.Awaiter actual = AsyncGPUReadback.RequestAsync(actualRT).GetAwaiter();
        yield return new WaitUntil(() => expected.IsCompleted && actual.IsCompleted);

        AsyncGPUReadbackRequest expectedResult = expected.GetResult();
        AsyncGPUReadbackRequest actualResult = actual.GetResult();
        
        if (expectedResult.hasError || actualResult.hasError)
        {
            Assert.Fail();
        }
        
        NativeArray<byte> expectedData = expectedResult.GetData<byte>();
        NativeArray<byte> actualData = actualResult.GetData<byte>();
        if (expectedData.Length != actualData.Length)
        {
            Assert.Fail();
        }
        
        for (int i = 0; i < expectedData.Length; ++i)
        {
            Assert.That(actualData[i], Is.EqualTo(expectedData[i]).Using(FloatEqualityComparer.Instance));
        }
        
        // Destroy Objects
        CoreUtils.Destroy(cube);
    }
}

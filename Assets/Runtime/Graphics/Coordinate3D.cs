namespace AnyG.Graphics
{
    using Unity.Mathematics;
    using UnityEngine;
    
    public static class Coordinate3D
    {
        /// <summary>
        /// Get Translate Matrix.
        /// </summary>
        public static float4x4 Translate(float3 translation)
        {
            return new float4x4
            (
                1, 0, 0, translation.x,
                0, 1, 0, translation.y,
                0, 0, 1, translation.z,
                0, 0, 0, 1
            );
        }
        
        /// <summary>
        /// Get Rotation Matrix.
        /// </summary>
        public static float4x4 Rotate(float3 rad)
        {
            float3 cos = math.cos(rad);
            float3 sin = math.sin(rad);
            float4x4 rotX = new float4x4
            (
                1, 0, 0, 0,
                0, cos.x, -sin.x, 0,
                0, sin.x, cos.x, 0,
                0, 0, 0, 1
            );
            
            float4x4 rotY = new float4x4
            (
                cos.y, 0, sin.y, 0,
                0, 1, 0, 0,
                -sin.y, 0, cos.y, 0,
                0, 0, 0, 1
            );
            
            float4x4 rotZ = new float4x4
            (
                cos.z, -sin.z, 0, 0,
                sin.z, cos.z, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );
            
            return math.mul(rotY, math.mul(rotX, rotZ));
        }
        
        /// <summary>
        /// Get Scale Matrix.
        /// </summary>
        public static float4x4 Scale(float3 scale)
        {
            return new float4x4
            (
                scale.x, 0, 0, 0,
                0, scale.y, 0, 0,
                0, 0, scale.z, 0,
                0, 0, 0, 1
            );
        }
        
        public static float4x4 TRS(float3 translation, float3 rad, float3 scale)
        {
            return math.mul(Translate(translation), math.mul(Rotate(rad), Scale(scale)));
        }
        
        public static float3 Translate(this float3 point, float3 translation)
        {
            return math.mul(Translate(translation), new float4(point.x, point.y, point.z, 1)).xyz;
        }
        
        public static float3 Rotate(this float3 point, float3 rad)
        {
            return math.mul(Rotate(rad), new float4(point.x, point.y, point.z, 1)).xyz;
        }
        
        public static float3 Scale(this float3 point, float3 scale)
        {
            return math.mul(Scale(scale), new float4(point.x, point.y, point.z, 1)).xyz;
        }
        
        public static float3 TRS(this float3 point, float3 translation, float3 rad, float3 scale)
        {
            return math.mul(TRS(translation, rad, scale), new float4(point.x, point.y, point.z, 1)).xyz;
        }

        public static Matrix4x4 GetModelMatrix(Transform transform)
        {
            return TRS(transform.position, math.radians(transform.eulerAngles), transform.localScale);
        }
        
        public static Matrix4x4 GetViewMatrix(Transform transform)
        {
            Matrix4x4 viewMatrix = GetModelMatrix(transform).inverse;
            // z-axis point to camera, flip z axis
            viewMatrix.SetRow(2, -viewMatrix.GetRow(2));
            return viewMatrix;
        }
        
        public static Matrix4x4 GetProjectionMatrix(Camera camera)
        {
            // 1.0f / height
            float heightScale = 1.0f / math.tan(math.radians(camera.fieldOfView / 2.0f));
            
            // 1.0f / width
            float widthScale = heightScale / camera.aspect;
            
            float near = camera.nearClipPlane;
            float far = camera.farClipPlane;

            if (SystemInfo.usesReversedZBuffer)
            {
                // depth is 1 to 0
                return new Matrix4x4
                (
                    new Vector4(widthScale, 0, 0, 0),
                    new Vector4(0, heightScale, 0, 0),
                    new Vector4(0, 0, near / (far - near), -1),
                    new Vector4(0, 0, near * far / (far - near), 0)
                );
            }
            else
            {
                // depth is -1 to 1
                return new Matrix4x4
                (
                    new Vector4(widthScale, 0, 0, 0),
                    new Vector4(0, heightScale, 0, 0),
                    new Vector4(0, 0, (far + near) / (near - far), -1),
                    new Vector4(0, 0, 2 * near * far / (near - far), 0)
                );
            }
        }
    }
}

namespace AnyG.Graphics
{
    using Unity.Mathematics;
    
    public static class Coordinate2D
    {
        public static float2 Rotate(this float2 point, float rad)
        {
            float cos = math.cos(rad);
            float sin = math.sin(rad);
            
            float3x3 rotationMatrix = new float3x3(
                cos, -sin, 0,
                sin, cos, 0,
                0, 0, 1
            );
            
            return math.mul(rotationMatrix, new float3(point.x, point.y, 1)).xy;
        }
        
        public static float2 Scale(this float2 point, float2 scale)
        {
            float3x3 scaleMatrix = new float3x3(
                scale.x, 0, 0,
                0, scale.y, 0,
                0, 0, 1
            );
            
            return math.mul(scaleMatrix, new float3(point.x, point.y, 1)).xy;
        }
        
        public static float2 Translate(this float2 point, float2 translation)
        {
            float3x3 translationMatrix = new float3x3(
                1, 0, translation.x,
                0, 1, translation.y,
                0, 0, 1
            );
            
            return math.mul(translationMatrix, new float3(point.x, point.y, 1)).xy;
        }
        
        public static float2 RotateDirect(this float2 point, float rad)
        {
            float cos = math.cos(rad);
            float sin = math.sin(rad);
            return new float2(point.x * cos - point.y * sin, point.x * sin + point.y * cos);
        }
        
        public static float2 ScaleDirect(this float2 point, float2 scale)
        {
            return new float2(point.x * scale.x, point.y * scale.y);
        }
        
        public static float2 TranslateDirect(this float2 point, float2 translation)
        {
            return new float2(point.x + translation.x, point.y + translation.y);
        }
    }
}


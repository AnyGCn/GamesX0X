namespace AnyG
{
    using UnityEngine;

    public static class GlobalConstant
    {
        public static int _ModelMatrixCustomShaderId = Shader.PropertyToID("_ModelMatrixCustom");
        public static int _ViewMatrixCustomShaderId = Shader.PropertyToID("_ViewMatrixCustom");
        public static int _ProjectionMatrixCustomShaderId = Shader.PropertyToID("_ProjectionMatrixCustom");
    }
    
}

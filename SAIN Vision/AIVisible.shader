Shader "Custom/VisibilityShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _ObjectTex ("Object Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        sampler2D _ObjectTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            float4 mainColor = tex2D(_MainTex, IN.uv_MainTex);
            float4 objectColor = tex2D(_ObjectTex, IN.uv_MainTex);

            // Compare colors with a tolerance value (e.g., 0.01)
            bool isVisible = all(lessThan(abs(mainColor.rgb - objectColor.rgb), 0.01));

            o.Albedo = isVisible ? float3(1, 1, 1) : float3(0, 0, 0);
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

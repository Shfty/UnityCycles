Shader "Terrain/Project Texture XZ" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
    _Base ("Texture", 2D) = "white" {}
}
SubShader {
    Tags { "RenderType" = "Opaque" }
    CGPROGRAM
    #pragma surface surf Lambert
    struct Input {
        float2 uv_Base;
    };
    float4 _Color;
    sampler2D _Base;
    void surf (Input IN, inout SurfaceOutput o) {
        o.Albedo = tex2D (_Base, IN.uv_Base).rgb * _Color;
    }
    ENDCG
} 
Fallback "Diffuse"
}
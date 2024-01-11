Shader "Terrain/Project Texture XZ (Y Grid)" {
Properties {
	_GridFrequency ("Grid Frequency", Float) = 100
	_GridWidth ("Grid Width", Float) = 0.5
	_GridThreshold ("Grid Threshold", Float) = 0.5
	_Color ("Main Color", Color) = (1,1,1,1)
    _Base ("Texture", 2D) = "white" {}
	_GridColor ("Grid Color", Color) = (1,1,1,1)
    _GridTex ("Grid Texture", 2D) = "black" {}
}
SubShader {
    Tags {
	"RenderType" = "Opaque"
	"PreviewType"="Plane"
	}
    CGPROGRAM
    #pragma surface surf Lambert
    struct Input {
		float3 worldPos;
        float2 uv_Base;
    };
    float _GridFrequency;
    float _GridWidth;
    float _GridThreshold;
    float4 _Color;
    sampler2D _Base;
    float4 _GridColor;
    sampler2D _GridTex;
    void surf (Input IN, inout SurfaceOutput o) {
		half3 baseColor = tex2D (_Base, IN.uv_Base).rgb * _Color;
		half3 gridColor = tex2D (_GridTex, IN.uv_Base).rgb * _GridColor;
		float lerpFactor = clamp( sin( IN.worldPos.y * _GridFrequency ) + ( _GridWidth ), _GridThreshold, 1 );
        o.Albedo = lerp( baseColor, gridColor, lerpFactor );
    }
    ENDCG
} 
Fallback "Diffuse"
}
Shader "Custom/World UV Plane Texture" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,0.5)
    _MainTex ("Texture", 2D) = "surface" { }
    _Scale ("Texture Scale", Float ) = 1
}
SubShader {

Tags { "RenderType"="Opaque" }

CGPROGRAM
#pragma surface surf Lambert

#include "UnityCG.cginc"

struct Input {
    float2  uv_MainTex;
    float3  worldPos;
};

sampler2D _MainTex;
float4 _Color;
float _Scale;

float4 _MainTex_ST;

void surf(Input IN, inout SurfaceOutput o)
{
	float2 uv = TRANSFORM_TEX( IN.worldPos.xz, _MainTex );
	fixed4 c = tex2D( _MainTex, uv * _Scale );
    o.Albedo = c.rgb * _Color;
}

ENDCG

}

Fallback "VertexLit"
} 
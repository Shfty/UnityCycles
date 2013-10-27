Shader "Projector/Alpha Blend" {
	Properties {
		_Color ( "Color", Color ) = (1,1,1,1)
		_ProjTex ("Cookie", 2D) = "gray" { TexGen ObjectLinear }
	}

	Subshader {
		Tags { "RenderType"="Transparent-1" }
		Pass {
			ZWrite Off
			AlphaTest Greater 0
			ColorMask RGBA
			Blend SrcAlpha OneMinusSrcAlpha
			Offset -1, -1
			SetTexture [_ProjTex] {
				constantColor [_Color]
				combine texture * constant
				Matrix [_Projector]
			}
		}
	}
}
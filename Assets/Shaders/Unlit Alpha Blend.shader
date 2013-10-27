Shader "Unlit/AlphaBlend" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_Texture ("Base (RGB) Trans (A)", 2D) = "white" {}
}

// 1 texture stage GPUs
SubShader {
	Tags {"Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Overlay"}
	
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha 
	ColorMask RGBA
		
	Pass {
		SetTexture [_Texture] {
			constantColor [_Color]
			combine texture * constant
		} 
	}
}
}
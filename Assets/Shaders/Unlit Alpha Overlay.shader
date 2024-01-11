Shader "Unlit/AlphaOverlay" {
Properties {
	_MainColor ("Main Color", Color) = (1,1,1,1)
	_OccludedColor ("Occluded Color", Color) = (1,1,1,0.5)
	_MainTexture ("Base (RGB) Trans (A)", 2D) = "white" {}
	_OccludedTexture ("Base (RGB) Trans (A)", 2D) = "white" {}
}

SubShader {
	Tags {
		"Queue"="Overlay"
		"IgnoreProjector"="True"
		"RenderType"="Overlay"
		"PreviewType"="Plane"
	}
	
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha 
	ColorMask RGBA
		
	Pass {
		ZTest LEqual
		SetTexture [_MainTexture] {
			constantColor [_MainColor]
			combine texture * constant
		} 
	}
	Pass {
		ZTest Greater
		SetTexture [_OccludedTexture] {
			constantColor [_OccludedColor]
			combine texture * constant
		} 
	}
}
}
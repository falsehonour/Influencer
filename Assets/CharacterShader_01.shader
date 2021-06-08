// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Shader Forge/CharacterShader_01" 
{
	Properties
	{
		_HitColor("Hit Color", Color) = (0.5,0.5,0.5,1)
		_Color("Color", 2D) = "white" {}
	_CubeMap("CubeMap", Cube) = "_Skybox" {}
	_HiddenColor("Hidden Color", Color) = (1,1,1,1)
	}
		SubShader{

		Tags
	{
		"Queue" = "Geometry+5"
		"RenderType" = "Opaque"
	}
		Pass
	{
		Name "OpaquePass"

		ZWrite Off
		ZTest Off
		Lighting Off

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
		uniform float4 _HiddenColor;

	struct VertexInput {
		float4 vertex : POSITION;
	};

	struct VertexOutput {
		float4 pos : SV_POSITION;
	};

	VertexOutput vert(VertexInput v) {
		VertexOutput o;
		o.pos = UnityObjectToClipPos(v.vertex);
		return o;
	}

	fixed4 frag(VertexOutput i) : COLOR{
		float4 overlayColor = _HiddenColor;
		return overlayColor;
	}
		ENDCG
	}
}
// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "2LayersSurface"
{
	Properties
	{
		_Layer0Texture("Layer0Texture", 2D) = "white" {}
		_Layer0Colour("Layer0Colour", Color) = (0,0,0,0)
		_Layer1Texture("Layer1Texture", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
		#else//ASE Sampling Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
		#endif//ASE Sampling Macros

		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		UNITY_DECLARE_TEX2D_NOSAMPLER(_Layer1Texture);
		uniform float4 _Layer1Texture_ST;
		SamplerState sampler_Layer1Texture;
		uniform float4 _Layer0Colour;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_Layer0Texture);
		uniform float4 _Layer0Texture_ST;
		SamplerState sampler_Layer0Texture;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Layer1Texture = i.uv_texcoord * _Layer1Texture_ST.xy + _Layer1Texture_ST.zw;
			float2 uv_Layer0Texture = i.uv_texcoord * _Layer0Texture_ST.xy + _Layer0Texture_ST.zw;
			o.Albedo = ( SAMPLE_TEXTURE2D( _Layer1Texture, sampler_Layer1Texture, uv_Layer1Texture ) + ( _Layer0Colour * SAMPLE_TEXTURE2D( _Layer0Texture, sampler_Layer0Texture, uv_Layer0Texture ) ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18400
-1280.833;352.5;1279.167;664.8333;1634.285;421.5244;1.953624;True;True
Node;AmplifyShaderEditor.ColorNode;16;-838.1775,95.347;Inherit;False;Property;_Layer0Colour;Layer0Colour;1;0;Create;True;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;14;-876.0521,291.5237;Inherit;True;Property;_Layer0Texture;Layer0Texture;0;0;Create;True;0;0;False;0;False;-1;a1f5bc25389035c4f9db6acc63698b10;a709d7bbbc88d1341a48a1d2794fd019;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-524.6947,191.047;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;5;-878.57,-159.0754;Inherit;True;Property;_Layer1Texture;Layer1Texture;2;0;Create;True;0;0;False;0;False;-1;b7b98eb25ea61d545863d2cba3883cab;b7b98eb25ea61d545863d2cba3883cab;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;2;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;15;-251.8764,34.81067;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;2LayersSurface;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;17;0;16;0
WireConnection;17;1;14;0
WireConnection;15;0;5;0
WireConnection;15;1;17;0
WireConnection;0;0;15;0
ASEEND*/
//CHKSM=0212B3AC2DF524BCD70D52D58147FF2C73C53B47
Shader "Effect/Effect Add" {
	Properties {
		_TintColor ("Main Color", Vector) = (0.5,0.5,0.5,0.5)
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_CutTex ("Cutout (A)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
		_MainRotation ("Main Rotation", Float) = 0
		_CutRotation ("Cut Rotation", Float) = 0
		_UVScrollX ("Main UV X Scroll", Float) = 0
		_UVScrollY ("Main UV Y Scroll", Float) = 0
		_UVCutScrollX ("Cut UV X Scroll", Float) = 0
		_UVCutScrollY ("Cut UV Y Scroll", Float) = 0
		_UVMirrorX ("UV Mirror X", Range(0, 1)) = 0
		_UVMirrorY ("UV Mirror Y", Range(0, 1)) = 0
		_InvFade ("Soft Particles Factor", Range(0.01, 3)) = 1
		_EmissionGain ("Emission Gain", Range(0, 1)) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;
			float4 _MainTex_ST;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Vertex_Stage_Output
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.uv = (input.uv.xy * _MainTex_ST.xy) + _MainTex_ST.zw;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			Texture2D<float4> _MainTex;
			SamplerState sampler_MainTex;

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy);
			}

			ENDHLSL
		}
	}
	Fallback "Transparent/VertexLit"
	//CustomEditor "ShaderMaterialsEditor"
}
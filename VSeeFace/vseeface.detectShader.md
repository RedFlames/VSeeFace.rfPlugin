

Ripped this shader from VSeeFace to see how the MeshRaycaster works, keeping it here for reference only.
I thought maybe it's something like here by emiliana as well:
https://github.com/emilianavt/DokoDemoPainter/blob/master/Plugins/DokoDemoPainter/Shaders/Resources/DokoDemoPainterDetect.shader
but looking at it closer it is nothing like that at all. Interesting!

// - rf

Shader "Custom/Detect" {
	Properties {
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_SurfaceID ("Surface ID", Float) = -4
		_IgnoreTexture ("Ignore texture", Float) = 0
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
			float4 _Color;
			
			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};
			
			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy) * _Color;
			}
			
			ENDHLSL
		}
	}
}
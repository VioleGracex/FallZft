
Shader "BestPartners/Unlit Dissolve"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_MaskClipValue( "Mask Clip Value", Float ) = 0.5
		_Diffuse("Diffuse", 2D) = "white" {}
		_DissolveNoise("Dissolve Noise", 2D) = "white" {}
		_DissolveIntensity("Dissolve Intensity", Range( 0 , 1)) = 0
		_DissolveColor("Dissolve Color", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _DissolveIntensity;
		uniform sampler2D _DissolveNoise;
		uniform float4 _DissolveNoise_ST;
		uniform float4 _DissolveColor;
		uniform sampler2D _Diffuse;
		uniform float4 _Diffuse_ST;
		uniform float _MaskClipValue = 0.5;

		inline fixed4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return fixed4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_DissolveNoise = i.uv_texcoord * _DissolveNoise_ST.xy + _DissolveNoise_ST.zw;
			float temp_output_73_0 = ( (-0.6 + (( 1.0 - _DissolveIntensity ) - 0.0) * (0.6 - -0.6) / (1.0 - 0.0)) + tex2D( _DissolveNoise, uv_DissolveNoise ).r );
			float clampResult113 = clamp( (-4.0 + (temp_output_73_0 - 0.0) * (4.0 - -4.0) / (1.0 - 0.0)) , 0.0 , 1.0 );
			float2 uv_Diffuse = i.uv_texcoord * _Diffuse_ST.xy + _Diffuse_ST.zw;
			o.Emission = ( ( ( 1.0 - clampResult113 ) * _DissolveColor ) + tex2D( _Diffuse, uv_Diffuse ) ).rgb;
			o.Alpha = 1;
			clip( temp_output_73_0 - _MaskClipValue );
		}

		ENDCG
	}
	Fallback "Diffuse"
	
}

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Smoke_Shader01"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		[HDR]_Light_Color("Light_Color", Color) = (0,0,0,0)
		[HDR]_Main_Color("Main_Color", Color) = (0,0,0,0)
		_Panner_Time("Panner_Time", Float) = 2
		_Texture02("Texture02", 2D) = "white" {}
		_Texture01("Texture01", 2D) = "white" {}
		_DepthFade("Depth Fade", Float) = 3

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "Lighting.cginc"
				#include "AutoLight.cginc"
				#include "UnityShaderVariables.cginc"
				#define ASE_NEEDS_FRAG_COLOR


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					float4 ase_texcoord3 : TEXCOORD3;
					float4 ase_texcoord4 : TEXCOORD4;
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				//This is a late directive
				
				uniform float4 _Main_Color;
				uniform float4 _Light_Color;
				uniform sampler2D _Texture01;
				uniform float _Panner_Time;
				uniform sampler2D _Texture02;
				uniform float4 _CameraDepthTexture_TexelSize;
				uniform float _DepthFade;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.ase_texcoord3.xyz = ase_worldPos;
					float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
					float4 screenPos = ComputeScreenPos(ase_clipPos);
					o.ase_texcoord4 = screenPos;
					
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord3.w = 0;

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float2 texCoord91 = i.texcoord.xy * float2( 2,2 ) + float2( -1,-1 );
					float dotResult94 = dot( texCoord91 , texCoord91 );
					float temp_output_96_0 = saturate( ( 1.0 - dotResult94 ) );
					float3 appendResult93 = (float3(texCoord91 , sqrt( temp_output_96_0 )));
					float3 ase_worldPos = i.ase_texcoord3.xyz;
					float3 worldSpaceLightDir = UnityWorldSpaceLightDir(ase_worldPos);
					float3 worldToViewDir98 = mul( UNITY_MATRIX_V, float4( worldSpaceLightDir, 0 ) ).xyz;
					float dotResult60 = dot( appendResult93 , worldToViewDir98 );
					float4 lerpResult138 = lerp( _Main_Color , _Light_Color , saturate( dotResult60 ));
					float smoothstepResult132 = smoothstep( 0.0 , 1.0 , temp_output_96_0);
					float2 texCoord244 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float2 panner245 = ( ( _Time.y * _Panner_Time ) * float2( 1,1 ) + texCoord244);
					float smoothstepResult266 = smoothstep( 0.0 , 1.0 , ( tex2D( _Texture01, panner245 ).r + tex2D( _Texture02, panner245 ).r ));
					float myVarName113 = ( smoothstepResult132 * smoothstepResult266 * i.color.a );
					float4 screenPos = i.ase_texcoord4;
					float4 ase_screenPosNorm = screenPos / screenPos.w;
					ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
					float screenDepth141 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
					float distanceDepth141 = saturate( abs( ( screenDepth141 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _DepthFade ) ) );
					float4 appendResult57 = (float4(( lerpResult138 * i.color ).rgb , ( myVarName113 * distanceDepth141 )));
					

					fixed4 col = appendResult57;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18935
224;73;1418;655;548.9428;-22.88324;1;True;True
Node;AmplifyShaderEditor.TextureCoordinatesNode;91;-1656.731,-261.8603;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;2,2;False;1;FLOAT2;-1,-1;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;241;-2575.888,288.3938;Inherit;False;Property;_Panner_Time;Panner_Time;2;0;Create;True;0;0;0;False;0;False;2;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;242;-2604.652,127.6485;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;94;-1416.343,-201.947;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;243;-2608.779,-33.13204;Inherit;False;Constant;_Vector3;Vector 3;8;0;Create;True;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;240;-2399.915,169.9496;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;244;-2642.621,-182.0311;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;95;-1188.082,-116.0645;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;245;-2366.818,12.55464;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;96;-975.7005,62.09164;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;262;-2113.139,-56.89603;Inherit;True;Property;_Texture01;Texture01;4;0;Create;True;0;0;0;False;0;False;-1;ba78417a147b35a4fbb11055d4b0d049;adf36d1d6b16e3c4b92e7852d69b7572;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;249;-2083.7,157.3278;Inherit;True;Property;_Texture02;Texture02;3;0;Create;True;0;0;0;False;0;False;-1;adf36d1d6b16e3c4b92e7852d69b7572;ba78417a147b35a4fbb11055d4b0d049;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;265;-1607.928,159.119;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;59;-307.6586,160.6112;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SqrtOpNode;97;-743.9264,43.38238;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;266;-1320.309,178.642;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TransformDirectionNode;98;-79.91333,145.0019;Inherit;False;World;View;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;93;-427.23,-88.23133;Inherit;True;FLOAT3;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexColorNode;140;-839.1498,337.0874;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;132;-760.8359,144.8582;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;60;156.9147,-45.82563;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;128;-553.4271,143.6897;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;113;-338.9732,397.7806;Inherit;True;myVarName;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;145;244.1665,-492.7159;Inherit;False;Property;_Main_Color;Main_Color;1;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;3.856125,1.25182,0.200082,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;268;123.0572,485.8832;Inherit;False;Property;_DepthFade;Depth Fade;5;0;Create;True;0;0;0;False;0;False;3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;246;216.5776,-286.5244;Inherit;False;Property;_Light_Color;Light_Color;0;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;5.189172,2.348276,1.199384,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;99;391.8563,-26.25753;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;139;605.6599,-544.9893;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;114;243.4802,219.9096;Inherit;True;113;myVarName;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;141;353.4576,434.0152;Inherit;False;True;True;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;138;551.4114,-170.9505;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;267;648.8837,300.8896;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;146;775.0886,-96.20195;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;57;895.9785,26.70329;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;151;1152.119,29.58403;Float;False;True;-1;2;ASEMaterialInspector;0;9;Smoke_Shader01;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;False;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;True;True;True;True;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;94;0;91;0
WireConnection;94;1;91;0
WireConnection;240;0;242;2
WireConnection;240;1;241;0
WireConnection;95;0;94;0
WireConnection;245;0;244;0
WireConnection;245;2;243;0
WireConnection;245;1;240;0
WireConnection;96;0;95;0
WireConnection;262;1;245;0
WireConnection;249;1;245;0
WireConnection;265;0;262;1
WireConnection;265;1;249;1
WireConnection;97;0;96;0
WireConnection;266;0;265;0
WireConnection;98;0;59;0
WireConnection;93;0;91;0
WireConnection;93;2;97;0
WireConnection;132;0;96;0
WireConnection;60;0;93;0
WireConnection;60;1;98;0
WireConnection;128;0;132;0
WireConnection;128;1;266;0
WireConnection;128;2;140;4
WireConnection;113;0;128;0
WireConnection;99;0;60;0
WireConnection;141;0;268;0
WireConnection;138;0;145;0
WireConnection;138;1;246;0
WireConnection;138;2;99;0
WireConnection;267;0;114;0
WireConnection;267;1;141;0
WireConnection;146;0;138;0
WireConnection;146;1;139;0
WireConnection;57;0;146;0
WireConnection;57;3;267;0
WireConnection;151;0;57;0
ASEEND*/
//CHKSM=15D6AF691667201E371FAB1E8EE20C38531457C7
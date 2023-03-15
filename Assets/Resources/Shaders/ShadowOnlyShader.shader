// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'PositionFog()' with transforming position into clip space.
// Upgrade NOTE: replaced 'V2F_POS_FOG' with 'float4 pos : SV_POSITION'
//https://github.com/keijiro/ShadowDrawer
 


Shader "FX/Matte Shadow" {

       Properties
    {
        _ShadowIntensity ("Shadow Intensity", Range (0, 1)) = 0.6
    }
 
 
    SubShader
    {
 
        Tags {"Queue"="AlphaTest" }
 
        Pass
        {
            Tags {"LightMode" = "ForwardBase" }
            Cull Back
            ZWrite off  
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
 
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            uniform float _ShadowIntensity;
 
            struct v2f
            {
                float4 pos : SV_POSITION;
                LIGHTING_COORDS(0,1)
            };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos (v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o);
               
                return o;
            }
            fixed4 frag(v2f i) : COLOR
            {
                float attenuation = LIGHT_ATTENUATION(i);
                return fixed4(0,0,0,(1-attenuation)*_ShadowIntensity);
            }
            ENDCG
        }
 
    }
    Fallback "VertexLit"
}

// Properties
//     { 
//         _ShadowColor("Shadow Color", Color) = (0.35,0.4,0.45,1.0)
//     }

//         SubShader
//     {
//         Tags
//         {
//             "RenderPipeline" = "UniversalPipeline"
//             "RenderType" = "Transparent"
//             "Queue" = "Transparent-1"
//         }

//         Pass
//         {
//             Name "ForwardLit"
//             Tags { "LightMode" = "UniversalForward" }

//             Blend DstColor Zero, Zero One
//             Cull Back
//             ZTest LEqual
//             ZWrite Off

//             HLSLPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag

//             #pragma prefer_hlslcc gles
//             #pragma exclude_renderers d3d11_9x
//             #pragma target 2.0

//             #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
//             #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
//             #pragma multi_compile _ _SHADOWS_SOFT
//             #pragma multi_compile_fog

//             #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

//             CBUFFER_START(UnityPerMaterial)
//             float4 _ShadowColor;
//             CBUFFER_END

//             struct Attributes
//             {
//                 float4 positionOS : POSITION;
//                 UNITY_VERTEX_INPUT_INSTANCE_ID
//             };

//             struct Varyings
//             {
//                 float4 positionCS               : SV_POSITION;
//                 float3 positionWS               : TEXCOORD0;
//                 float fogCoord : TEXCOORD1;
//                 UNITY_VERTEX_INPUT_INSTANCE_ID
//                 UNITY_VERTEX_OUTPUT_STEREO
//             };

//             Varyings vert(Attributes input)
//             {
//                 Varyings output = (Varyings)0;

//                 UNITY_SETUP_INSTANCE_ID(input);
//                 UNITY_TRANSFER_INSTANCE_ID(input, output);
//                 UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

//                 VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
//                 output.positionCS = vertexInput.positionCS;
//                 output.positionWS = vertexInput.positionWS;
//                 output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);

//                 return output;
//             }

//             half4 frag(Varyings input) : SV_Target
//             {
//                 UNITY_SETUP_INSTANCE_ID(input);
//                 UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

//                 half4 color = half4(1,1,1,1);

//             #ifdef _MAIN_LIGHT_SHADOWS
//                 VertexPositionInputs vertexInput = (VertexPositionInputs)0;
//                 vertexInput.positionWS = input.positionWS;

//                 float4 shadowCoord = GetShadowCoord(vertexInput);
//                 half shadowAttenutation = MainLightRealtimeShadow(shadowCoord);
//                 color = lerp(half4(1,1,1,1), _ShadowColor, (1.0 - shadowAttenutation) * _ShadowColor.a);
//                 color.rgb = MixFogColor(color.rgb, half3(1,1,1), input.fogCoord);
//             #endif
//                 return color;
//             }

//             ENDHLSL
//         }
//     }
// }


// Properties
// {
//      _ShadowStrength ("Shadow Strength", Range (0, 1)) = 1
// }
// SubShader
// {
//     Tags
//     {
//         "Queue"="AlphaTest"
//         "IgnoreProjector"="True"
//         "RenderType"="Transparent"
//     }
//     Pass
//     {
//         Blend SrcAlpha OneMinusSrcAlpha
//         Name "ShadowPass"
//         Tags {"LightMode" = "ForwardBase"}
 
//         CGPROGRAM
//         #pragma vertex vert
//         #pragma fragment frag
//         #pragma multi_compile_fwdbase
     
//         #include "UnityCG.cginc"
//         #include "AutoLight.cginc"
//         struct v2f
//         {
//             float4 pos : SV_POSITION;
//             LIGHTING_COORDS(0,1)
//         };
     
//         fixed _ShadowStrength;
//         v2f vert (appdata_full v)
//         {
//             v2f o;
//             o.pos = UnityObjectToClipPos (v.vertex);
//             TRANSFER_VERTEX_TO_FRAGMENT(o);
//             return o;
//         }
//         fixed4 frag (v2f i) : COLOR
//         {
//             fixed atten = LIGHT_ATTENUATION(i);
//             fixed shadowalpha = (1.0 - atten) * _ShadowStrength;
//             return fixed4(0.0, 0.0, 0.0, shadowalpha);
//         }
//         ENDCG
//     }
//     UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
// }
// }

// Properties
//     {
//         _TextureMap ("Texture", 2D) = "" {}
//     }
//     SubShader
//     {
//         Pass
//         {
//             Tags {"LightMode" = "ForwardBase"}
//             CGPROGRAM
//             #pragma vertex VSMain
//             #pragma fragment PSMain
//             #pragma multi_compile_fwdbase
//             #include "AutoLight.cginc"
           
//             sampler2D _TextureMap;
 
//             struct SHADERDATA
//             {
//                 float4 position : SV_POSITION;
//                 float2 uv : TEXCOORD0;
//                 float4 _ShadowCoord : TEXCOORD1;
//             };
 
//             float4 ComputeScreenPos (float4 p)
//             {
//                 float4 o = p * 0.5;
//                 return float4(float2(o.x, o.y*_ProjectionParams.x) + o.w, p.zw);
//             }
 
//             SHADERDATA VSMain (float4 vertex:POSITION, float2 uv:TEXCOORD0)
//             {
//                 SHADERDATA vs;
//                 vs.position = UnityObjectToClipPos(vertex);
//                 vs.uv = uv;
//                 vs._ShadowCoord = ComputeScreenPos(vs.position);
//                 return vs;
//             }
 
//             float4 PSMain (SHADERDATA ps) : SV_TARGET
//             {
//                 return lerp(float4(0,0,0,1), tex2D(_TextureMap, ps.uv), step(0.5, SHADOW_ATTENUATION(ps)));
//             }
           
//             ENDCG
//         }
       
//         Pass
//         {
//             Tags{ "LightMode" = "ShadowCaster" }
//             CGPROGRAM
//             #pragma vertex VSMain
//             #pragma fragment PSMain
 
//             float4 VSMain (float4 vertex:POSITION) : SV_POSITION
//             {
//                 return UnityObjectToClipPos(vertex);
//             }
 
//             float4 PSMain (float4 vertex:SV_POSITION) : SV_TARGET
//             {
//                 return 0;
//             }
           
//             ENDCG
//         }
//     }
// }
 
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Particle_Additive_TintColor"
{
    Properties {
        _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
        _MainTex ("Particle Texture", 2D) = "white" {}
    }

    Category {
        Tags { "Queue"="Transparent"   "RenderType"="Transparent" }
        Blend SrcAlpha One
        AlphaTest Greater .01
        ColorMask RGB
        Cull Back Lighting Off ZWrite Off Fog { Mode Off }
        BindChannels {
            Bind "Color", color
            Bind "Vertex", vertex
            Bind "TexCoord", texcoord
        }
        
        // ---- Dual texture cards
        SubShader {
            Pass {
                SetTexture [_MainTex] {
                    constantColor [_TintColor]
                    combine constant * primary
                }
                SetTexture [_MainTex] {
                    combine texture * previous DOUBLE
                }
            }
        }
        
        // ---- Single texture cards (does not do color tint)
        SubShader {
            Pass {
                SetTexture [_MainTex] {
                    combine texture * primary
                }
            }
        }
    }
}
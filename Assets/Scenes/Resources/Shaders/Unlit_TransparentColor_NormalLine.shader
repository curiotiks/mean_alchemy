Shader "Unlit/Unlit_TransparentColor_NormalLine"
{   //https://gist.github.com/keijiro/1681052
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    }

    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        
        // Cull Off
        // ZWrite Off
        Lighting Off 
        Fog { Mode Off }

        Blend SrcAlpha OneMinusSrcAlpha 

        Pass {
            Color [_Color]
            SetTexture [_MainTex] { combine texture * primary } 
        }
    }
}
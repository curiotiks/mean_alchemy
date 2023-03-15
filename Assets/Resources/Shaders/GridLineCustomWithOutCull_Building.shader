Shader "Custom/GridLineCustomWithOutCull_Building" {
 Properties {
     _Color ("Color Tint", Color) = (1,1,1,1)    
     _MainTex ("Base (RGB) Alpha (A)", 2D) = "white"
 }

    Category 
    {
        Lighting Off
        ZTest Always
        ZWrite Off
        //ZWrite On  // uncomment if you have problems like the sprite disappear in some rotations.
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        //AlphaTest Greater 0.001  // uncomment if you have problems like the sprites or 3d text have white quads instead of alpha pixels.
        Tags {Queue=Transparent}

        SubShader 
        {
            Pass 
            {
                SetTexture [_MainTex] 
                {
                    ConstantColor [_Color]
                    Combine Texture * constant
                }
            }
        }
    }

}

//  SubShader {
//     Cull Off
//     Pass{
//         ZTest Greater
//         }
//     Pass{
//         ZTest Less
//     }
//     Pass{
//         ZTest Always
//     }
//         Blend SrcAlpha OneMinusSrcAlpha
//             //AlphaTest Greater 0.001  // uncomment if you have problems like the sprites or 3d text have white quads instead of alpha pixels.
//         Tags {Queue=Transparent}
//         //  Tags { "RenderType"="Opaque" }
//         LOD 200
        
//         SetTexture [_MainTex] 
//                     {
//                         ConstantColor [_Color]
//                         Combine Texture * constant
//                     }
//     } 
//     FallBack "Diffuse"
// }
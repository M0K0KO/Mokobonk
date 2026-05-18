Shader "MokoVAT/VAT Lit"
{
    Properties
    {
        _BaseMap   ("Base Map", 2D)    = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        _Smoothness ("Smoothness", Range(0,1)) = 0.2
        _Metallic   ("Metallic",   Range(0,1)) = 0.0

        _PositionTex ("VAT Position", 2D) = "black" {}
        _NormalTex   ("VAT Normal",   2D) = "black" {}

        _TextureWidth     ("Texture Width",  Float) = 256
        _TextureHeight    ("Texture Height", Float) = 1
        _RowsPerFrame     ("Rows Per Frame", Float) = 1
        _TotalFrameCount  ("Total Frame Count", Float) = 1
        _FPS              ("FPS", Float) = 30
        _ClipStartFrame   ("Clip Start Frame", Float) = 0
        _ClipFrameCount   ("Clip Frame Count", Float) = 1
        _ClipStartTime    ("Clip Start Time", Float) = 0

        _PrevClipStartFrame  ("Prev Clip Start Frame", Float)  = 0
        _PrevClipFrameCount  ("Prev Clip Frame Count", Float)  = 1
        _PrevClipStartTime   ("Prev Clip Start Time", Float)  = 0
        _BlendFactor         ("Blend Factor (0=prev, 1=curr)", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 300

        // ---------------- Forward ----------------
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target 4.5

            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float  _Smoothness;
                float  _Metallic;

                float  _TextureWidth;
                float  _TextureHeight;
                float  _RowsPerFrame;
                float  _TotalFrameCount;
                float  _FPS;
                float  _ClipStartFrame;
                float  _ClipFrameCount;
                float  _ClipStartTime;

                float  _PrevClipStartFrame;
                float  _PrevClipFrameCount;
                float  _PrevClipStartTime;
                float  _BlendFactor;  
            CBUFFER_END

            #ifdef UNITY_DOTS_INSTANCING_ENABLED
            UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP(float, _ClipStartFrame)
                UNITY_DOTS_INSTANCED_PROP(float, _ClipFrameCount)
                UNITY_DOTS_INSTANCED_PROP(float, _ClipStartTime)
                UNITY_DOTS_INSTANCED_PROP(float, _PrevClipStartFrame)
                UNITY_DOTS_INSTANCED_PROP(float, _PrevClipFrameCount)
                UNITY_DOTS_INSTANCED_PROP(float, _PrevClipStartTime)
                UNITY_DOTS_INSTANCED_PROP(float, _BlendFactor)
            UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

            #define _ClipStartFrame      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _ClipStartFrame)
            #define _ClipFrameCount      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _ClipFrameCount)
            #define _ClipStartTime       UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _ClipStartTime)
            #define _PrevClipStartFrame  UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _PrevClipStartFrame)
            #define _PrevClipFrameCount  UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _PrevClipFrameCount)
            #define _PrevClipStartTime   UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _PrevClipStartTime)
            #define _BlendFactor         UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _BlendFactor)
            #endif

            #include "VATCommon.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                float2 vatID      : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float2 uv          : TEXCOORD2;
                float  fogCoord    : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                Varyings OUT;

                float3 posOS, nrmOS;
                SampleVAT(IN.vatID, _Time.y, posOS, nrmOS);

                VertexPositionInputs vpi = GetVertexPositionInputs(posOS);
                VertexNormalInputs   vni = GetVertexNormalInputs(nrmOS, IN.tangentOS);

                OUT.positionHCS = vpi.positionCS;
                OUT.positionWS  = vpi.positionWS;
                OUT.normalWS    = vni.normalWS;
                OUT.uv          = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.fogCoord    = ComputeFogFactor(vpi.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                SurfaceData surface = (SurfaceData)0;
                surface.albedo      = baseSample.rgb;
                surface.alpha       = 1;
                surface.metallic    = _Metallic;
                surface.smoothness  = _Smoothness;
                surface.occlusion   = 1;
                surface.normalTS    = float3(0,0,1);
                surface.emission    = 0;

                InputData input = (InputData)0;
                input.positionWS        = IN.positionWS;
                input.normalWS          = normalize(IN.normalWS);
                input.viewDirectionWS   = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                input.shadowCoord       = TransformWorldToShadowCoord(IN.positionWS);
                input.fogCoord          = IN.fogCoord;
                input.bakedGI           = SampleSH(input.normalWS);

                half4 color = UniversalFragmentPBR(input, surface);
                color.rgb = MixFog(color.rgb, IN.fogCoord);
                return color;
            }
            ENDHLSL
        }

        // ---------------- Shadow ----------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex   shadowVert
            #pragma fragment shadowFrag
            #pragma target 4.5

            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float  _Smoothness;
                float  _Metallic;

                float  _TextureWidth;
                float  _TextureHeight;
                float  _RowsPerFrame;
                float  _TotalFrameCount;
                float  _FPS;
                float  _ClipStartFrame;
                float  _ClipFrameCount;
                float  _ClipStartTime;

                float  _PrevClipStartFrame;
                float  _PrevClipFrameCount;
                float  _PrevClipStartTime;
                float  _BlendFactor;  

            CBUFFER_END

            #ifdef UNITY_DOTS_INSTANCING_ENABLED
            UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP(float, _ClipStartFrame)
                UNITY_DOTS_INSTANCED_PROP(float, _ClipFrameCount)
                UNITY_DOTS_INSTANCED_PROP(float, _ClipStartTime)
                UNITY_DOTS_INSTANCED_PROP(float, _PrevClipStartFrame)
                UNITY_DOTS_INSTANCED_PROP(float, _PrevClipFrameCount)
                UNITY_DOTS_INSTANCED_PROP(float, _PrevClipStartTime)
                UNITY_DOTS_INSTANCED_PROP(float, _BlendFactor)
            UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

            #define _ClipStartFrame      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _ClipStartFrame)
            #define _ClipFrameCount      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _ClipFrameCount)
            #define _ClipStartTime       UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _ClipStartTime)
            #define _PrevClipStartFrame  UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _PrevClipStartFrame)
            #define _PrevClipFrameCount  UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _PrevClipFrameCount)
            #define _PrevClipStartTime   UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _PrevClipStartTime)
            #define _BlendFactor         UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _BlendFactor)
            #endif

            #include "VATCommon.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct A { float3 positionOS:POSITION; float3 normalOS:NORMAL; float2 vatID:TEXCOORD1; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 positionHCS:SV_POSITION; };

            float4 GetShadowPositionHClip(float3 posOS, float3 nrmOS)
            {
                float3 positionWS = TransformObjectToWorld(posOS);
                float3 normalWS   = TransformObjectToWorldNormal(nrmOS);

            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
            #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif
                return positionCS;
            }

            V shadowVert(A IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                float3 posOS, nrmOS;
                SampleVAT(IN.vatID, _Time.y, posOS, nrmOS);

                V OUT;
                OUT.positionHCS = GetShadowPositionHClip(posOS, nrmOS);
                return OUT;
            }

            half4 shadowFrag(V IN) : SV_Target { return 0; }
            ENDHLSL
        }

        // ---------------- DepthOnly ----------------
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex   depthVert
            #pragma fragment depthFrag
            #pragma target 4.5

            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float  _Smoothness;
                float  _Metallic;

                float  _TextureWidth;
                float  _TextureHeight;
                float  _RowsPerFrame;
                float  _TotalFrameCount;
                float  _FPS;
                float  _ClipStartFrame;
                float  _ClipFrameCount;
                float  _ClipStartTime;

                float  _PrevClipStartFrame;
                float  _PrevClipFrameCount;
                float  _PrevClipStartTime;
                float  _BlendFactor;  
            CBUFFER_END

            #ifdef UNITY_DOTS_INSTANCING_ENABLED
            UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP(float, _ClipStartFrame)
                UNITY_DOTS_INSTANCED_PROP(float, _ClipFrameCount)
                UNITY_DOTS_INSTANCED_PROP(float, _ClipStartTime)
                UNITY_DOTS_INSTANCED_PROP(float, _PrevClipStartFrame)
                UNITY_DOTS_INSTANCED_PROP(float, _PrevClipFrameCount)
                UNITY_DOTS_INSTANCED_PROP(float, _PrevClipStartTime)
                UNITY_DOTS_INSTANCED_PROP(float, _BlendFactor)
            UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

            #define _ClipStartFrame      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _ClipStartFrame)
            #define _ClipFrameCount      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _ClipFrameCount)
            #define _ClipStartTime       UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _ClipStartTime)
            #define _PrevClipStartFrame  UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _PrevClipStartFrame)
            #define _PrevClipFrameCount  UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _PrevClipFrameCount)
            #define _PrevClipStartTime   UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _PrevClipStartTime)
            #define _BlendFactor         UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _BlendFactor)
            #endif

            #include "VATCommon.hlsl"

            struct A { float3 positionOS:POSITION; float3 normalOS:NORMAL; float2 vatID:TEXCOORD1; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 positionHCS:SV_POSITION; };

            V depthVert(A IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                float3 posOS, nrmOS;
                SampleVAT(IN.vatID, _Time.y, posOS, nrmOS);

                V OUT;
                OUT.positionHCS = TransformObjectToHClip(posOS);
                return OUT;
            }

            half4 depthFrag(V IN) : SV_Target { return 0; }
            ENDHLSL
        }

        // ---------------- DepthNormals ----------------
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormals" }

            ZWrite On

            HLSLPROGRAM
            #pragma vertex   dnVert
            #pragma fragment dnFrag
            #pragma target 4.5

            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float  _Smoothness;
                float  _Metallic;

                float  _TextureWidth;
                float  _TextureHeight;
                float  _RowsPerFrame;
                float  _TotalFrameCount;
                float  _FPS;
                float  _ClipStartFrame;
                float  _ClipFrameCount;
                float  _ClipStartTime;
                
                float  _PrevClipStartFrame;
                float  _PrevClipFrameCount;
                float  _PrevClipStartTime;
                float  _BlendFactor;  
            CBUFFER_END

            #ifdef UNITY_DOTS_INSTANCING_ENABLED
            UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP(float, _ClipStartFrame)
                UNITY_DOTS_INSTANCED_PROP(float, _ClipFrameCount)
                UNITY_DOTS_INSTANCED_PROP(float, _ClipStartTime)
                UNITY_DOTS_INSTANCED_PROP(float, _PrevClipStartFrame)
                UNITY_DOTS_INSTANCED_PROP(float, _PrevClipFrameCount)
                UNITY_DOTS_INSTANCED_PROP(float, _PrevClipStartTime)
                UNITY_DOTS_INSTANCED_PROP(float, _BlendFactor)
            UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

            #define _ClipStartFrame      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _ClipStartFrame)
            #define _ClipFrameCount      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _ClipFrameCount)
            #define _ClipStartTime       UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _ClipStartTime)
            #define _PrevClipStartFrame  UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _PrevClipStartFrame)
            #define _PrevClipFrameCount  UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _PrevClipFrameCount)
            #define _PrevClipStartTime   UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _PrevClipStartTime)
            #define _BlendFactor         UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _BlendFactor)
            #endif

            #include "VATCommon.hlsl"

            struct A { float3 positionOS:POSITION; float3 normalOS:NORMAL; float2 vatID:TEXCOORD1; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 positionHCS:SV_POSITION; float3 normalWS:TEXCOORD0; };

            V dnVert(A IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                float3 posOS, nrmOS;
                SampleVAT(IN.vatID, _Time.y, posOS, nrmOS);

                V OUT;
                OUT.positionHCS = TransformObjectToHClip(posOS);
                OUT.normalWS    = TransformObjectToWorldNormal(nrmOS);
                return OUT;
            }

            half4 dnFrag(V IN) : SV_Target
            {
                return half4(normalize(IN.normalWS) * 0.5 + 0.5, 0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}

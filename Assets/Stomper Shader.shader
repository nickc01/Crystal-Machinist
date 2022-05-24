// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/Stomper Shader"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
        [PerRendererData] _BlurSamples("Blur Samples", Int) = 0
        _BlurIncrements("Blur Increments", Float) = 0.02
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Cull Off
            Lighting Off
            ZWrite Off
            Blend One OneMinusSrcAlpha

            CGPROGRAM
            #pragma surface surf Lambert vertex:vert nofog nolightmap nodynlightmap keepalpha noinstancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            struct Input
            {
                float2 uv_MainTex;
                fixed4 color;
            };

            int _BlurSamples;
            float _BlurIncrements;

            fixed4 verticalBlur(fixed4 initialSample, float2 uv, fixed4 color/*, int samplesSquared, float incrementSize*/)
            {
                int i = 1;
                for (i = 1; i <= _BlurSamples; i++)
                {
                    initialSample += SampleSpriteTexture(float2(uv.x, uv.y + (_BlurIncrements * i))) * color;
                }

                for (i = 1; i <= _BlurSamples; i++)
                {
                    initialSample += SampleSpriteTexture(float2(uv.x, uv.y - (_BlurIncrements * i))) * color;
                }

                return initialSample / ((_BlurSamples * 2) + 1);
            }

            void vert(inout appdata_full v, out Input o)
            {
                v.vertex = UnityFlipSprite(v.vertex, _Flip);

                #if defined(PIXELSNAP_ON)
                v.vertex = UnityPixelSnap(v.vertex);
                #endif

                UNITY_INITIALIZE_OUTPUT(Input, o);
                o.color = v.color * _Color * _RendererColor;
            }

            void surf(Input IN, inout SurfaceOutput o)
            {
                fixed4 c = SampleSpriteTexture(IN.uv_MainTex) * IN.color;
                c = verticalBlur(c,IN.uv_MainTex,IN.color);
                o.Albedo = c.rgb * c.a;
                o.Alpha = c.a;
            }
            ENDCG
        }

            Fallback "Sprites/Diffuse"
}

/*Shader "Sprites/Lit" {
    Properties {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Vector) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Vector) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    //DummyShader
    SubShader{
        Tags { "RenderType" = "Opaque" }
        LOD 200
        CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma target 3.0
        sampler2D _MainTex;
        struct Input
        {
            float2 uv_MainTex;
        };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
        }
        ENDCG
    }
    Fallback "Sprites/Diffuse"
}*/
Shader "Custom/ObstacleShader"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseMap ("Albedo", 2D) = "white" {}

        _FadeRadiusOuter ("Fade Circle Outer Radius", Range(0.0, 1.0)) = 0.4
        _FadeRadiusInner ("Fade Circle Inner Radius", Range(0.0, 1.0)) = 0.15
        _FadeDistOuter ("Fade Start Distance", Float) = 5.0
        _FadeDistInner ("Fade End Distance", Float) = 1.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
            ZWrite On
            ColorMask 0
        }
        
        CGPROGRAM
        #pragma surface surf Standard alpha:fade
        #pragma target 3.0

        sampler2D _BaseMap;
        float _Metallic;
        float _Smoothness;
        float4 _BaseColor;
        float  _FadeRadiusInner;
        float  _FadeRadiusOuter;
        float  _FadeDistOuter;
        float  _FadeDistInner;

        struct Input
        {
            float2 uv_BaseMap;
            float4 screenPos;
            float3 worldPos;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_BaseMap, IN.uv_BaseMap) * _BaseColor;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;

            // === Screen-space radial distances ===
            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float aspect = _ScreenParams.x / _ScreenParams.y;
            float2 centred = screenUV - float2(0.5, 0.5);
            centred.x *= aspect;
            float radialDist = length(centred);

            // === Depth factor (same as before) ===
            float camDist = distance(IN.worldPos, _WorldSpaceCameraPos);
            float depthFade = 1.0 - smoothstep(_FadeDistInner, _FadeDistOuter, camDist);

            // === Radial factor (new two-circle logic) ===
            float radialFade;
            if (radialDist >= _FadeRadiusOuter)
            {
                // Outside outer circle — fully opaque, kill everything
                radialFade = 0.0;
            }
            else if (radialDist <= _FadeRadiusInner)
            {
                // Inside inner circle — depth is the only factor
                radialFade = 1.0;
            }
            else
            {
                // In the ring between circles — blend radial position into it
                // This goes from 0 at the outer edge to 1 at the inner edge
                radialFade = 1.0 - smoothstep(_FadeRadiusInner, _FadeRadiusOuter, radialDist);
            }

            // === Combine ===
            float transparencyAmount = radialFade * depthFade;
            o.Alpha = c.a * _BaseColor.a * (1.0 - transparencyAmount);
        }
        ENDCG
    }

    FallBack "Diffuse"
}
Shader "Custom/ScreenShader"
{
    Properties
    {
        // --- Walls 1 Camera (Background) ---
        _Cam1_Walls_1 ("Walls 1 Feed (Background)", 2D) = "white" {}
        
        // ------------------------------------
        // --- Walls 2 Camera (Primary Inset) ---
        // ------------------------------------
        _Cam2_Walls_2 ("Walls 2 Feed (Primary Inset)", 2D) = "white" {}
        _SubX_2 ("Cam 2 Inset X (0-1)", Range(0, 1)) = 0.2
        _SubY_2 ("Cam 2 Inset Y (0-1)", Range(0, 1)) = 0.2
        _SubWidth_2 ("Cam 2 Inset Width (0-1)", Range(0, 1)) = 0.6
        _SubHeight_2 ("Cam 2 Inset Height (0-1)", Range(0, 1)) = 0.6

        // ------------------------------------
        // --- Walls 3 Camera (Secondary Inset) ---
        // ------------------------------------
        _Cam3_Walls_3 ("Walls 3 Feed (Secondary Inset)", 2D) = "white" {}
        _SubX_3 ("Cam 3 Inset X (0-1)", Range(0, 1)) = 0.3 // Default offset for visibility
        _SubY_3 ("Cam 3 Inset Y (0-1)", Range(0, 1)) = 0.3
        _SubWidth_3 ("Cam 3 Inset Width (0-1)", Range(0, 1)) = 0.4
        _SubHeight_3 ("Cam 3 Inset Height (0-1)", Range(0, 1)) = 0.4

        // --- Chroma Key Camera ---
        _Cam0_ChromaKeyTex ("Chroma Key Feed (Top Layer)", 2D) = "white" {}
        _ChromaKeyColor ("Chroma Key Color", Color) = (0, 0, 1, 1) // Blue
        _Tolerance ("Tolerance", Float) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            // --- Textures and Colors ---
            sampler2D _Cam1_Walls_1;
            sampler2D _Cam2_Walls_2;
            sampler2D _Cam3_Walls_3;
            sampler2D _Cam0_ChromaKeyTex;
            float4 _ChromaKeyColor;
            float _Tolerance;
            
            // --- Cam 2 Inset Parameters (Renamed) ---
            float _SubX_2;
            float _SubY_2;
            float _SubWidth_2;
            float _SubHeight_2;

            // --- Cam 3 Inset Parameters (New) ---
            float _SubX_3;
            float _SubY_3;
            float _SubWidth_3;
            float _SubHeight_3;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // -----------------------------------------------------------
                // --- Step 1: Determine the Base Wall Color (Cam1 or Cam2) ---
                // -----------------------------------------------------------
                float4 wallCompositeColor; // Starts as Cam1, may become Cam2

                // Calculate the boundaries for Cam 2 inset
                float insetMinX_2 = _SubX_2;
                float insetMaxX_2 = _SubX_2 + _SubWidth_2;
                float insetMinY_2 = _SubY_2;
                float insetMaxY_2 = _SubY_2 + _SubHeight_2;
                
                // Check if the current UV coordinate is inside the Cam 2 inset
                if (uv.x > insetMinX_2 && uv.x < insetMaxX_2 && 
                    uv.y > insetMinY_2 && uv.y < insetMaxY_2)
                {
                    // INSIDE CAM 2 INSET: Sample Camera 2
                    float2 subUV_2 = float2(
                        (uv.x - _SubX_2) / _SubWidth_2,
                        (uv.y - _SubY_2) / _SubHeight_2
                    );
                    wallCompositeColor = tex2D(_Cam2_Walls_2, subUV_2);
                }
                else
                {
                    // OUTSIDE CAM 2 INSET: Sample Camera 1
                    wallCompositeColor = tex2D(_Cam1_Walls_1, uv);
                }

                // -------------------------------------------------------------------------
                // --- Step 2: Layer Camera 3 (Walls 3) Inset on top of the Composite Color ---
                // -------------------------------------------------------------------------

                // Calculate the boundaries for Cam 3 inset
                float insetMinX_3 = _SubX_3;
                float insetMaxX_3 = _SubX_3 + _SubWidth_3;
                float insetMinY_3 = _SubY_3;
                float insetMaxY_3 = _SubY_3 + _SubHeight_3;

                // Check if the current UV coordinate is inside the Cam 3 inset
                if (uv.x > insetMinX_3 && uv.x < insetMaxX_3 && 
                    uv.y > insetMinY_3 && uv.y < insetMaxY_3)
                {
                    // INSIDE CAM 3 INSET: Sample Camera 3
                    float2 subUV_3 = float2(
                        (uv.x - _SubX_3) / _SubWidth_3,
                        (uv.y - _SubY_3) / _SubHeight_3
                    );
                    
                    // Walls 3 feed replaces the color within its inset boundaries
                    wallCompositeColor = tex2D(_Cam3_Walls_3, subUV_3);
                }

                // -----------------------------------------------------------
                // --- Step 3: Layer Camera 0 (Chroma Key) on top ---
                // -----------------------------------------------------------
                float4 chromaColor = tex2D(_Cam0_ChromaKeyTex, uv);
                
                // Chroma keying: calculate distance to the chroma key color
                float distance = length(chromaColor.rgb - _ChromaKeyColor.rgb);
                
                if (distance < _Tolerance)
                {
                    // Chroma key found (transparent): return the layered wall color
                    return wallCompositeColor; 
                }
                else
                {
                    // Chroma key NOT found (opaque): return the chroma key texture's color
                    return chromaColor;
                }
            }
            ENDCG
        }
    }
}
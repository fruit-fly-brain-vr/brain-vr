Shader "NeuronClippableShader"{
    //show values to edit in inspector
    Properties{
        _Color ("Neuron_Color", Color) = (0, 0, 0, 1)
        _MainTex ("Texture", 2D) = "white" {}
        //[HDR]_Emission ("Cutoff_Color", color) = (0,0,0,0)
        [HDR]_CutoffColor ("Cutoff Color", color) = (0,0,0,0)
    }

    SubShader{
        //the material is completely non-transparent and is rendered at the same time as the other opaque geometry
        Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

        // render faces regardless if they point towards the camera or away from it
        Cull Off

        CGPROGRAM
        //the shader is a surface shader, meaning that it will be extended by unity in the background
        //to have fancy lighting and other features
        //our surface shader function is called surf and we use our custom lighting model
        //fullforwardshadows makes sure unity adds the shadow passes the shader might need
        //vertex:vert makes the shader use vert as a vertex shader function
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        float4 _CutoffColor;

        float4 _PlaneLeft;
        float4 _PlaneRight;

        //input struct which is automatically filled by unity
        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
            float facing : VFACE;
        };

        //the surface shader function which sets parameters the lighting function then uses
        void surf (Input i, inout SurfaceOutputStandard o) {
       ////
            float distanceLeft = dot(i.worldPos, _PlaneLeft.xyz);
            distanceLeft = distanceLeft + _PlaneLeft.w;
            clip(distanceLeft);

            float distanceRight = dot(i.worldPos, _PlaneRight.xyz);
            distanceRight = distanceRight + _PlaneRight.w;
            clip(distanceRight);
       ////
            float facing = i.facing * 0.5 + 0.5;

            //normal color stuff
            fixed4 col = _Color;
            o.Albedo = col.rgb * facing;
            o.Emission = _CutoffColor;
        }
        ENDCG
    }
    FallBack "Standard" //fallback adds a shadow pass so we get shadows on other objects
}
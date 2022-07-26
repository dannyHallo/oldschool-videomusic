Shader "Unlit/crtShader"
{

    // This shader is written by ehj1
    // https://www.shadertoy.com/view/ldXGW4

    Properties
    {
        _MainTex ("tex2D", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            #define mod(x,y) (x-y*floor(x/y))
            // change these values to 0.0 to turn off individual effects
            static float vertJerkOpt = 1.0;
            static float vertMovementOpt = 1.0;
            float whiteNoiseMultiplier = 1.0;
            float rgbOffsetOptX;
            float rgbOffsetOptY;
            float scanlineNum;
            float scanlineMultiplier;
            float horzFuzzMultiplier;
            float horzFuzzFraquency;
            float horzFuzzDensity;
            float scrollLikelihood;
            static const float pi = 3.14159265;

            // Noise generation functions borrowed from: 
            // https://github.com/ashima/webgl-noise/blob/master/src/noise2D.glsl

            float3 mod289(float3 x) {
                return x - floor(x * (1.0 / 289.0)) * 289.0;
            }

            float2 mod289(float2 x) {
                return x - floor(x * (1.0 / 289.0)) * 289.0;
            }

            float3 permute(float3 x) {
                return mod289(((x*34.0)+1.0)*x);
            }

            float snoise(float2 v)
            {
                const float4 C = float4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
                0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
                -0.577350269189626,  // -1.0 + 2.0 * C.x
                0.024390243902439); // 1.0 / 41.0
                // First corner
                float2 i  = floor(v + dot(v, C.yy) );
                float2 x0 = v -   i + dot(i, C.xx);

                // Other corners
                float2 i1;
                //i1.x = step( x0.y, x0.x ); // x0.x > x0.y ? 1.0 : 0.0
                //i1.y = 1.0 - i1.x;
                i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
                // x0 = x0 - 0.0 + 0.0 * C.xx ;
                // x1 = x0 - i1 + 1.0 * C.xx ;
                // x2 = x0 - 1.0 + 2.0 * C.xx ;
                float4 x12 = x0.xyxy + C.xxzz;
                x12.xy -= i1;

                // Permutations
                i = mod289(i); // Avoid truncation effects in permutation
                float3 p = permute( permute( i.y + float3(0.0, i1.y, 1.0 ))
                + i.x + float3(0.0, i1.x, 1.0 ));

                float3 m = max(0.5 - float3(dot(x0,x0), dot(x12.xy,x12.xy), dot(x12.zw,x12.zw)), 0.0);
                m = m*m ;
                m = m*m ;

                // Gradients: 41 points uniformly over a line, mapped onto a diamond.
                // The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)

                float3 x = 2.0 * frac(p * C.www) - 1.0;
                float3 h = abs(x) - 0.5;
                float3 ox = floor(x + 0.5);
                float3 a0 = x - ox;

                // Normalise gradients implicitly by scaling m
                // Approximation of: m *= inversesqrt( a0*a0 + h*h );
                m *= 1.79284291400159 - 0.85373472095314 * ( a0*a0 + h*h );

                // Compute final noise value at P
                float3 g;
                g.x  = a0.x  * x0.x  + h.x  * x0.y;
                g.yz = a0.yz * x12.xz + h.yz * x12.yw;
                return 130.0 * dot(m, g);
            }

            float staticV(float2 uv) {
                float staticHeight = snoise(float2(uv.y * 100.0, _Time.y * 1.2)) * 0.3 + 5.0;
                float staticAmount = snoise(float2(2.0, _Time.y * 1.2)) * 0.1 + 0.3;
                float staticStrength = snoise(float2(3.0, _Time.y * 0.6)) * 2.0 + 4.0;

                return (1.0 - step(
                snoise(float2(5.0 * _Time.y + uv.x * 20.0,
                pow((mod(_Time.y, 10.0) * 1.0 + 10.0) * 0.3 + 3.0, staticHeight))), 
                staticAmount)) * staticStrength;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv =  i.uv;
                
                // float jerkOffset = (1.0-step(snoise(float2(_Time.y*1.3,5.0)),0.8))*0.05;
                
                // Continuous noises on x
                float fuzzOffset = snoise(float2(_Time.y * 15.0 * horzFuzzFraquency, uv.y * 240.0 * horzFuzzDensity)) * 0.001;
                float largeFuzzOffset = snoise(float2(_Time.y * 2.0 * horzFuzzFraquency, uv.y * 100.0 * horzFuzzDensity)) * 0.004;
                float xOffset = (fuzzOffset + largeFuzzOffset) * horzFuzzMultiplier;
                
                // Movement in vertical, 0 or another fixed val
                float vertMovementOn = (1.0 - step(snoise(float2(_Time.y * 0.2, 8.0)), scrollLikelihood)) * vertMovementOpt;
                float vertJerk = (1.0 - step(snoise(float2(_Time.y * 1.5, 5.0)), 0.6)) * vertJerkOpt;
                float vertJerk2 = (1.0 - step(snoise(float2(_Time.y * 5.5, 5.0)), 0.2)) * vertJerkOpt;
                float yOffset = abs(sin(_Time.y) * 4.0) * vertMovementOn + vertJerk * vertJerk2 * 0.3;
                float y = mod(uv.y + yOffset, 1.0);
                
                float staticVal = 0.0;
                for (float j = -1.0; j <= 1.0; j += 1.0) {
                    float maxDist = 0.025;
                    float dist = j * 0.005;
                    staticVal += staticV(float2(uv.x + xOffset, y + dist)) * (maxDist-abs(dist)) * 1.5;
                }
                
                staticVal *= whiteNoiseMultiplier;
                rgbOffsetOptY = rgbOffsetOptX * 0.6;

                float red 	=   tex2D(	_MainTex, 	float2(uv.x + xOffset - 0.01 * rgbOffsetOptX,   y - 0.01 * rgbOffsetOptY)).r  + staticVal;
                float green = 	tex2D(	_MainTex, 	float2(uv.x + xOffset,                          y)).g                         + staticVal;
                float blue 	=	tex2D(	_MainTex, 	float2(uv.x + xOffset + 0.01 * rgbOffsetOptX,   y + 0.01 * rgbOffsetOptY)).b  + staticVal;
                
                float3 color = float3(red,green,blue);

                float scanline = sin(2 * pi * (uv.y * scanlineNum)) * 0.04 * scanlineMultiplier;
                color += scanline;
                
                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
